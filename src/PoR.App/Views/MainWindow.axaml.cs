using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using AvaloniaFluentUI.Windowing;
using PoR.App.ViewModels;

namespace PoR.App.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        TitleBarTemplateSettings.IsVisible = false;
        SplashScreen = new MainWindowSplashScreen(
            () => TitleBarTemplateSettings.IsVisible = true);

        AddHandler(PointerPressedEvent, OnWindowPointerPressed, RoutingStrategies.Tunnel);
        Opened += OnOpened;
        Closed += OnClosed;
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control source || IsInteractive(source))
        {
            return;
        }

        if (FocusManager?.GetFocusedElement() is not Control focused ||
            focused == RootLayout ||
            focused.FindAncestorOfType<TextBox>(includeSelf: true) is null)
        {
            return;
        }

        RootLayout.Focus();
    }

    private static bool IsInteractive(Control source)
        => source.FindAncestorOfType<TextBox>(includeSelf: true) is not null
           || source.FindAncestorOfType<ComboBox>(includeSelf: true) is not null
           || source.FindAncestorOfType<Button>(includeSelf: true) is not null
           || source.FindAncestorOfType<TabItem>(includeSelf: true) is not null
           || source.FindAncestorOfType<ListBoxItem>(includeSelf: true) is not null
           || source.FindAncestorOfType<ScrollBar>(includeSelf: true) is not null
           || source.FindAncestorOfType<Slider>(includeSelf: true) is not null;

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }

        RootLayout.Focus();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }
}

internal sealed class MainWindowSplashScreen : IApplicationSplashScreen
{
    private readonly Action _action;

    public MainWindowSplashScreen(Action action)
    {
        _action = action;
    }

    public object SplashScreenContent => new Image
    {
        Source = new Bitmap(AssetLoader.Open(new Uri("avares://PoR.App/Assets/splash.png"))),
        Stretch = Stretch.Uniform,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
    };

    public async Task RunTasks(CancellationToken cancellationToken)
    {
        await Task.Delay(2000, cancellationToken);
        _action();
    }

    public int MinimumShowTime => 2000;
}
