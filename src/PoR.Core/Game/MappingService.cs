namespace PoR.Core.Game;

/// <summary>
/// Computes the dynamic combo-box mappings used by the original EnumData.
/// </summary>
public sealed class MappingService
{
    private readonly IMemoryReader _memory;

    public MappingService(IMemoryReader memory)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
    }

    public IReadOnlyList<MappingItem> GetPointerMapping(
        PointerMappingDefinition definition,
        EnumDefinition enumDefinition,
        GameProfile profile)
    {
        if (definition.Step <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(definition), "Step must be positive.");
        }

        var currentPointer = _memory.ReadUInt32(definition.PointerAddress);
        if (currentPointer == 0)
        {
            return [];
        }

        for (var slotIndex = 0; slotIndex < profile.SlotCount; slotIndex++)
        {
            var slotAddress = definition.PointerAddress +
                              (uint)(slotIndex * profile.SlotStep);
            var pointer = _memory.ReadUInt32(slotAddress);
            if (pointer == 0)
            {
                continue;
            }

            var text = _memory.ReadCString(pointer);
            if (!enumDefinition.TryGetIndex(text, out var enumIndex))
            {
                continue;
            }

            var baseAddress = (long)currentPointer - ((long)enumIndex * definition.Step);
            if (baseAddress < 0 || baseAddress > uint.MaxValue)
            {
                return [];
            }

            var items = new List<MappingItem>(enumDefinition.Entries.Count);
            for (var entryIndex = 0; entryIndex < enumDefinition.Entries.Count; entryIndex++)
            {
                var value = baseAddress + ((long)entryIndex * definition.Step);
                if (value < 0 || value > uint.MaxValue)
                {
                    return [];
                }

                var entry = enumDefinition.Entries[entryIndex];
                items.Add(new MappingItem(
                    (uint)value,
                    entry.Key,
                    entry.TermKey,
                    IconKey(enumDefinition.Id, entry.Key)));
            }

            return items;
        }

        return [];
    }

    public IReadOnlyList<MappingItem> GetTextMapping(
        TextMappingDefinition definition,
        EnumDefinition enumDefinition)
    {
        if (definition.Step <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(definition), "Step must be positive.");
        }

        var items = new List<MappingItem>(enumDefinition.Entries.Count);
        for (var index = 0; index < enumDefinition.Entries.Count; index++)
        {
            var raw = _memory.ReadUInt32(
                definition.BaseAddress + (uint)(index * definition.Step));
            var value = (long)raw + definition.AddressOffset;
            if (value < 0 || value > uint.MaxValue)
            {
                continue;
            }

            var entry = enumDefinition.Entries[index];
            items.Add(new MappingItem(
                (uint)value,
                entry.Key,
                entry.TermKey,
                IconKey(enumDefinition.Id, entry.Key)));
        }

        return items;
    }

    public IReadOnlyList<MappingItem> GetGroupMapping(IReadOnlyList<GroupEntry> groups)
        => groups
            .Select(entry => new MappingItem(
                entry.Value,
                $"group.{entry.Value:X8}",
                entry.TermKey))
            .ToList();

    public IReadOnlyList<MappingItem> GetArmyMapping(IReadOnlyList<ArmyEntry> armies)
        => armies
            .Select(entry => new MappingItem(
                (uint)entry.Value,
                $"army.{entry.Value:X2}",
                entry.TermKey))
            .ToList();

    private static string? IconKey(string enumId, string key)
        => enumId is "pid" or "iid" or "sid" ? key : null;
}
