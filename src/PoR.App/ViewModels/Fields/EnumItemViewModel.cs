using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PoR.App.ViewModels.Fields;

public sealed record EnumItemViewModel(
    uint Value,
    string Key,
    string DisplayName,
    Bitmap? Icon,
    double IconSize,
    FontFamily FontFamily,
    double FontSize)
{
    public bool HasIcon => Icon is not null;
}
