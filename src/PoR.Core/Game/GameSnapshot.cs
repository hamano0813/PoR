namespace PoR.Core.Game;

public sealed record SlotInfo(
    int Index,
    uint SlotAddress,
    uint PersonPointer,
    uint ClassPointer,
    string? PidKey,
    string? PidTermKey,
    string? JidKey,
    string? JidTermKey)
{
    public bool IsEmpty => PersonPointer == 0;
}

/// <summary>
/// Result of one refresh: the slot snapshot plus every dynamic mapping.
/// </summary>
public sealed class GameSnapshot
{
    public GameSnapshot(
        SlotSnapshot slots,
        IReadOnlyList<SlotInfo> slotInfos,
        IReadOnlyDictionary<string, IReadOnlyList<MappingItem>> mappings,
        IReadOnlyDictionary<string, long> globalValues,
        DateTimeOffset timestamp)
    {
        Slots = slots;
        SlotInfos = slotInfos;
        Mappings = mappings;
        GlobalValues = globalValues;
        Timestamp = timestamp;
    }

    public SlotSnapshot Slots { get; }

    public IReadOnlyList<SlotInfo> SlotInfos { get; }

    public IReadOnlyDictionary<string, IReadOnlyList<MappingItem>> Mappings { get; }

    public IReadOnlyDictionary<string, long> GlobalValues { get; }

    public DateTimeOffset Timestamp { get; }

    public IReadOnlyList<MappingItem> GetMapping(string id)
        => Mappings.TryGetValue(id, out var mapping)
            ? mapping
            : throw new KeyNotFoundException($"Unknown mapping '{id}'.");
}
