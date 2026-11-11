using Avalonia.Data;
using Avalonia.Markup.Xaml;
using PoR.App.Services;

namespace PoR.App.Localization;

/// <summary>
/// XAML helper: {loc:Tr Ui_App_Title}
/// </summary>
public sealed class TrExtension : MarkupExtension
{
    public TrExtension()
    {
    }

    public TrExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
        => new Binding
        {
            Source = LocalizationService.Instance,
            Path = $"[{Key}]",
            Mode = BindingMode.OneWay,
        };
}
