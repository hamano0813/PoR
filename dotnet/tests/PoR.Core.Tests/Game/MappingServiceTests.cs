using System.Buffers.Binary;
using System.Text;
using PoR.Core.Game;

namespace PoR.Core.Tests.Game;

public sealed class MappingServiceTests
{
    private static readonly GameProfile Profile = new(0x802AF5E4, 0x280, 3, 0);

    [Fact]
    public void PointerMappingFindsCurrentEntryAndComputesAddresses()
    {
        var memory = new FakeMemory();
        var pointerAddress = 0x802AF76Cu;
        var currentEntry = 0x90001000u;
        memory.SetUInt32(pointerAddress, currentEntry);
        memory.SetCString(currentEntry, "PID_B");

        var definition = new PointerMappingDefinition("pid", pointerAddress, 0x64, "pid");
        var enumDefinition = new EnumDefinition(
            "pid",
            [
                new EnumEntry("PID_A", "PID_A"),
                new EnumEntry("PID_B", "PID_B"),
                new EnumEntry("PID_C", "PID_C"),
            ]);

        var items = new MappingService(memory)
            .GetPointerMapping(definition, enumDefinition, Profile);

        Assert.Equal(3, items.Count);
        Assert.Equal(currentEntry - 0x64u, items[0].Value);
        Assert.Equal(currentEntry, items[1].Value);
        Assert.Equal(currentEntry + 0x64u, items[2].Value);
        Assert.Equal(["PID_A", "PID_B", "PID_C"], items.Select(item => item.Key));
        Assert.All(items, item => Assert.Equal(item.Key, item.IconKey));
    }

    [Fact]
    public void PointerMappingReturnsEmptyWhenNoSlotTextMatches()
    {
        var memory = new FakeMemory();
        var definition = new PointerMappingDefinition("pid", 0x802AF76C, 0x64, "pid");
        var enumDefinition = new EnumDefinition(
            "pid",
            [new EnumEntry("PID_A", "PID_A")]);

        var items = new MappingService(memory)
            .GetPointerMapping(definition, enumDefinition, Profile);

        Assert.Empty(items);
    }

    [Fact]
    public void TextMappingAppliesAddressOffset()
    {
        var memory = new FakeMemory();
        var baseAddress = 0x80511258u;
        var step = 0x10;
        var offset = -0x131F;
        var expectedFirst = 0x80512000u;
        var expectedSecond = 0x80512010u;
        memory.SetUInt32(baseAddress, (uint)(expectedFirst - offset));
        memory.SetUInt32(baseAddress + (uint)step, (uint)(expectedSecond - offset));

        var definition = new TextMappingDefinition("seq", baseAddress, step, offset, "seq");
        var enumDefinition = new EnumDefinition(
            "seq",
            [
                new EnumEntry("SEQ_A", "SEQ_A"),
                new EnumEntry("SEQ_B", "SEQ_B"),
            ]);

        var items = new MappingService(memory)
            .GetTextMapping(definition, enumDefinition);

        Assert.Equal(2, items.Count);
        Assert.Equal(expectedFirst, items[0].Value);
        Assert.Equal(expectedSecond, items[1].Value);
        Assert.Equal(["SEQ_A", "SEQ_B"], items.Select(item => item.Key));
    }

    [Fact]
    public void GroupAndArmyMappingsKeepValuesAndTerms()
    {
        var service = new MappingService(new FakeMemory());

        var groups = service.GetGroupMapping(
            [new GroupEntry(0x802C8364, "this"), new GroupEntry(0x802C8370, "enemy")]);
        Assert.Equal([0x802C8364u, 0x802C8370u], groups.Select(item => item.Value));
        Assert.Equal(["this", "enemy"], groups.Select(item => item.TermKey));

        var armies = service.GetArmyMapping(
            [new ArmyEntry(0x01, "Crimea"), new ArmyEntry(0x02, "Daein")]);
        Assert.Equal([1u, 2u], armies.Select(item => item.Value));
        Assert.Equal(["Crimea", "Daein"], armies.Select(item => item.TermKey));
    }

    private sealed class FakeMemory : IMemoryReader
    {
        private readonly Dictionary<uint, byte> _bytes = [];

        public void SetBytes(uint address, ReadOnlySpan<byte> bytes)
        {
            for (var index = 0; index < bytes.Length; index++)
            {
                _bytes[address + (uint)index] = bytes[index];
            }
        }

        public void SetUInt32(uint address, uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            SetBytes(address, buffer);
        }

        public void SetCString(uint address, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value + "\0");
            SetBytes(address, bytes);
        }

        public uint ReadUInt32(uint consoleAddress)
        {
            Span<byte> buffer = stackalloc byte[4];
            for (var index = 0; index < buffer.Length; index++)
            {
                buffer[index] = Get(consoleAddress + (uint)index);
            }

            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }

        public byte[] ReadBytes(uint consoleAddress, int length)
        {
            var buffer = new byte[length];
            for (var index = 0; index < length; index++)
            {
                buffer[index] = Get(consoleAddress + (uint)index);
            }

            return buffer;
        }

        public string ReadCString(uint consoleAddress, int maxDepth = 8, int maxLength = 512)
        {
            var bytes = new List<byte>();
            for (var index = 0; index < maxLength; index++)
            {
                var value = Get(consoleAddress + (uint)index);
                if (value == 0)
                {
                    break;
                }

                bytes.Add(value);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private byte Get(uint address)
            => _bytes.TryGetValue(address, out var value) ? value : (byte)0;
    }
}
