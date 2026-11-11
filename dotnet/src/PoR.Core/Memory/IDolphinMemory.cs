namespace PoR.Core.Memory;

/// <summary>
/// Read/write access to the emulated memory of a running Dolphin process.
/// </summary>
public interface IDolphinMemory : IDisposable
{
    DolphinStatus Status { get; }

    /// <summary>Human-readable detail for the current status, e.g. "Dolphin not found".</summary>
    string? StatusMessage { get; }

    /// <summary>Dolphin process ID. Zero when not hooked.</summary>
    uint ProcessId { get; }

    /// <summary>Host process address of the start of MEM1. Zero when unhooked.</summary>
    ulong Mem1HostBase { get; }

    /// <summary>Simulated MEM1 size in bytes. Zero when unhooked.</summary>
    uint Mem1Size { get; }

    event EventHandler<DolphinStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Finds Dolphin and locates MEM1. Safe to call repeatedly.
    /// </summary>
    bool Hook();

    void Unhook();

    /// <summary>
    /// Reads raw bytes from a GameCube console address (e.g. 0x802AF76C).
    /// Throws <see cref="MemoryAccessException"/> on failure.
    /// </summary>
    void ReadBytes(uint consoleAddress, Span<byte> buffer);

    /// <summary>
    /// Writes raw bytes to a GameCube console address.
    /// Throws <see cref="MemoryAccessException"/> on failure.
    /// </summary>
    void WriteBytes(uint consoleAddress, ReadOnlySpan<byte> data);
}
