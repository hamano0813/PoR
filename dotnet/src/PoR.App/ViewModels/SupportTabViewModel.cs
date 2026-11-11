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

        C = new SupportProgressViewModel("C", 0, thresholds[0] + 1);
        B = new SupportProgressViewModel("B", thresholds[0] + 1, thresholds[1] + 2);
        A = new SupportProgressViewModel("A", thresholds[1] + 2, thresholds[2] + 3);

        Points.PropertyChanged += OnPointsChanged;
        UpdateBars();
    }

    public string PartnerName { get; }

    public Bitmap? AffinityIcon { get; }

    public bool HasAffinity => AffinityIcon is not null;

    public NumericFieldViewModel Points { get; }

    public SupportProgressViewModel C { get; }

    public SupportProgressViewModel B { get; }

    public SupportProgressViewModel A { get; }

    private void OnPointsChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(NumericFieldViewModel.Value))
        {
            UpdateBars();
        }
    }

    private void UpdateBars()
    {
        var value = (double)Points.Value;
        C.Value = Math.Clamp(value, C.Minimum, C.Maximum);
        B.Value = Math.Clamp(value, B.Minimum, B.Maximum);
        A.Value = Math.Clamp(value, A.Minimum, A.Maximum);
    }
}

public sealed partial class SupportProgressViewModel : ObservableObject
{
    public SupportProgressViewModel(string label, double minimum, double maximum)
    {
        Label = label;
        Minimum = minimum;
        Maximum = maximum;
    }

    public string Label { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    [ObservableProperty]
    private double _value;
}
