using PoR.Core.Game;
using PoR.Core.Memory;
using PoR.Core.Values;

namespace PoR.Game;

/// <summary>
/// Coordinates Dolphin memory, the game catalog, and dynamic mappings.
/// All memory operations are serialized.
/// </summary>
public sealed class GameSession : IAsyncDisposable
{
    private readonly IDolphinMemory _memory;
    private readonly GameDataCatalog _catalog;
    private readonly DolphinMemoryReader _reader;
    private readonly MappingService _mappingService;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public GameSession(IDolphinMemory memory, GameDataCatalog? catalog = null)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _catalog = catalog ?? GameDataCatalog.Default;
        _reader = new DolphinMemoryReader(_memory);
        _mappingService = new MappingService(_reader);
    }

    public IDolphinMemory Memory => _memory;

    public GameDataCatalog Catalog => _catalog;

    public DolphinStatus Status => _memory.Status;

    public string? StatusMessage => _memory.StatusMessage;

    public bool Hook() => _memory.Hook();

    public void Unhook() => _memory.Unhook();

    public async Task<GameSnapshot> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_memory.Status != DolphinStatus.Hooked && !_memory.Hook())
            {
                throw new InvalidOperationException(
                    $"Dolphin is not hooked: {_memory.Status}. {_memory.StatusMessage}");
            }

            return await Task.Run(BuildSnapshot, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task WriteFieldAsync(
        FieldDefinition field,
        int? slotIndex,
        long value,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var address = ResolveAddress(field, slotIndex);
            var buffer = new byte[field.ByteLength];

            // Bit writes are read-modify-write over the whole field.
            if (field.Bit is not null)
            {
                _memory.ReadBytes(address, buffer);
            }

            ValueCodec.Write(buffer, field.Length, value, field.Bit);
            _memory.WriteBytes(address, buffer);
        }
        finally
        {
            _gate.Release();
        }
    }

    public long ReadField(FieldDefinition field, int? slotIndex)
    {
        var address = ResolveAddress(field, slotIndex);
        var buffer = new byte[field.ByteLength];
        _memory.ReadBytes(address, buffer);
        return ValueCodec.Read(buffer, field.Length, field.Bit);
    }

    public uint ResolveAddress(FieldDefinition field, int? slotIndex)
    {
        if (field.Scope == FieldScope.Global)
        {
            return field.Address;
        }

        if (slotIndex is null)
        {
            throw new ArgumentNullException(
                nameof(slotIndex),
                $"Character field '{field.Id}' needs a slot index.");
        }

        if (slotIndex < 0 || slotIndex >= _catalog.Profile.SlotCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slotIndex),
                slotIndex,
                $"Slot index must be 0..{_catalog.Profile.SlotCount - 1}.");
        }

        return field.Address + (uint)(slotIndex.Value * _catalog.Profile.SlotStep);
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            _gate.Release();
        }
        finally
        {
            _gate.Dispose();
        }
    }

    private GameSnapshot BuildSnapshot()
    {
        var profile = _catalog.Profile;
        var buffer = new byte[profile.SlotCount * profile.SlotStep];
        _memory.ReadBytes(profile.SlotBase, buffer);

        var slots = new SlotSnapshot(
            profile.SlotBase,
            profile.SlotCount,
            profile.SlotStep,
            buffer);

        var mappings = new Dictionary<string, IReadOnlyList<MappingItem>>(StringComparer.Ordinal);

        foreach (var (id, definition) in _catalog.PointerMappings)
        {
            mappings[id] = _mappingService.GetPointerMapping(
                definition,
                _catalog.Enums[definition.EnumId],
                profile);
        }

        foreach (var (id, definition) in _catalog.TextMappings)
        {
            mappings[id] = _mappingService.GetTextMapping(
                definition,
                _catalog.Enums[definition.EnumId]);
        }

        mappings["group"] = _mappingService.GetGroupMapping(_catalog.Groups);
        mappings["army"] = _mappingService.GetArmyMapping(_catalog.Armies);

        var pidMapping = mappings.GetValueOrDefault("pid") ?? [];
        var jidMapping = mappings.GetValueOrDefault("jid") ?? [];
        var personField = _catalog.GetField("person");
        var classField = _catalog.GetField("class");

        var slotInfos = new List<SlotInfo>(profile.SlotCount);
        var allyMapping = new List<MappingItem>();
        for (var index = 0; index < profile.SlotCount; index++)
        {
            var personPointer = (uint)slots.ReadField(personField, index);
            var classPointer = (uint)slots.ReadField(classField, index);
            var pid = pidMapping.FirstOrDefault(item => item.Value == personPointer);
            var jid = jidMapping.FirstOrDefault(item => item.Value == classPointer);
            var slotAddress = profile.SlotBase + (uint)(index * profile.SlotStep);

            slotInfos.Add(
                new SlotInfo(
                    index,
                    slotAddress,
                    personPointer,
                    classPointer,
                    pid?.Key,
                    pid?.TermKey,
                    jid?.Key,
                    jid?.TermKey));

            if (personPointer != 0 && pid is not null)
            {
                allyMapping.Add(
                    new MappingItem(
                        slotAddress,
                        pid.Key,
                        pid.TermKey,
                        pid.IconKey));
            }
        }

        mappings["ally"] = allyMapping;

        return new GameSnapshot(
            slots,
            slotInfos,
            mappings,
            DateTimeOffset.Now);
    }
}
