using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels.Fields;

public sealed partial class NumericFieldViewModel : FieldViewModelBase
{
    [ObservableProperty]
    private decimal _value;

    [ObservableProperty]
    private bool _isValueEnabled = true;

    public string Text
    {
        get => Value.ToString(CultureInfo.InvariantCulture);
        set => CommitText(value);
    }

    public void CommitText(string input)
    {
        if (Loading)
        {
            return;
        }

        if (!decimal.TryParse(
                input,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsed))
        {
            OnPropertyChanged(nameof(Text));
            return;
        }

        var clamped = Math.Clamp(parsed, Minimum, Maximum);
        if (clamped != Value)
        {
            Value = clamped;
        }
        else
        {
            OnPropertyChanged(nameof(Text));
        }
    }

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

    public decimal Minimum { get; private set; }

    public decimal Maximum { get; private set; }

    public bool CanEdit => IsEditable && IsValueEnabled;

    public void SetRange(decimal minimum, decimal maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public void Load(long value)
    {
        SetLoading(true);
        Value = value;
        SetLoading(false);
        OnPropertyChanged(nameof(Text));
    }

    /// <summary>
    /// Sets a rule value without applying the editor range. Used when a feature
    /// is checked/unchecked and the proficiency must be forced to 1 or 0.
    /// </summary>
    public void SetValueDirect(long value)
    {
        SetLoading(true);
        Value = value;
        SetLoading(false);
        OnPropertyChanged(nameof(Text));
        _ = WriteAsync(value);
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
        OnPropertyChanged(nameof(Text));
    }

    partial void OnIsValueEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(CanEdit));
    }
}
