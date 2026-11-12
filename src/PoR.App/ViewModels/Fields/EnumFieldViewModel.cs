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
        var isPortrait = string.Equals(definition.EnumId, "pid", StringComparison.Ordinal);
        var isItem = string.Equals(definition.EnumId, "iid", StringComparison.Ordinal);
        IconSize = isPortrait ? 100 : isItem ? 24 : 32;
        EditorHeight = isPortrait ? 112 : 32;
        DropDownHeight = isPortrait ? 540 : 320;

        Items = mapping
            .Select(item => new EnumItemViewModel(
                item.Value,
                item.Key,
                localization.Term(item.TermKey),
                GetIcon(definition.EnumId, item.Key, assets),
                IconSize))
            .ToList();

        SetLoading(true);
        SelectedItem = Items.FirstOrDefault(item => item.Value == (uint)value);
        SetLoading(false);
    }

    public IReadOnlyList<EnumItemViewModel> Items { get; }

    public double IconSize { get; }

    public double EditorHeight { get; }

    public double DropDownHeight { get; }

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
