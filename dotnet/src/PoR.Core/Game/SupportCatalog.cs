namespace PoR.Core.Game;

public sealed record SupportEntry(string PartnerKey, IReadOnlyList<int> Thresholds);

public sealed record SupportRow(string PidKey, IReadOnlyList<SupportEntry?> Entries);

public sealed class SupportCatalog
{
    private readonly IReadOnlyDictionary<string, SupportRow> _byPid;

    public SupportCatalog(IReadOnlyList<SupportRow> rows)
    {
        Rows = rows;
        _byPid = rows.ToDictionary(row => row.PidKey, StringComparer.Ordinal);
    }

    public IReadOnlyList<SupportRow> Rows { get; }

    public SupportRow? Find(string pidKey)
        => _byPid.GetValueOrDefault(pidKey);
}

public sealed record AffinityEntry(string PidKey, string AffinityKey);

public sealed class AffinityCatalog
{
    private readonly IReadOnlyDictionary<string, string> _byPid;

    public AffinityCatalog(IReadOnlyList<AffinityEntry> entries)
    {
        Entries = entries;
        _byPid = entries.ToDictionary(entry => entry.PidKey, entry => entry.AffinityKey, StringComparer.Ordinal);
    }

    public IReadOnlyList<AffinityEntry> Entries { get; }

    public string? Find(string pidKey)
        => _byPid.GetValueOrDefault(pidKey);
}

public sealed record SkillCatalog(
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> Masteries,
    IReadOnlyList<string> Features);
