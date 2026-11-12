using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace PoR.App.Controls;

/// <summary>
/// TabControl without the FluentAvalonia template's 12px side padding
/// around the selected page.
/// </summary>
public sealed class CompactTabControl : TabControl
{
    protected override Type StyleKeyOverride => typeof(TabControl);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (e.NameScope.Find<ContentPresenter>("PART_SelectedContentHost") is { } host)
        {
            host.Margin = default;
            host.Padding = default;
        }
    }
}
