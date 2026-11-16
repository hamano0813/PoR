using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PoR.App.ViewModels;

namespace PoR.App.Views;

public partial class StatusView : UserControl
{
    public StatusView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // 初始焦点可能落在页面内某个编辑控件上，ScrollViewer 会自动把该控件带入视野；
        // 这里在布局完成后把状态页恢复到顶部，保证“状态”标题可见。
        Dispatcher.UIThread.Post(
            () => StatusScroll.Offset = default,
            DispatcherPriority.Background);
    }

    private void OnSupportMeterPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control ||
            !e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (control.DataContext is not SupportMeterViewModel meter ||
            string.IsNullOrWhiteSpace(meter.BonusText))
        {
            return;
        }

        if (FlyoutBase.GetAttachedFlyout(control) is Flyout flyout &&
            flyout.Content is Control content)
        {
            // The flyout is hosted in a popup, so pass the meter's data
            // context through explicitly to keep BonusText bound.
            content.DataContext = control.DataContext;
        }

        FlyoutBase.ShowAttachedFlyout(control);
        e.Handled = true;
    }
}
