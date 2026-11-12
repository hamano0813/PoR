using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Windowing;
using PoR.App.ViewModels;

namespace PoR.App.Views;

public partial class MainWindow : FAAppWindow
{
    public MainWindow()
    {
        InitializeComponent();
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
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }
}
