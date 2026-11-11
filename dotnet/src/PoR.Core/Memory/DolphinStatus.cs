namespace PoR.Core.Memory;

/// <summary>
/// Connection state of the Dolphin memory access layer.
/// Mirrors the four states exposed by Dolphin Memory Engine.
/// </summary>
public enum DolphinStatus
{
    Hooked,
    NotRunning,
    NoEmu,
    Unhooked,
    AccessDenied,
}
