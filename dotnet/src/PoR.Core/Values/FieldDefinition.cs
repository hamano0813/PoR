namespace PoR.Core.Values;

/// <summary>
/// One in-game field. Character fields are relative to the slot base; global
/// fields (money, bonus EXP) are absolute.
/// </summary>
public sealed record FieldDefinition(
    string Id,
    string LegacyKey,
    uint Address,
    int Length,
    BitField? Bit,
    FieldScope Scope,
    FieldKind Kind,
    string? EnumId,
    bool ReadOnly,
    bool RefreshAfterWrite,
    string DisplayKey)
{
    public int ByteLength => Math.Abs(Length);

    public bool IsSigned => Length < 0;
}
