using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace PoR.App.Controls;

/// <summary>
/// FluentAvalonia NumericUpDown with a compact vertical spinner.
/// The buttons are moved into a narrow right-hand gutter with the
/// increase button above the decrease button.
/// </summary>
public sealed class CompactNumericUpDown : NumericUpDown
{
    private const double SpinnerWidth = 18;
    private const double ButtonHeight = 15;

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
        panel.VerticalAlignment = VerticalAlignment.Center;

        foreach (var button in panel.Children.OfType<RepeatButton>())
        {
            button.MinWidth = 0;
            button.Width = SpinnerWidth;
            button.MaxWidth = SpinnerWidth;
            button.MinHeight = 0;
            button.Height = ButtonHeight;
            button.MaxHeight = ButtonHeight;
            button.Margin = default;
            button.Padding = default;
            button.FontSize = 10;
            button.HorizontalContentAlignment = HorizontalAlignment.Center;
            button.VerticalContentAlignment = VerticalAlignment.Center;
            button.IsTabStop = false;
        }
    }
}
