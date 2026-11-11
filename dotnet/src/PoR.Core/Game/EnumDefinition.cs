namespace PoR.Core.Game;

public sealed record EnumEntry(string Key, string TermKey);

public sealed class EnumDefinition
{
    private readonly Dictionary<string, int> _indexByKey;

    public EnumDefinition(string id, IReadOnlyList<EnumEntry> entries)
    {
        Id = id;
        Entries = entries;
        _indexByKey = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < entries.Count; index++)
        {
            if (!_indexByKey.TryAdd(entries[index].Key, index))
            {
                throw new ArgumentException(
                    $"Duplicate enum key '{entries[index].Key}' in {id}.",
                    nameof(entries));
            }
        }
    }

    public string Id { get; }

    public IReadOnlyList<EnumEntry> Entries { get; }

    public bool TryGetIndex(string key, out int index)
        => _indexByKey.TryGetValue(key, out index);

    public int GetIndex(string key)
        => _indexByKey.TryGetValue(key, out var index)
            ? index
            : throw new KeyNotFoundException($"Enum {Id} has no key '{key}'.");
}
