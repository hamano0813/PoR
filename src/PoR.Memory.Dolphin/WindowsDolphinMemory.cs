using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using PoR.Core.Memory;

namespace PoR.Memory.Dolphin;

/// <summary>
/// Windows/GameCube implementation of the Dolphin memory access layer.
/// It ports only the MEM1 path used by Path of Radiance.
/// </summary>
public sealed class WindowsDolphinMemory : IDolphinMemory
{
    private const uint GameCubeMem1Start = 0x80000000;
    private const uint ProcessAccess =
        NativeMethods.PROCESS_QUERY_INFORMATION |
        NativeMethods.PROCESS_VM_OPERATION |
        NativeMethods.PROCESS_VM_READ |
        NativeMethods.PROCESS_VM_WRITE;

    private readonly object _gate = new();

    private SafeProcessHandle? _process;
    private uint _processId;
    private ulong _mem1HostBase;
    private uint _mem1Size;
    private DolphinStatus _status = DolphinStatus.Unhooked;
    private string? _statusMessage = "Not hooked";
    private bool _disposed;

    public DolphinStatus Status => _status;

    public string? StatusMessage => _statusMessage;

    public uint ProcessId => _processId;

    public ulong Mem1HostBase => _mem1HostBase;

    public uint Mem1Size => _mem1Size;

    public event EventHandler<DolphinStatusChangedEventArgs>? StatusChanged;

    public bool Hook()
    {
        lock (_gate)
        {
            ThrowIfDisposed();

            if (_status == DolphinStatus.Hooked &&
                _process is { IsInvalid: false, IsClosed: false })
            {
                return true;
            }

            UnhookCore();

            var processInfo = DolphinProcessLocator.Find();
            if (processInfo is null)
            {
                SetStatus(
                    DolphinStatus.NotRunning,
                    "Dolphin process not found. Expected Dolphin.exe, DolphinQt2.exe or DolphinWx.exe.");
                return false;
            }

            var rawHandle = NativeMethods.OpenProcess(
                ProcessAccess,
                false,
                processInfo.ProcessId);

            if (rawHandle == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                SetStatus(
                    DolphinStatus.AccessDenied,
                    $"OpenProcess failed for {processInfo.ExecutableName} " +
                    $"(pid {processInfo.ProcessId}, win32={error}). " +
                    "Run Dolphin and the modifier at the same privilege level.");
                return false;
            }

            var process = new SafeProcessHandle(rawHandle, ownsHandle: true);
            var mem1 = Mem1Locator.Find(process, out var diagnostics);

            if (mem1 is null)
            {
                process.Dispose();
                SetStatus(
                    DolphinStatus.NoEmu,
                    "Dolphin is running, but GameCube MEM1 was not found. " +
                    "Start a game and wait for it to boot." +
                    (string.IsNullOrWhiteSpace(diagnostics)
                        ? string.Empty
                        : Environment.NewLine + diagnostics));
                return false;
            }

            _process = process;
            _processId = processInfo.ProcessId;
            _mem1HostBase = mem1.HostBase;
            _mem1Size = mem1.Size;

            SetStatus(
                DolphinStatus.Hooked,
                $"Hooked {processInfo.ExecutableName} pid {_processId}; " +
                $"MEM1 host 0x{_mem1HostBase:X}, size 0x{_mem1Size:X}.");
            return true;
        }
    }

    public void Unhook()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            var wasHooked = _status == DolphinStatus.Hooked;
            UnhookCore();

            if (wasHooked)
            {
                SetStatus(DolphinStatus.Unhooked, "Unhooked.");
            }
        }
    }

    public void ReadBytes(uint consoleAddress, Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return;
        }

        lock (_gate)
        {
            EnsureUsable(consoleAddress);
            var hostAddress = ConsoleToHost(consoleAddress, buffer.Length);
            var temporary = new byte[buffer.Length];

            if (!NativeMethods.ReadProcessMemory(
                    _process!,
                    new IntPtr((long)hostAddress),
                    temporary,
                    (nuint)temporary.Length,
                    out var bytesRead) ||
                (int)bytesRead != temporary.Length)
            {
                var error = Marshal.GetLastWin32Error();
                UnhookCore();
                SetStatus(
                    DolphinStatus.Unhooked,
                    $"Read failed at console address 0x{consoleAddress:X8} (win32={error}). Unhooked.");
                throw new MemoryAccessException(
                    $"ReadProcessMemory failed at console address 0x{consoleAddress:X8} " +
                    $"(win32={error}).",
                    consoleAddress);
            }

            temporary.AsSpan().CopyTo(buffer);
        }
    }

    public void WriteBytes(uint consoleAddress, ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return;
        }

        lock (_gate)
        {
            EnsureUsable(consoleAddress);
            var hostAddress = ConsoleToHost(consoleAddress, data.Length);
            var temporary = data.ToArray();

            if (!NativeMethods.WriteProcessMemory(
                    _process!,
                    new IntPtr((long)hostAddress),
                    temporary,
                    (nuint)temporary.Length,
                    out var bytesWritten) ||
                (int)bytesWritten != temporary.Length)
            {
                var error = Marshal.GetLastWin32Error();
                UnhookCore();
                SetStatus(
                    DolphinStatus.Unhooked,
                    $"Write failed at console address 0x{consoleAddress:X8} (win32={error}). Unhooked.");
                throw new MemoryAccessException(
                    $"WriteProcessMemory failed at console address 0x{consoleAddress:X8} " +
                    $"(win32={error}).",
                    consoleAddress);
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            UnhookCore();
            _status = DolphinStatus.Unhooked;
            _statusMessage = "Disposed.";
        }
    }

    private void EnsureUsable(uint consoleAddress)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WindowsDolphinMemory));
        }

        if (_status != DolphinStatus.Hooked ||
            _process is null ||
            _process.IsInvalid ||
            _process.IsClosed)
        {
            throw new MemoryAccessException(
                "Dolphin is not hooked. Call Hook() before reading/writing memory.",
                consoleAddress);
        }
    }

    private ulong ConsoleToHost(uint consoleAddress, int length)
    {
        if (consoleAddress < GameCubeMem1Start)
        {
            throw new MemoryAccessException(
                $"Console address 0x{consoleAddress:X8} is below GameCube MEM1.",
                consoleAddress);
        }

        var offset = (ulong)(consoleAddress - GameCubeMem1Start);
        if (offset + (ulong)length > _mem1Size)
        {
            throw new MemoryAccessException(
                $"Console address 0x{consoleAddress:X8} + {length} is outside MEM1 " +
                $"(base 0x{GameCubeMem1Start:X8}, size 0x{_mem1Size:X}).",
                consoleAddress);
        }

        return _mem1HostBase + offset;
    }

    private void UnhookCore()
    {
        _process?.Dispose();
        _process = null;
        _processId = 0;
        _mem1HostBase = 0;
        _mem1Size = 0;
    }

    private void SetStatus(DolphinStatus status, string? message)
    {
        var oldStatus = _status;
        if (oldStatus == status && _statusMessage == message)
        {
            return;
        }

        _status = status;
        _statusMessage = message;
        StatusChanged?.Invoke(
            this,
            new DolphinStatusChangedEventArgs(oldStatus, status, message));
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WindowsDolphinMemory));
        }
    }
}
