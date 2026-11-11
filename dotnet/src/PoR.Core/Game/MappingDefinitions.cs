namespace PoR.Core.Game;

public sealed record PointerMappingDefinition(
    string Id,
    uint PointerAddress,
    int Step,
    string EnumId);

public sealed record TextMappingDefinition(
    string Id,
    uint BaseAddress,
    int Step,
    int AddressOffset,
    string EnumId);

public sealed record GroupEntry(uint Value, string TermKey);

public sealed record ArmyEntry(int Value, string TermKey);

/// <summary>
/// One item in a combo box. <see cref="Value"/> is what gets written to memory;
/// <see cref="Key"/> is the stable enum key; <see cref="TermKey"/> is localized.
/// </summary>
public sealed record MappingItem(
    uint Value,
    string Key,
    string TermKey,
    string? IconKey = null);
