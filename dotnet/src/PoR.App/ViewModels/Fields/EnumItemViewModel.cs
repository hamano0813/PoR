using Avalonia.Media.Imaging;

namespace PoR.App.ViewModels.Fields;

public sealed record EnumItemViewModel(
    uint Value,
    string Key,
    string DisplayName,
    Bitmap? Icon)
{
    public bool HasIcon => Icon is not null;
}
