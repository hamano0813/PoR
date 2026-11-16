using System.ComponentModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels;

public sealed class SupportTabViewModel
{
    private readonly LocalizationService _localization;
    private readonly AssetService _assets;

    public SupportTabViewModel(
        GameSnapshot snapshot,
        SlotInfo slot,
        GameSession session,
        LocalizationService localization,
        AssetService assets)
    {
        _localization = localization;
        _assets = assets;

        var pidTermKeys = session.Catalog.Enums["pid"].Entries
            .ToDictionary(entry => entry.Key, entry => entry.TermKey, StringComparer.Ordinal);

        CurrentName = Term(slot.PidKey, slot.PidTermKey, pidTermKeys);
        var currentAffinity = session.Catalog.Affinities.Find(slot.PidKey ?? "");
        CurrentAffinityIcon = _assets.GetAffinityIcon(currentAffinity);
        HasCurrentAffinity = currentAffinity is not null;

        var support = slot.PidKey is null
            ? null
            : session.Catalog.Supports.Find(slot.PidKey);
        var rows = new List<SupportRowViewModel>();

        if (support is not null)
        {
            for (var index = 0; index < support.Entries.Count; index++)
            {
                if (support.Entries[index] is not { } entry)
                {
                    continue;
                }

                var field = session.Catalog.GetField($"support{index + 1}");
                var points = new NumericFieldViewModel(
                    session,
                    field,
                    slot.Index,
                    _localization.Ui("Ui_Support_Points"),
                    snapshot.Slots.ReadField(field, slot.Index));
                points.SetRange(0, 30);

                var partnerAffinity = session.Catalog.Affinities.Find(entry.PartnerKey);
                var partnerSlot = snapshot.SlotInfos.FirstOrDefault(
                    info => !info.IsEmpty &&
                            string.Equals(
                                info.PidKey,
                                entry.PartnerKey,
                                StringComparison.Ordinal));
                var partnerSupportIndex = FindSupportIndex(
                    session,
                    entry.PartnerKey,
                    slot.PidKey);
                var partnerField = partnerSupportIndex is int supportIndex
                    ? session.Catalog.GetField($"support{supportIndex + 1}")
                    : null;

                rows.Add(new SupportRowViewModel(
                    Term(entry.PartnerKey, null, pidTermKeys),
                    _assets.GetAffinityIcon(partnerAffinity),
                    points,
                    entry.Thresholds,
                    session,
                    partnerField,
                    partnerSlot?.Index,
                    currentAffinity,
                    partnerAffinity,
                    _localization));
            }
        }

        Rows = rows;
        EmptyText = _localization.Ui("Ui_Support_Empty");
    }

    public IReadOnlyList<SupportRowViewModel> Rows { get; }

    public Bitmap? CurrentAffinityIcon { get; }

    public bool HasCurrentAffinity { get; }

    public string CurrentName { get; }

    public bool HasRows => Rows.Count > 0;

    public bool IsEmpty => !HasRows;

    public string EmptyText { get; }

    private string Term(
        string? key,
        string? termKey,
        IReadOnlyDictionary<string, string> pidTermKeys)
    {
        var resolved = termKey;
        if (resolved is null && key is not null)
        {
            pidTermKeys.TryGetValue(key, out resolved);
        }

        var text = _localization.Term(resolved);
        return string.IsNullOrWhiteSpace(text)
            ? key ?? _localization.Ui("Ui_Status_Unknown")
            : text;
    }

    private static int? FindSupportIndex(
        GameSession session,
        string partnerPid,
        string? currentPid)
    {
        if (currentPid is null)
        {
            return null;
        }

        var support = session.Catalog.Supports.Find(partnerPid);
        if (support is null)
        {
            return null;
        }

        for (var index = 0; index < support.Entries.Count; index++)
        {
            if (support.Entries[index] is { } entry &&
                string.Equals(entry.PartnerKey, currentPid, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return null;
    }
}

public sealed class SupportRowViewModel : ObservableObject
{
    private readonly GameSession _session;
    private readonly FieldDefinition? _partnerField;
    private readonly int? _partnerSlotIndex;

    public SupportRowViewModel(
        string partnerName,
        Bitmap? affinityIcon,
        NumericFieldViewModel points,
        IReadOnlyList<int> thresholds,
        GameSession session,
        FieldDefinition? partnerField,
        int? partnerSlotIndex,
        string? currentAffinity,
        string? partnerAffinity,
        LocalizationService localization)
    {
        PartnerName = partnerName;
        AffinityIcon = affinityIcon;
        Points = points;
        _session = session;
        _partnerField = partnerField;
        _partnerSlotIndex = partnerSlotIndex;
        Meter = new SupportMeterViewModel(
            thresholds,
            currentAffinity,
            partnerAffinity,
            localization);

        Points.PropertyChanged += OnPointsChanged;
        UpdateMeter();
    }

    public string PartnerName { get; }

    public Bitmap? AffinityIcon { get; }

    public bool HasAffinity => AffinityIcon is not null;

    public NumericFieldViewModel Points { get; }

    public SupportMeterViewModel Meter { get; }

    private void OnPointsChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(NumericFieldViewModel.Value))
        {
            UpdateMeter();
            WritePartnerValue();
        }
    }

    private void UpdateMeter()
    {
        Meter.Update((double)Points.Value);
    }

    private void WritePartnerValue()
    {
        if (_partnerField is null || _partnerSlotIndex is not int slotIndex)
        {
            return;
        }

        _ = WritePartnerValueAsync(_partnerField, slotIndex);
    }

    private async Task WritePartnerValueAsync(FieldDefinition field, int slotIndex)
    {
        try
        {
            await _session.WriteFieldAsync(field, slotIndex, (long)Points.Value);
        }
        catch
        {
            // The primary field reports its own write errors; a failed linked
            // write should not interrupt the current value edit.
        }
    }
}

/// <summary>
/// 单条累计支援值进度：以 A 级阈值为满值，C/B 阈值处画刻度，
/// 并在进度条上显示当前已达到的最高等级。
/// </summary>
public sealed partial class SupportMeterViewModel : ObservableObject
{
    // 进度条右侧对齐三级进度区域，左侧留出 8px 与支援值微调框分隔。
    private const double TrackWidth = 316d;

    private readonly int _cThreshold;
    private readonly int _bThreshold;
    private readonly int _aThreshold;
    private readonly string? _currentAffinity;
    private readonly string? _partnerAffinity;
    private readonly string _attackLabel;
    private readonly string _defenseLabel;
    private readonly string _hitLabel;
    private readonly string _avoidLabel;

    public SupportMeterViewModel(
        IReadOnlyList<int> thresholds,
        string? currentAffinity,
        string? partnerAffinity,
        LocalizationService localization)
    {
        // support.json 存的是各档之间的累计差值，实际要严格超过该值才升档：
        // C 需要 >C，B 需要 >C+B，A 需要 >C+B+A。+1/+2/+3 后得到各档的
        // 第一个有效点数（9/13/29 等）。
        _cThreshold = thresholds[0] + 1;
        _bThreshold = thresholds[1] + 2;
        _aThreshold = thresholds[2] + 3;
        _currentAffinity = currentAffinity;
        _partnerAffinity = partnerAffinity;
        _attackLabel = localization.Ui("Ui_Support_Atk");
        _defenseLabel = localization.Ui("Ui_Support_Def");
        _hitLabel = localization.Ui("Ui_Support_Hit");
        _avoidLabel = localization.Ui("Ui_Support_Avoid");

        Maximum = _aThreshold;
        var span = Math.Max(1, _aThreshold);
        CTickLeft = TrackWidth * _cThreshold / span;
        BTickLeft = TrackWidth * _bThreshold / span;
        ToolTipText = $"C：{_cThreshold}    B：{_bThreshold}    A：{_aThreshold}";
    }

    public double Maximum { get; }

    public double CTickLeft { get; }

    public double BTickLeft { get; }

    public double FillWidth => Maximum <= 0 ? 0 : TrackWidth * Value / Maximum;

    [ObservableProperty]
    private double _value;

    [ObservableProperty]
    private string _rankLabel = string.Empty;

    [ObservableProperty]
    private bool _hasRankLabel;

    [ObservableProperty]
    private string _toolTipText = string.Empty;

    [ObservableProperty]
    private string _bonusText = string.Empty;

    public void Update(double points)
    {
        Value = Math.Clamp(points, 0, Maximum);
        var level = points >= _aThreshold
            ? 3
            : points >= _bThreshold
                ? 2
                : points >= _cThreshold
                    ? 1
                    : 0;
        RankLabel = level switch
        {
            3 => "A",
            2 => "B",
            1 => "C",
            _ => string.Empty,
        };
        HasRankLabel = level > 0;
        BonusText = FormatBonus(level);
    }

    private string FormatBonus(int level)
    {
        if (level <= 0)
        {
            return string.Empty;
        }

        var bonus = AffinitySupportCatalog.Calculate(
            _currentAffinity,
            _partnerAffinity,
            level);
        var parts = new List<string>(4);
        if (bonus.Attack > 0)
        {
            parts.Add($"{_attackLabel}+{bonus.Attack}");
        }

        if (bonus.Defense > 0)
        {
            parts.Add($"{_defenseLabel}+{bonus.Defense}");
        }

        if (bonus.Hit > 0)
        {
            parts.Add($"{_hitLabel}+{bonus.Hit}");
        }

        if (bonus.Avoid > 0)
        {
            parts.Add($"{_avoidLabel}+{bonus.Avoid}");
        }

        return parts.Count == 0 ? string.Empty : string.Join(" ", parts);
    }

    partial void OnValueChanged(double value)
    {
        OnPropertyChanged(nameof(FillWidth));
    }
}
