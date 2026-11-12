using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
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
        double iconSize = 32)
        : base(session, definition, slotIndex, label)
    {
        Icon = icon;
        IconSize = iconSize;

        SetLoading(true);
        IsChecked = value != 0;
        SetLoading(false);
    }

    public Bitmap? Icon { get; }

    public bool HasIcon => Icon is not null;

    public double IconSize { get; }

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
