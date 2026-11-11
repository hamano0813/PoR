using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels.Fields;

public sealed partial class NumericFieldViewModel : FieldViewModelBase
{
    [ObservableProperty]
    private decimal _value;

    public NumericFieldViewModel(
        GameSession session,
        FieldDefinition definition,
        int? slotIndex,
        string label,
        long value)
        : base(session, definition, slotIndex, label)
    {
        var (minimum, maximum) = ValueCodec.GetRange(definition.Length, definition.Bit);
        Minimum = minimum;
        Maximum = maximum;

        SetLoading(true);
        Value = value;
        SetLoading(false);
    }

    public decimal Minimum { get; }

    public decimal Maximum { get; }

    public void Load(long value)
    {
        SetLoading(true);
        Value = value;
        SetLoading(false);
    }

    [RelayCommand]
    private void Increment()
        => Value = Math.Min(Value + 1, Maximum);

    [RelayCommand]
    private void Decrement()
        => Value = Math.Max(Value - 1, Minimum);

    partial void OnValueChanged(decimal value)
    {
        if (Loading)
        {
            return;
        }

        var clamped = Math.Clamp(value, Minimum, Maximum);
        if (clamped != value)
        {
            SetLoading(true);
            Value = clamped;
            SetLoading(false);
        }

        _ = WriteAsync((long)clamped);
    }
}
