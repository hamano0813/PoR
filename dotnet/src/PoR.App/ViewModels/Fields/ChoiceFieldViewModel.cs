using CommunityToolkit.Mvvm.ComponentModel;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels.Fields;

public sealed record ChoiceItem(long Value, string Label);

public sealed partial class ChoiceFieldViewModel : FieldViewModelBase
{
    [ObservableProperty]
    private ChoiceItem? _selectedItem;

    public ChoiceFieldViewModel(
        GameSession session,
        FieldDefinition definition,
        int? slotIndex,
        string label,
        IReadOnlyList<ChoiceItem> items,
        long value)
        : base(session, definition, slotIndex, label)
    {
        Items = items;

        SetLoading(true);
        SelectedItem = items.FirstOrDefault(item => item.Value == value);
        SetLoading(false);
    }

    public IReadOnlyList<ChoiceItem> Items { get; }

    public void Load(long value)
    {
        SetLoading(true);
        SelectedItem = Items.FirstOrDefault(item => item.Value == value);
        SetLoading(false);
    }

    partial void OnSelectedItemChanged(ChoiceItem? value)
    {
        if (!Loading && value is not null)
        {
            _ = WriteAsync(value.Value);
        }
    }
}
