using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PoR.Memory.Dolphin;

internal sealed record Mem1Info(ulong HostBase, uint Size);

internal static class Mem1Locator
{
    private const uint SystemInfoOffset = 0x00000018;
    private const uint DolphinOsGlobalsOffset = 0x00000084;
    private const int SystemInfoSize = 20;
    private const int DolphinOsGlobalsSize = 124;
    private const int SimulatedMemorySizeOffset = 108;
    private const uint DefaultGameCubeMem1Size = 0x01800000;
    private const uint MaxPlausibleMem1Size = 0x10000000;

    private static readonly byte[] GameCubeDiscMagic = [0xC2, 0x33, 0x9F, 0x3D];

    private static readonly byte[][] KnownBootCodes =
    [
        [0x0D, 0x15, 0xEA, 0x5E],
        [0xE5, 0x20, 0x7C, 0x22],
    ];

    /// <summary>
    /// Walks committed memory regions and probes the Dolphin emulator structures
    /// used by DME to locate GameCube MEM1.
    /// </summary>
    public static Mem1Info? Find(SafeProcessHandle process, out string diagnostics)
    {
        var notes = new List<string>();
        var address = IntPtr.Zero;
        var memoryBasicInfoSize = (nuint)Marshal.SizeOf<NativeMethods.MEMORY_BASIC_INFORMATION>();
        var workingSetInfoSize = (uint)Marshal.SizeOf<NativeMethods.PSAPI_WORKING_SET_EX_INFORMATION>();

        while (true)
        {
            var queried = NativeMethods.VirtualQueryEx(
                process,
                address,
                out var info,
                memoryBasicInfoSize);

            if (queried == 0 || info.RegionSize == 0)
            {
                break;
            }

            if (info.State == NativeMethods.MEM_COMMIT)
            {
                var workingSet = new NativeMethods.PSAPI_WORKING_SET_EX_INFORMATION
                {
                    VirtualAddress = info.BaseAddress,
                };

                if (NativeMethods.QueryWorkingSetEx(process, ref workingSet, workingSetInfoSize) &&
                    (workingSet.Flags & 1UL) != 0)
                {
                    if (TryProbe(process, info.BaseAddress, out var mem1, out var reason) &&
                        mem1 is not null)
                    {
                        notes.Add(
                            $"found MEM1 at host 0x{mem1.HostBase:X}, size 0x{mem1.Size:X}");
                        diagnostics = string.Join(Environment.NewLine, notes);
                        return mem1;
                    }

                    if (notes.Count < 16)
                    {
                        notes.Add(
                            $"candidate 0x{info.BaseAddress.ToInt64():X}: {reason}");
                    }
                }
            }

            var current = info.BaseAddress.ToInt64();
            var next = current + (long)info.RegionSize;
            if (next <= current)
            {
                break;
            }

            address = new IntPtr(next);
        }

        diagnostics = notes.Count == 0
            ? "No committed memory regions with a valid working set were found."
            : string.Join(Environment.NewLine, notes);
        return null;
    }

    private static bool TryProbe(
        SafeProcessHandle process,
        IntPtr baseAddress,
        out Mem1Info? mem1,
        out string reason)
    {
        mem1 = null;

        var systemInfo = new byte[SystemInfoSize];
        if (!TryRead(
                process,
                IntPtr.Add(baseAddress, (int)SystemInfoOffset),
                systemInfo,
                out var readError))
        {
            reason = $"SystemInfo read failed: {readError}";
            return false;
        }

        if (!systemInfo.AsSpan(4, 4).SequenceEqual(GameCubeDiscMagic))
        {
            reason = "GameCube disc magic 0xC2339F3D not found";
            return false;
        }

        var bootCodeValid = KnownBootCodes.Any(
            code => systemInfo.AsSpan(8, 4).SequenceEqual(code));
        if (!bootCodeValid)
        {
            reason = "boot code is not a known GameCube boot code";
            return false;
        }

        var globals = new byte[DolphinOsGlobalsSize];
        if (!TryRead(
                process,
                IntPtr.Add(baseAddress, (int)DolphinOsGlobalsOffset),
                globals,
                out readError))
        {
            reason = $"DolphinOSGlobals read failed: {readError}";
            return false;
        }

        var size = BinaryPrimitives.ReadUInt32BigEndian(
            globals.AsSpan(SimulatedMemorySizeOffset, sizeof(uint)));

        // Dolphin may report zero early during boot; MEM1 is still 24 MiB.
        if (size == 0)
        {
            size = DefaultGameCubeMem1Size;
        }

        if (size > MaxPlausibleMem1Size)
        {
            reason = $"implausible simulated MEM1 size 0x{size:X}";
            return false;
        }

        mem1 = new Mem1Info((ulong)baseAddress.ToInt64(), size);
        reason = "ok";
        return true;
    }

    private static bool TryRead(
        SafeProcessHandle process,
        IntPtr address,
        byte[] buffer,
        out string error)
    {
        if (!NativeMethods.ReadProcessMemory(
                process,
                address,
                buffer,
                (nuint)buffer.Length,
                out var bytesRead) ||
            (int)bytesRead != buffer.Length)
        {
            error =
                $"ReadProcessMemory at 0x{address.ToInt64():X} failed " +
                $"(win32={Marshal.GetLastWin32Error()}, read={bytesRead}/{buffer.Length})";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
