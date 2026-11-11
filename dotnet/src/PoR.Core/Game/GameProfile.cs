namespace PoR.Core.Game;

public sealed record GameProfile(
    uint SlotBase,
    int SlotStep,
    int SlotCount,
    int FieldCount)
{
    public uint SlotEnd => SlotBase + (uint)(SlotStep * SlotCount);
}
