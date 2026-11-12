using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

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
}
