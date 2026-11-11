using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using PoR.Core.Memory;
using PoR.Memory.Dolphin;

namespace PoR.Tools.Probe;

internal static class Program
{
    private const uint PersonAddress = 0x802AF76C;
    private const uint ClassAddress = 0x802AF770;
    private const uint LevelAddress = 0x802AF77E;
    private const uint MoneyAddress = 0x8032E7A1;

    private const uint SlotBase = 0x802AF5E4;
    private const int SlotStep = 0x280;
    private const int SlotCount = 0x9F;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        var asJson = args.Any(
            arg => string.Equals(arg, "--json", StringComparison.OrdinalIgnoreCase));
        var writeSelfTest = args.Any(
            arg => string.Equals(arg, "--write-self-test", StringComparison.OrdinalIgnoreCase));

        using var memory = new WindowsDolphinMemory();
        if (!asJson)
        {
            memory.StatusChanged += (_, change) =>
                Console.Error.WriteLine(
                    $"[status] {change.OldStatus} -> {change.NewStatus}: {change.Message}");
        }

        if (!memory.Hook())
        {
            if (asJson)
            {
                Console.WriteLine(
                    JsonSerializer.Serialize(
                        new
                        {
                            status = memory.Status.ToString(),
                            message = memory.StatusMessage,
                        },
                        JsonOptions));
            }
            else
            {
                Console.Error.WriteLine($"Hook failed: {memory.Status}");
                Console.Error.WriteLine(memory.StatusMessage);
            }

            return 1;
        }

        try
        {
            var person = ReadUInt32(memory, PersonAddress);
            var classId = ReadUInt32(memory, ClassAddress);
            var level = ReadByte(memory, LevelAddress);
            var money = ReadUInt24(memory, MoneyAddress);
            ReadSlotSnapshot(memory, out var visibleSlots, out var sample);
            var writeRoundTrip = writeSelfTest && RunWriteSelfTest(memory);

            if (asJson)
            {
                Console.WriteLine(
                    JsonSerializer.Serialize(
                        new
                        {
                            status = memory.Status.ToString(),
                            processId = memory.ProcessId,
                            mem1HostBase = $"0x{memory.Mem1HostBase:X}",
                            mem1Size = $"0x{memory.Mem1Size:X}",
                            person = $"0x{person:X8}",
                            classId = $"0x{classId:X8}",
                            level,
                            money,
                            visibleSlots,
                            sample,
                            writeSelfTest = writeSelfTest ? writeRoundTrip : (bool?)null,
                        },
                        JsonOptions));
            }
            else
            {
                Console.WriteLine($"Status:      {memory.Status}");
                Console.WriteLine($"Process ID:  {memory.ProcessId}");
                Console.WriteLine($"MEM1 host:   0x{memory.Mem1HostBase:X}");
                Console.WriteLine($"MEM1 size:   0x{memory.Mem1Size:X}");
                Console.WriteLine($"Person:      0x{person:X8}");
                Console.WriteLine($"Class:       0x{classId:X8}");
                Console.WriteLine($"Level:       {level}");
                Console.WriteLine($"Money:       {money}");
                Console.WriteLine($"Visible:     {visibleSlots} slots");
                if (writeSelfTest)
                {
                    Console.WriteLine($"Write test:  {(writeRoundTrip ? "PASS" : "FAIL")}");
                }

                foreach (var slot in sample)
                {
                    Console.WriteLine(
                        $"  [{slot.Index}] person=0x{slot.Person:X8} class=0x{slot.ClassId:X8}");
                }
            }

            if (writeSelfTest && !writeRoundTrip)
            {
                return 3;
            }

            return 0;
        }
        catch (MemoryAccessException exception)
        {
            if (asJson)
            {
                Console.WriteLine(
                    JsonSerializer.Serialize(
                        new
                        {
                            status = memory.Status.ToString(),
                            error = exception.Message,
                        },
                        JsonOptions));
            }
            else
            {
                Console.Error.WriteLine($"Memory access failed: {exception.Message}");
            }

            return 2;
        }
    }

    private static void ReadSlotSnapshot(
        IDolphinMemory memory,
        out int visibleSlots,
        out List<SlotSample> sample)
    {
        var buffer = new byte[SlotCount * SlotStep];
        memory.ReadBytes(SlotBase, buffer);

        var personOffset = (int)(PersonAddress - SlotBase);
        var classOffset = (int)(ClassAddress - SlotBase);
        visibleSlots = 0;
        sample = [];

        for (var index = 0; index < SlotCount; index++)
        {
            var offset = index * SlotStep;
            var person = BinaryPrimitives.ReadUInt32BigEndian(
                buffer.AsSpan(offset + personOffset, sizeof(uint)));
            if (person == 0)
            {
                continue;
            }

            var classId = BinaryPrimitives.ReadUInt32BigEndian(
                buffer.AsSpan(offset + classOffset, sizeof(uint)));
            visibleSlots++;

            if (sample.Count < 10)
            {
                sample.Add(new SlotSample(index, person, classId));
            }
        }
    }

    private static bool RunWriteSelfTest(IDolphinMemory memory)
    {
        // Write the same bytes back: validates WriteProcessMemory without
        // changing the player's game state.
        var moneyBefore = new byte[3];
        memory.ReadBytes(MoneyAddress, moneyBefore);
        memory.WriteBytes(MoneyAddress, moneyBefore);
        var moneyAfter = new byte[3];
        memory.ReadBytes(MoneyAddress, moneyAfter);

        var levelBefore = new byte[1];
        memory.ReadBytes(LevelAddress, levelBefore);
        memory.WriteBytes(LevelAddress, levelBefore);
        var levelAfter = new byte[1];
        memory.ReadBytes(LevelAddress, levelAfter);

        return moneyBefore.AsSpan().SequenceEqual(moneyAfter) &&
               levelBefore.AsSpan().SequenceEqual(levelAfter);
    }

    private static byte ReadByte(IDolphinMemory memory, uint address)
    {
        Span<byte> buffer = stackalloc byte[1];
        memory.ReadBytes(address, buffer);
        return buffer[0];
    }

    private static uint ReadUInt32(IDolphinMemory memory, uint address)
    {
        Span<byte> buffer = stackalloc byte[4];
        memory.ReadBytes(address, buffer);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    private static uint ReadUInt24(IDolphinMemory memory, uint address)
    {
        Span<byte> buffer = stackalloc byte[3];
        memory.ReadBytes(address, buffer);
        return (uint)((buffer[0] << 16) | (buffer[1] << 8) | buffer[2]);
    }

    private sealed record SlotSample(int Index, uint Person, uint ClassId);
}
