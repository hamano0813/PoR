using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using PoR.App.Services;
using PoR.Core.Game;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels.Fields;

public sealed partial class EnumFieldViewModel : FieldViewModelBase
{
    [ObservableProperty]
    private EnumItemViewModel? _selectedItem;

    public EnumFieldViewModel(
        GameSession session,
        FieldDefinition definition,
        int? slotIndex,
        string label,
        IReadOnlyList<MappingItem> mapping,
        LocalizationService localization,
        AssetService assets,
        long value)
        : base(session, definition, slotIndex, label)
    {
        Items = mapping
            .Select(item => new EnumItemViewModel(
                item.Value,
                item.Key,
                localization.Term(item.TermKey),
                GetIcon(definition.EnumId, item.Key, assets)))
            .ToList();

        SetLoading(true);
        SelectedItem = Items.FirstOrDefault(item => item.Value == (uint)value);
        SetLoading(false);
    }

    public IReadOnlyList<EnumItemViewModel> Items { get; }

    public void Load(long value)
    {
        SetLoading(true);
        SelectedItem = Items.FirstOrDefault(item => item.Value == (uint)value);
        SetLoading(false);
    }

    partial void OnSelectedItemChanged(EnumItemViewModel? value)
    {
        if (!Loading && value is not null)
        {
            _ = WriteAsync(value.Value);
        }
    }

    private static Bitmap? GetIcon(string? enumId, string key, AssetService assets)
        => enumId switch
        {
            "pid" => assets.GetPidIcon(key),
            "iid" => assets.GetItemIcon(key),
            "sid" => assets.GetSkillIcon(key),
            _ => null,
        };
}
