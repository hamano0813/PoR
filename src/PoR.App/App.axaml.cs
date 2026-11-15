using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PoR.App.ViewModels;
using PoR.App.ViewModels.Fields;
using PoR.App.Views;

namespace PoR.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        InputElement.LostFocusEvent.AddClassHandler<TextBox>(OnTextBoxLostFocus);
        InputElement.KeyDownEvent.AddClassHandler<TextBox>(OnTextBoxKeyDown);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void OnTextBoxLostFocus(TextBox textBox, RoutedEventArgs e)
    {
        if (textBox.DataContext is NumericFieldViewModel field)
        {
            field.CommitText(textBox.Text ?? string.Empty);
        }
    }

    private static void OnTextBoxKeyDown(TextBox textBox, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && textBox.DataContext is NumericFieldViewModel field)
        {
            field.CommitText(textBox.Text ?? string.Empty);
        }
    }
}
