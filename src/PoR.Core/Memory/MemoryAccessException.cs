namespace PoR.Core.Memory;

/// <summary>
/// Raised when a read/write against Dolphin's emulated memory fails.
/// Implementations should unhook before throwing this exception.
/// </summary>
public sealed class MemoryAccessException : Exception
{
    public MemoryAccessException(
        string message,
        uint consoleAddress,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ConsoleAddress = consoleAddress;
    }

    public uint ConsoleAddress { get; }
}
