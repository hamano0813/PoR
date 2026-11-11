using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace PoR.App.Controls;

/// <summary>
/// FluentAvalonia NumericUpDown with a narrower spinner strip.
/// The buttons stay horizontal (the FA template layout), but are reduced
/// from 32px to 14px wide with a smaller glyph.
/// </summary>
public sealed class CompactNumericUpDown : NumericUpDown
{
    private const double ButtonWidth = 14;

    public CompactNumericUpDown()
    {
        Loaded += (_, _) => CompactSpinner();
    }

    protected override Type StyleKeyOverride => typeof(NumericUpDown);

    private void CompactSpinner()
    {
        var panel = this.GetVisualDescendants()
            .OfType<StackPanel>()
            .FirstOrDefault(panel => panel.Name == "PART_SpinnerPanel");
        if (panel is null)
        {
            return;
        }

        panel.Spacing = 0;

        foreach (var button in panel.Children.OfType<RepeatButton>())
        {
            button.MinWidth = 0;
            button.Width = ButtonWidth;
            button.MaxWidth = ButtonWidth;
            button.MinHeight = 0;
            button.Margin = new Thickness(1, 2, 1, 2);
            button.Padding = default;
            button.FontSize = 10;
            button.HorizontalContentAlignment = HorizontalAlignment.Center;
            button.VerticalContentAlignment = VerticalAlignment.Center;
            button.IsTabStop = false;
        }
    }
}
