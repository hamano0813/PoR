namespace PoR.Core.Game;

/// <summary>
/// Small read-only abstraction used by mapping logic. Implemented over
/// <see cref="Memory.IDolphinMemory"/> at runtime and faked in tests.
/// </summary>
public interface IMemoryReader
{
    uint ReadUInt32(uint consoleAddress);

    byte[] ReadBytes(uint consoleAddress, int length);

    string ReadCString(uint consoleAddress, int maxDepth = 8, int maxLength = 512);
}
