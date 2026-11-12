using Avalonia.Controls;

namespace PoR.App.Controls;

/// <summary>
/// Cycles unchecked -> indeterminate -> checked, matching the original
/// Qt three-state action checkbox. The indeterminate state starts the
/// 10 ms action lock in the ViewModel.
/// </summary>
public sealed class TriStateLockCheckBox : CheckBox
{
    // Use the standard CheckBox control theme for the derived control.
    protected override Type StyleKeyOverride => typeof(CheckBox);

    protected override void OnClick()
    {
        IsChecked = IsChecked switch
        {
            false => null,
            null => true,
            _ => false,
        };
    }
}
