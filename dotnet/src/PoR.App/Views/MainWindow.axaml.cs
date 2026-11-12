using System.Runtime.InteropServices;
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
        HideMaximizeButton();

        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private void HideMaximizeButton()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var handle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var style = GetWindowLongPtr(handle, GwlStyle).ToInt64();
        style &= ~WsMaximizeBox;
        SetWindowLongPtr(handle, GwlStyle, new IntPtr(style));
        SetWindowPos(
            handle,
            IntPtr.Zero,
            0,
            0,
            0,
            0,
            SwpNoSize | SwpNoMove | SwpNoZOrder | SwpFrameChanged);
    }

    private const int GwlStyle = -16;
    private const long WsMaximizeBox = 0x00010000;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpFrameChanged = 0x0020;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }
}
