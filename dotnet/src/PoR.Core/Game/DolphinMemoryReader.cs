using System.Buffers.Binary;
using System.Text;
using PoR.Core.Memory;

namespace PoR.Core.Game;

/// <summary>
/// IMemoryReader over the live Dolphin memory implementation.
/// </summary>
public sealed class DolphinMemoryReader : IMemoryReader
{
    private const uint Mem1Start = 0x80000000;
    private const uint Mem1End = 0x81800000;

    private readonly IDolphinMemory _memory;

    public DolphinMemoryReader(IDolphinMemory memory)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
    }

    public uint ReadUInt32(uint consoleAddress)
    {
        Span<byte> buffer = stackalloc byte[4];
        _memory.ReadBytes(consoleAddress, buffer);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    public byte[] ReadBytes(uint consoleAddress, int length)
    {
        var buffer = new byte[length];
        _memory.ReadBytes(consoleAddress, buffer);
        return buffer;
    }

    public string ReadCString(uint consoleAddress, int maxDepth = 8, int maxLength = 512)
    {
        if (consoleAddress == 0 || maxDepth < 0 || maxLength <= 0)
        {
            return string.Empty;
        }

        var word = ReadUInt32(consoleAddress);
        if (word > Mem1Start && word < Mem1End)
        {
            return ReadCString(word, maxDepth - 1, maxLength);
        }

        var bytes = new List<byte>(Math.Min(maxLength, 32));
        var chunk = new byte[32];
        var consumed = 0;

        while (consumed < maxLength)
        {
            var size = Math.Min(chunk.Length, maxLength - consumed);
            try
            {
                _memory.ReadBytes(
                    consoleAddress + (uint)consumed,
                    chunk.AsSpan(0, size));
            }
            catch (MemoryAccessException)
            {
                break;
            }

            var nul = Array.IndexOf(chunk, (byte)0, 0, size);
            if (nul >= 0)
            {
                for (var index = 0; index < nul; index++)
                {
                    bytes.Add(chunk[index]);
                }

                break;
            }

            for (var index = 0; index < size; index++)
            {
                bytes.Add(chunk[index]);
            }

            consumed += size;
        }

        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}
