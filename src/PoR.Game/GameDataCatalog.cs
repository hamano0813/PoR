using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using PoR.Core.Game;
using PoR.Core.Values;

namespace PoR.Game;

/// <summary>
/// Loads the embedded JSON game catalog.
/// </summary>
public sealed class GameDataCatalog
{
    private static readonly Lazy<GameDataCatalog> LazyDefault = new(LoadDefault);
    private static readonly HashSet<string> SpecialEnumIds =
        new(StringComparer.Ordinal) { "group", "army", "ally" };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private GameDataCatalog(
        GameProfile profile,
        IReadOnlyDictionary<string, FieldDefinition> fields,
        IReadOnlyDictionary<string, EnumDefinition> enums,
        IReadOnlyDictionary<string, PointerMappingDefinition> pointerMappings,
        IReadOnlyDictionary<string, TextMappingDefinition> textMappings,
        IReadOnlyList<GroupEntry> groups,
        IReadOnlyList<ArmyEntry> armies,
        SupportCatalog supports,
        AffinityCatalog affinities,
        SkillCatalog skills)
    {
        Profile = profile;
        Fields = fields;
        Enums = enums;
        PointerMappings = pointerMappings;
        TextMappings = textMappings;
        Groups = groups;
        Armies = armies;
        Supports = supports;
        Affinities = affinities;
        Skills = skills;
    }

    public static GameDataCatalog Default => LazyDefault.Value;

    public GameProfile Profile { get; }

    public IReadOnlyDictionary<string, FieldDefinition> Fields { get; }

    public IReadOnlyDictionary<string, EnumDefinition> Enums { get; }

    public IReadOnlyDictionary<string, PointerMappingDefinition> PointerMappings { get; }

    public IReadOnlyDictionary<string, TextMappingDefinition> TextMappings { get; }

    public IReadOnlyList<GroupEntry> Groups { get; }

    public IReadOnlyList<ArmyEntry> Armies { get; }

    public SupportCatalog Supports { get; }

    public AffinityCatalog Affinities { get; }

    public SkillCatalog Skills { get; }

    public FieldDefinition GetField(string id)
        => Fields.TryGetValue(id, out var field)
            ? field
            : throw new KeyNotFoundException($"Unknown field '{id}'.");

    public static GameDataCatalog LoadDefault()
    {
        var assembly = typeof(GameDataCatalog).Assembly;
        var prefix = assembly.GetName().Name + ".Data.";

        var enumsDto = ReadJson<Dictionary<string, List<EnumEntryDto>>>(assembly, prefix, "enums.json");
        var enums = enumsDto.ToDictionary(
            pair => pair.Key,
            pair => new EnumDefinition(
                pair.Key,
                pair.Value.Select(entry => new EnumEntry(entry.Key, entry.Term)).ToList()),
            StringComparer.Ordinal);

        var fieldsDto = ReadJson<FieldsFileDto>(assembly, prefix, "fields.json");
        var profile = new GameProfile(
            ParseHex(fieldsDto.Profile.SlotBase),
            fieldsDto.Profile.SlotStep,
            fieldsDto.Profile.SlotCount,
            fieldsDto.Profile.FieldCount);

        var fields = new Dictionary<string, FieldDefinition>(StringComparer.Ordinal);
        foreach (var dto in fieldsDto.Fields)
        {
            var field = new FieldDefinition(
                dto.Id,
                dto.LegacyKey,
                ParseHex(dto.Address),
                dto.Length,
                dto.Bit is null ? null : new BitField(dto.Bit.Start, dto.Bit.End),
                ParseEnum<FieldScope>(dto.Scope),
                ParseEnum<FieldKind>(dto.Kind),
                dto.EnumId,
                dto.ReadOnly,
                dto.RefreshAfterWrite,
                dto.DisplayKey);

            ValidateField(field, profile, enums);
            if (!fields.TryAdd(field.Id, field))
            {
                throw new InvalidDataException($"Duplicate field id '{field.Id}'.");
            }
        }

        if (fields.Count != profile.FieldCount)
        {
            throw new InvalidDataException(
                $"Profile claims {profile.FieldCount} fields, loaded {fields.Count}.");
        }

        var skillsDto = ReadJson<SkillsDto>(assembly, prefix, "skills.json");
        var supportDto = ReadJson<List<SupportRowDto>>(assembly, prefix, "support.json");
        var affinityDto = ReadJson<List<AffinityDto>>(assembly, prefix, "affinity.json");
        var groupDto = ReadJson<List<GroupDto>>(assembly, prefix, "group.json");
        var armyDto = ReadJson<List<ArmyDto>>(assembly, prefix, "army.json");
        var mappingsDto = ReadJson<Dictionary<string, MappingDto>>(assembly, prefix, "mappings.json");

        var supports = new SupportCatalog(
            supportDto.Select(row => new SupportRow(
                row.Pid,
                row.Entries
                    .Select(entry => entry is null
                        ? null
                        : new SupportEntry(entry.Partner, entry.Thresholds))
                    .ToList()))
                .ToList());

        var affinities = new AffinityCatalog(
            affinityDto.Select(entry => new AffinityEntry(entry.Pid, entry.Affinity)).ToList());

        var groups = groupDto
            .Select(entry => new GroupEntry(ParseHex(entry.Value), entry.Term))
            .ToList();
        var armies = armyDto
            .Select(entry => new ArmyEntry(entry.Value, entry.Term))
            .ToList();

        var pointerMappings = new Dictionary<string, PointerMappingDefinition>(StringComparer.Ordinal);
        var textMappings = new Dictionary<string, TextMappingDefinition>(StringComparer.Ordinal);
        foreach (var (id, dto) in mappingsDto)
        {
            if (!enums.ContainsKey(dto.EnumId))
            {
                throw new InvalidDataException($"Mapping '{id}' references unknown enum '{dto.EnumId}'.");
            }

            switch (dto.Type)
            {
                case "pointer":
                    if (dto.PointerAddress is null)
                    {
                        throw new InvalidDataException($"Pointer mapping '{id}' lacks pointerAddress.");
                    }

                    pointerMappings.Add(
                        id,
                        new PointerMappingDefinition(
                            id,
                            ParseHex(dto.PointerAddress),
                            dto.Step,
                            dto.EnumId));
                    break;

                case "text":
                    if (dto.BaseAddress is null)
                    {
                        throw new InvalidDataException($"Text mapping '{id}' lacks baseAddress.");
                    }

                    textMappings.Add(
                        id,
                        new TextMappingDefinition(
                            id,
                            ParseHex(dto.BaseAddress),
                            dto.Step,
                            dto.AddressOffset,
                            dto.EnumId));
                    break;

                default:
                    throw new InvalidDataException(
                        $"Mapping '{id}' has unknown type '{dto.Type}'.");
            }
        }

        ValidateReferences(enums, supports, affinities);

        return new GameDataCatalog(
            profile,
            fields,
            enums,
            pointerMappings,
            textMappings,
            groups,
            armies,
            supports,
            affinities,
            new SkillCatalog(skillsDto.Skills, skillsDto.Masteries, skillsDto.Features));
    }

    private static T ReadJson<T>(Assembly assembly, string prefix, string fileName)
    {
        var resourceName = prefix + fileName;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Missing embedded resource '{resourceName}'. " +
                $"Available: {string.Join(", ", assembly.GetManifestResourceNames())}");
        return JsonSerializer.Deserialize<T>(stream, JsonOptions)
            ?? throw new InvalidDataException($"Could not deserialize '{resourceName}'.");
    }

    private static void ValidateField(
        FieldDefinition field,
        GameProfile profile,
        IReadOnlyDictionary<string, EnumDefinition> enums)
    {
        if (field.ByteLength is < 1 or > 8)
        {
            throw new InvalidDataException($"Field '{field.Id}' has invalid length {field.Length}.");
        }

        field.Bit?.Validate(field.ByteLength);

        if (field.Scope == FieldScope.Character)
        {
            if (field.Address < profile.SlotBase ||
                field.Address + (uint)field.ByteLength > profile.SlotBase + (uint)profile.SlotStep)
            {
                throw new InvalidDataException(
                    $"Character field '{field.Id}' is outside the slot template.");
            }
        }
        else if (field.Address >= profile.SlotBase && field.Address < profile.SlotEnd)
        {
            throw new InvalidDataException(
                $"Global field '{field.Id}' overlaps the slot array.");
        }

        if (field.Kind == FieldKind.Enum)
        {
            if (field.EnumId is null ||
                (!enums.ContainsKey(field.EnumId) && !SpecialEnumIds.Contains(field.EnumId)))
            {
                throw new InvalidDataException(
                    $"Field '{field.Id}' references unknown enum '{field.EnumId}'.");
            }
        }
        else if (field.EnumId is not null)
        {
            throw new InvalidDataException(
                $"Non-enum field '{field.Id}' unexpectedly has enumId '{field.EnumId}'.");
        }
    }

    private static void ValidateReferences(
        IReadOnlyDictionary<string, EnumDefinition> enums,
        SupportCatalog supports,
        AffinityCatalog affinities)
    {
        var pidKeys = enums["pid"].Entries.Select(entry => entry.Key).ToHashSet(StringComparer.Ordinal);

        foreach (var row in supports.Rows)
        {
            if (!pidKeys.Contains(row.PidKey))
            {
                throw new InvalidDataException($"Support row references unknown PID '{row.PidKey}'.");
            }

            if (row.Entries.Count != 7)
            {
                throw new InvalidDataException(
                    $"Support row '{row.PidKey}' has {row.Entries.Count} entries; expected 7.");
            }

            foreach (var entry in row.Entries.Where(entry => entry is not null))
            {
                if (!pidKeys.Contains(entry!.PartnerKey))
                {
                    throw new InvalidDataException(
                        $"Support partner '{entry.PartnerKey}' is not a PID.");
                }

                if (entry.Thresholds.Count != 3 ||
                    !entry.Thresholds.SequenceEqual(entry.Thresholds.Order()))
                {
                    throw new InvalidDataException(
                        $"Support thresholds for '{row.PidKey}'/'{entry.PartnerKey}' are invalid.");
                }
            }
        }

        foreach (var entry in affinities.Entries)
        {
            if (!pidKeys.Contains(entry.PidKey))
            {
                throw new InvalidDataException(
                    $"Affinity entry references unknown PID '{entry.PidKey}'.");
            }
        }

    }

    private static uint ParseHex(string value)
    {
        var span = value.AsSpan().Trim();
        if (span.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            span = span[2..];
        }

        return uint.Parse(span, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    private static T ParseEnum<T>(string value) where T : struct, Enum
        => Enum.Parse<T>(value, ignoreCase: true);

    private sealed class FieldsFileDto
    {
        public ProfileDto Profile { get; set; } = new();
        public List<FieldDto> Fields { get; set; } = [];
    }

    private sealed class ProfileDto
    {
        public string SlotBase { get; set; } = "";
        public int SlotStep { get; set; }
        public int SlotCount { get; set; }
        public int FieldCount { get; set; }
    }

    private sealed class FieldDto
    {
        public string Id { get; set; } = "";
        public string LegacyKey { get; set; } = "";
        public string Address { get; set; } = "";
        public int Length { get; set; }
        public BitDto? Bit { get; set; }
        public string Scope { get; set; } = "";
        public string Kind { get; set; } = "";
        public string? EnumId { get; set; }
        public bool ReadOnly { get; set; }
        public bool RefreshAfterWrite { get; set; }
        public string DisplayKey { get; set; } = "";
    }

    private sealed class BitDto
    {
        public int Start { get; set; }
        public int End { get; set; }
    }

    private sealed class EnumEntryDto
    {
        public string Key { get; set; } = "";
        public string Term { get; set; } = "";
    }

    private sealed class SkillsDto
    {
        public List<string> Skills { get; set; } = [];
        public List<string> Masteries { get; set; } = [];
        public List<string> Features { get; set; } = [];
    }

    private sealed class SupportRowDto
    {
        public string Pid { get; set; } = "";
        public List<SupportEntryDto?> Entries { get; set; } = [];
    }

    private sealed class SupportEntryDto
    {
        public string Partner { get; set; } = "";
        public List<int> Thresholds { get; set; } = [];
    }

    private sealed class AffinityDto
    {
        public string Pid { get; set; } = "";
        public string Affinity { get; set; } = "";
    }

    private sealed class GroupDto
    {
        public string Value { get; set; } = "";
        public string Term { get; set; } = "";
    }

    private sealed class ArmyDto
    {
        public int Value { get; set; }
        public string Term { get; set; } = "";
    }

    private sealed class MappingDto
    {
        public string Type { get; set; } = "";
        public string? PointerAddress { get; set; }
        public string? BaseAddress { get; set; }
        public int Step { get; set; }
        public int AddressOffset { get; set; }

        [JsonPropertyName("enum")]
        public string EnumId { get; set; } = "";
    }
}
