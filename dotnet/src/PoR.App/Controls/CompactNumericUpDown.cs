using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace PoR.App.Controls;

/// <summary>
/// NumericUpDown with the default spinner buttons rearranged into a narrow
/// vertical pair, matching the compact spin boxes of the original tool.
/// </summary>
public sealed class CompactNumericUpDown : NumericUpDown
{
    private const double SpinnerWidth = 18;
    private const double ButtonHeight = 15;
    private const double GlyphWidth = 10;
    private const double GlyphHeight = 5;

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

        panel.Orientation = Orientation.Vertical;
        panel.Width = SpinnerWidth;
        panel.Spacing = 0;

        foreach (var button in panel.Children.OfType<RepeatButton>())
        {
            button.Width = SpinnerWidth;
            button.MinWidth = 0;
            button.MaxWidth = SpinnerWidth;
            button.Height = ButtonHeight;
            button.MinHeight = 0;
            button.MaxHeight = ButtonHeight;
            button.Padding = default;
            button.CornerRadius = new CornerRadius(0);
            button.IsTabStop = false;

            foreach (var icon in button.GetVisualDescendants().OfType<PathIcon>())
            {
                icon.Width = GlyphWidth;
                icon.Height = GlyphHeight;
            }
        }
    }
}
