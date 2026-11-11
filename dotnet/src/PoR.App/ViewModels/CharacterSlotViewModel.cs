using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using PoR.App.Services;
using PoR.Core.Game;

namespace PoR.App.ViewModels;

public sealed class CharacterSlotViewModel : ObservableObject
{
    private readonly LocalizationService _localization;

    public CharacterSlotViewModel(
        SlotInfo info,
        LocalizationService localization,
        AssetService assets)
    {
        Info = info;
        _localization = localization;
        Avatar = assets.GetPidIcon(info.PidKey);
    }

    public SlotInfo Info { get; }

    public int Index => Info.Index;

    public string DisplayName => DisplayText(Info.PidTermKey, Info.PidKey);

    public string ClassName => DisplayText(Info.JidTermKey, Info.JidKey);

    public Bitmap? Avatar { get; }

    public void RefreshText()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(ClassName));
    }

    private string DisplayText(string? termKey, string? fallbackKey)
    {
        var text = _localization.Term(termKey);
        return string.IsNullOrWhiteSpace(text)
            ? (fallbackKey ?? _localization.Ui("Ui_Status_Unknown"))
            : text;
    }
}
