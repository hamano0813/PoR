using System.Collections.Concurrent;
using PoR.Core.Game;
using PoR.Core.Memory;
using PoR.Core.Values;

namespace PoR.Game;

/// <summary>
/// Coordinates Dolphin memory, the game catalog, and dynamic mappings.
/// All memory operations are serialized.
/// </summary>
public sealed class GameSession : IAsyncDisposable
{
    private readonly IDolphinMemory _memory;
    private readonly GameDataCatalog _catalog;
    private readonly DolphinMemoryReader _reader;
    private readonly MappingService _mappingService;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ConcurrentDictionary<uint, LockEntry> _locks = new();

    public GameSession(IDolphinMemory memory, GameDataCatalog? catalog = null)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _catalog = catalog ?? GameDataCatalog.Default;
        _reader = new DolphinMemoryReader(_memory);
        _mappingService = new MappingService(_reader);
    }

    public IDolphinMemory Memory => _memory;

    public GameDataCatalog Catalog => _catalog;

    /// <summary>
    /// The most recent snapshot. Successful character writes are applied to it
    /// so the UI does not show stale values when switching characters.
    /// </summary>
    public GameSnapshot? CurrentSnapshot { get; private set; }

    public DolphinStatus Status => _memory.Status;

    public string? StatusMessage => _memory.StatusMessage;

    public event EventHandler<LockFailedEventArgs>? LockFailed;

    public bool Hook() => _memory.Hook();

    public void Unhook() => _memory.Unhook();

    public async Task<GameSnapshot> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Always validate the hook. The emulator may have been closed and
            // restarted since the previous refresh, leaving a stale process
            // handle and MEM1 address behind.
            if (!_memory.Hook() && !await RetryHookAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException(
                    $"Dolphin is not hooked: {_memory.Status}. {_memory.StatusMessage}");
            }

            try
            {
                var snapshot = await Task.Run(BuildSnapshot, cancellationToken).ConfigureAwait(false);
                CurrentSnapshot = snapshot;
                return snapshot;
            }
            catch (MemoryAccessException) when (!cancellationToken.IsCancellationRequested)
            {
                // The game may have restarted after the hook was established.
                // Reconnect once and retry the snapshot.
                _memory.Unhook();
                if (!_memory.Hook())
                {
                    throw;
                }

                var snapshot = await Task.Run(BuildSnapshot, cancellationToken).ConfigureAwait(false);
                CurrentSnapshot = snapshot;
                return snapshot;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task WriteFieldAsync(
        FieldDefinition field,
        int? slotIndex,
        long value,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var address = ResolveAddress(field, slotIndex);
            var buffer = new byte[field.ByteLength];

            // Bit writes are read-modify-write over the whole field.
            if (field.Bit is not null)
            {
                _memory.ReadBytes(address, buffer);
            }

            ValueCodec.Write(buffer, field.Length, value, field.Bit);
            _memory.WriteBytes(address, buffer);

            if (field.Scope == FieldScope.Character && slotIndex is int slot)
            {
                CurrentSnapshot?.Slots.WriteField(field, slot, value);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public long ReadField(FieldDefinition field, int? slotIndex)
    {
        var address = ResolveAddress(field, slotIndex);
        var buffer = new byte[field.ByteLength];
        _memory.ReadBytes(address, buffer);
        return ValueCodec.Read(buffer, field.Length, field.Bit);
    }

    public bool IsLocked(FieldDefinition field, int? slotIndex)
        => _locks.ContainsKey(ResolveAddress(field, slotIndex));

    public async Task StartLockAsync(
        FieldDefinition field,
        int? slotIndex,
        long value,
        TimeSpan? interval = null,
        CancellationToken cancellationToken = default)
    {
        var address = ResolveAddress(field, slotIndex);

        if (_locks.TryRemove(address, out var previous))
        {
            previous.Cancellation.Cancel();
            await previous.Loop.ConfigureAwait(false);
            previous.Cancellation.Dispose();
        }

        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var entry = new LockEntry(cancellation);
        _locks[address] = entry;

        entry.Loop = Task.Run(
            async () =>
            {
                using var timer = new PeriodicTimer(
                    interval ?? TimeSpan.FromMilliseconds(10));
                try
                {
                    while (await timer.WaitForNextTickAsync(cancellation.Token)
                               .ConfigureAwait(false))
                    {
                        await WriteFieldAsync(
                                field,
                                slotIndex,
                                value,
                                cancellation.Token)
                            .ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal stop.
                }
                catch (Exception exception)
                {
                    _locks.TryRemove(address, out _);
                    LockFailed?.Invoke(this, new LockFailedEventArgs(address, exception));
                }
            },
            cancellation.Token);

        await Task.CompletedTask;
    }

    public async Task StopLockAsync(
        FieldDefinition field,
        int? slotIndex,
        long? finalValue = null,
        CancellationToken cancellationToken = default)
    {
        var address = ResolveAddress(field, slotIndex);

        if (_locks.TryRemove(address, out var entry))
        {
            entry.Cancellation.Cancel();
            try
            {
                await entry.Loop.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Normal stop.
            }

            entry.Cancellation.Dispose();
        }

        if (finalValue is { } value)
        {
            await WriteFieldAsync(field, slotIndex, value, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task StopAllLocksAsync()
    {
        foreach (var address in _locks.Keys.ToArray())
        {
            if (_locks.TryRemove(address, out var entry))
            {
                entry.Cancellation.Cancel();
                try
                {
                    await entry.Loop.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Normal stop.
                }

                entry.Cancellation.Dispose();
            }
        }
    }

    public uint ResolveAddress(FieldDefinition field, int? slotIndex)
    {
        if (field.Scope == FieldScope.Global)
        {
            return field.Address;
        }

        if (slotIndex is null)
        {
            throw new ArgumentNullException(
                nameof(slotIndex),
                $"Character field '{field.Id}' needs a slot index.");
        }

        if (slotIndex < 0 || slotIndex >= _catalog.Profile.SlotCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slotIndex),
                slotIndex,
                $"Slot index must be 0..{_catalog.Profile.SlotCount - 1}.");
        }

        return field.Address + (uint)(slotIndex.Value * _catalog.Profile.SlotStep);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAllLocksAsync().ConfigureAwait(false);
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            _gate.Release();
        }
        finally
        {
            _gate.Dispose();
        }
    }

    private sealed class LockEntry
    {
        public LockEntry(CancellationTokenSource cancellation)
        {
            Cancellation = cancellation;
        }

        public CancellationTokenSource Cancellation { get; }

        public Task Loop { get; set; } = Task.CompletedTask;
    }

    private async Task<bool> RetryHookAsync(CancellationToken cancellationToken)
    {
        if (_memory.Status != DolphinStatus.NoEmu)
        {
            return false;
        }

        // The Dolphin process may be up while the game is still creating MEM1.
        await Task.Delay(300, cancellationToken).ConfigureAwait(false);
        return _memory.Hook();
    }

    private GameSnapshot BuildSnapshot()
    {
        var profile = _catalog.Profile;
        var buffer = new byte[profile.SlotCount * profile.SlotStep];
        _memory.ReadBytes(profile.SlotBase, buffer);

        var slots = new SlotSnapshot(
            profile.SlotBase,
            profile.SlotCount,
            profile.SlotStep,
            buffer);

        var globalValues = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var field in _catalog.Fields.Values)
        {
            if (field.Scope != FieldScope.Global)
            {
                continue;
            }

            var fieldBuffer = new byte[field.ByteLength];
            _memory.ReadBytes(field.Address, fieldBuffer);
            globalValues[field.Id] = ValueCodec.Read(fieldBuffer, field.Length, field.Bit);
        }

        var mappings = new Dictionary<string, IReadOnlyList<MappingItem>>(StringComparer.Ordinal);

        foreach (var (id, definition) in _catalog.PointerMappings)
        {
            mappings[id] = _mappingService.GetPointerMapping(
                definition,
                _catalog.Enums[definition.EnumId],
                profile);
        }

        foreach (var (id, definition) in _catalog.TextMappings)
        {
            mappings[id] = _mappingService.GetTextMapping(
                definition,
                _catalog.Enums[definition.EnumId]);
        }

        mappings["group"] = _mappingService.GetGroupMapping(_catalog.Groups);
        mappings["army"] = _mappingService.GetArmyMapping(_catalog.Armies);

        var pidMapping = mappings.GetValueOrDefault("pid") ?? [];
        var jidMapping = mappings.GetValueOrDefault("jid") ?? [];
        var personField = _catalog.GetField("person");
        var classField = _catalog.GetField("class");

        var slotInfos = new List<SlotInfo>(profile.SlotCount);
        var allyMapping = new List<MappingItem>
        {
            // A zero ally pointer means no co-op partner.
            new(0, "SLOT_NONE", "——"),
        };
        for (var index = 0; index < profile.SlotCount; index++)
        {
            var personPointer = (uint)slots.ReadField(personField, index);
            var classPointer = (uint)slots.ReadField(classField, index);
            var pid = pidMapping.FirstOrDefault(item => item.Value == personPointer);
            var jid = jidMapping.FirstOrDefault(item => item.Value == classPointer);
            var slotAddress = profile.SlotBase + (uint)(index * profile.SlotStep);

            slotInfos.Add(
                new SlotInfo(
                    index,
                    slotAddress,
                    personPointer,
                    classPointer,
                    pid?.Key,
                    pid?.TermKey,
                    jid?.Key,
                    jid?.TermKey));

            if (personPointer != 0 && pid is not null)
            {
                allyMapping.Add(
                    new MappingItem(
                        slotAddress,
                        pid.Key,
                        pid.TermKey,
                        pid.IconKey));
            }
        }

        mappings["ally"] = allyMapping;

        return new GameSnapshot(
            slots,
            slotInfos,
            mappings,
            globalValues,
            DateTimeOffset.Now);
    }
}
