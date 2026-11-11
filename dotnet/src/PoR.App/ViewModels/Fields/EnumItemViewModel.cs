using Avalonia.Media.Imaging;

namespace PoR.App.ViewModels.Fields;

public sealed record EnumItemViewModel(
    uint Value,
    string Key,
    string DisplayName,
    Bitmap? Icon,
    double IconSize)
{
    public bool HasIcon => Icon is not null;
}
