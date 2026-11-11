using CommunityToolkit.Mvvm.ComponentModel;
using PoR.App.Services;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels.Fields;

public sealed partial class TriStateLockFieldViewModel : FieldViewModelBase
{
    private readonly LocalizationService _localization;

    [ObservableProperty]
    private bool? _stateValue;

    public TriStateLockFieldViewModel(
        GameSession session,
        FieldDefinition definition,
        int? slotIndex,
        LocalizationService localization,
        long value)
        : base(session, definition, slotIndex, localization.Ui($"Field_{definition.Id}"))
    {
        _localization = localization;

        SetLoading(true);
        StateValue = session.IsLocked(definition, slotIndex) ? null : value != 0;
        SetLoading(false);
    }

    public string DisplayText => StateValue switch
    {
        true => _localization.Ui("Choice_Acted"),
        false => _localization.Ui("Choice_NotActed"),
        _ => _localization.Ui("Choice_Locked"),
    };

    public void Load(long value)
    {
        SetLoading(true);
        StateValue = Session.IsLocked(Definition, SlotIndex) ? null : value != 0;
        SetLoading(false);
    }

    partial void OnStateValueChanged(bool? value)
    {
        OnPropertyChanged(nameof(DisplayText));

        if (!Loading)
        {
            _ = ApplyStateAsync(value);
        }
    }

    private async Task ApplyStateAsync(bool? value)
    {
        try
        {
            if (value is null)
            {
                await Session.StartLockAsync(
                    Definition,
                    SlotIndex,
                    0,
                    TimeSpan.FromMilliseconds(10));
            }
            else
            {
                await Session.StopLockAsync(
                    Definition,
                    SlotIndex,
                    value.Value ? 1 : 0);
            }
        }
        catch (Exception exception)
        {
            SetError(exception.Message);
        }
    }
}
