using System.ComponentModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
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

                var partnerAffinity = session.Catalog.Affinities.Find(entry.PartnerKey);
                rows.Add(new SupportRowViewModel(
                    Term(entry.PartnerKey, null, pidTermKeys),
                    _assets.GetAffinityIcon(partnerAffinity),
                    points,
                    entry.Thresholds));
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
}

public sealed class SupportRowViewModel : ObservableObject
{
    public SupportRowViewModel(
        string partnerName,
        Bitmap? affinityIcon,
        NumericFieldViewModel points,
        IReadOnlyList<int> thresholds)
    {
        PartnerName = partnerName;
        AffinityIcon = affinityIcon;
        Points = points;
        Meter = new SupportMeterViewModel(thresholds);

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
        }
    }

    private void UpdateMeter()
    {
        Meter.Update((double)Points.Value);
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

    public SupportMeterViewModel(IReadOnlyList<int> thresholds)
    {
        _cThreshold = thresholds[0];
        _bThreshold = thresholds[1];
        _aThreshold = thresholds[2];

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

    public void Update(double points)
    {
        Value = Math.Clamp(points, 0, Maximum);
        RankLabel = points >= _aThreshold
            ? "A"
            : points >= _bThreshold
                ? "B"
                : points >= _cThreshold
                    ? "C"
                    : string.Empty;
        HasRankLabel = RankLabel.Length > 0;
    }

    partial void OnValueChanged(double value)
    {
        OnPropertyChanged(nameof(FillWidth));
    }
}
