using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using PoR.App.Services;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels.Fields;

public sealed partial class BoolFieldViewModel : FieldViewModelBase
{
    [ObservableProperty]
    private bool _isChecked;

    public BoolFieldViewModel(
        GameSession session,
        FieldDefinition definition,
        int? slotIndex,
        string label,
        long value,
        Bitmap? icon = null,
        double iconSize = 32,
        bool useGameTermFont = false)
        : base(session, definition, slotIndex, label)
    {
        Icon = icon;
        IconSize = iconSize;

        var japanese = string.Equals(
            LocalizationService.Instance.Language,
            "jp",
            StringComparison.Ordinal);
        LabelFontFamily = new FontFamily(
            useGameTermFont && japanese ? "Yu Gothic UI" : "Microsoft YaHei UI");

        SetLoading(true);
        IsChecked = value != 0;
        SetLoading(false);
    }

    public Bitmap? Icon { get; }

    public bool HasIcon => Icon is not null;

    public double IconSize { get; }

    public FontFamily LabelFontFamily { get; }

    public void Load(long value)
    {
        SetLoading(true);
        IsChecked = value != 0;
        SetLoading(false);
    }

    partial void OnIsCheckedChanged(bool value)
    {
        if (!Loading)
        {
            _ = WriteAsync(value ? 1 : 0);
        }
    }
}
