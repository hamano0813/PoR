using PoR.Core.Memory;
using PoR.Game;

namespace PoR.Core.Tests.Game;

public sealed class GameSessionLockTests
{
    [Fact]
    public async Task StartLockRewritesFieldUntilStopped()
    {
        using var memory = new FakeMemory();
        await using var session = new GameSession(memory);
        var field = session.Catalog.GetField("action");

        await session.StartLockAsync(
            field,
            0,
            0,
            TimeSpan.FromMilliseconds(10));

        Assert.True(session.IsLocked(field, 0));

        // Simulate the game changing the value while the lock is active.
        memory.SetByte(field.Address, 1);
        await Task.Delay(100);

        Assert.Equal(0, session.ReadField(field, 0));

        await session.StopLockAsync(field, 0, finalValue: 1);

        Assert.False(session.IsLocked(field, 0));
        Assert.Equal(1, session.ReadField(field, 0));
    }

    [Fact]
    public async Task RefreshReadsGlobalFields()
    {
        using var memory = new FakeMemory();
        await using var session = new GameSession(memory);
        var money = session.Catalog.GetField("money");
        memory.WriteBytes(money.Address, new byte[] { 0x01, 0x02, 0x03 });

        var snapshot = await session.RefreshAsync();

        Assert.Equal(0x010203, snapshot.GlobalValues["money"]);
    }

    [Fact]
    public async Task StopAllLocksClearsEveryLock()
    {
        using var memory = new FakeMemory();
        await using var session = new GameSession(memory);
        var action = session.Catalog.GetField("action");
        var rescuing = session.Catalog.GetField("rescuing");

        await session.StartLockAsync(action, 0, 0);
        await session.StartLockAsync(rescuing, 0, 0);

        Assert.True(session.IsLocked(action, 0));
        Assert.True(session.IsLocked(rescuing, 0));

        await session.StopAllLocksAsync();

        Assert.False(session.IsLocked(action, 0));
        Assert.False(session.IsLocked(rescuing, 0));
    }

    private sealed class FakeMemory : IDolphinMemory
    {
        private const uint Mem1Start = 0x80000000;
        private readonly byte[] _memory = new byte[0x01800000];

        public DolphinStatus Status => DolphinStatus.Hooked;

        public string? StatusMessage => null;

        public uint ProcessId => 1;

        public ulong Mem1HostBase => 0x10000000;

        public uint Mem1Size => (uint)_memory.Length;

        public event EventHandler<DolphinStatusChangedEventArgs>? StatusChanged
        {
            add { }
            remove { }
        }

        public bool Hook() => true;

        public void Unhook()
        {
        }

        public void ReadBytes(uint consoleAddress, Span<byte> buffer)
            => _memory.AsSpan(GetOffset(consoleAddress), buffer.Length).CopyTo(buffer);

        public void WriteBytes(uint consoleAddress, ReadOnlySpan<byte> data)
            => data.CopyTo(_memory.AsSpan(GetOffset(consoleAddress), data.Length));

        public void SetByte(uint consoleAddress, byte value)
            => _memory[GetOffset(consoleAddress)] = value;

        public void Dispose()
        {
        }

        private static int GetOffset(uint consoleAddress)
            => checked((int)(consoleAddress - Mem1Start));
    }
}
