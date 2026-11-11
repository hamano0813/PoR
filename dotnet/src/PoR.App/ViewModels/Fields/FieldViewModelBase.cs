using CommunityToolkit.Mvvm.ComponentModel;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels.Fields;

public abstract partial class FieldViewModelBase : ObservableObject
{
    private bool _loading;

    protected FieldViewModelBase(
        GameSession session,
        FieldDefinition definition,
        int? slotIndex,
        string label)
    {
        Session = session;
        Definition = definition;
        SlotIndex = slotIndex;
        Label = label;
    }

    protected GameSession Session { get; }

    protected FieldDefinition Definition { get; }

    protected int? SlotIndex { get; }

    protected bool Loading => _loading;

    public string Label { get; }

    public bool IsEditable => !Definition.ReadOnly;

    public string? ErrorMessage { get; private set; }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    protected void SetLoading(bool value) => _loading = value;

    protected async Task WriteAsync(long value)
    {
        if (_loading)
        {
            return;
        }

        try
        {
            await Session.WriteFieldAsync(Definition, SlotIndex, value);
            ErrorMessage = null;
            OnPropertyChanged(nameof(HasError));
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            OnPropertyChanged(nameof(HasError));
        }
    }
}
