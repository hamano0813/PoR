using Avalonia.Media.Imaging;
using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
using PoR.Game;

namespace PoR.App.ViewModels;

public sealed class AbilityTabViewModel
{
    private static readonly (string LabelKey, string StatId, string? GrowthId, string? WeaponKey)[] RowDefinitions =
    [
        ("Ui_Ability_HP", "stat.hp", "growth.hp", "SWORD"),
        ("Ui_Ability_STR", "stat.str", "growth.str", "LANCE"),
        ("Ui_Ability_MAG", "stat.mag", "growth.mag", "AXE"),
        ("Ui_Ability_SKL", "stat.skl", "growth.skl", "BOW"),
        ("Ui_Ability_SPD", "stat.spd", "growth.spd", "FIRE"),
        ("Ui_Ability_LUK", "stat.luk", "growth.luk", "THUNDER"),
        ("Ui_Ability_DEF", "stat.def", "growth.def", "WIND"),
        ("Ui_Ability_MDF", "stat.mdf", "growth.mdf", "STAFF"),
        ("Ui_Ability_CON", "stat.con", null, null),
        ("Ui_Ability_MOV", "stat.mov", null, null),
    ];

    private readonly GameSnapshot _snapshot;
    private readonly SlotInfo _slot;
    private readonly GameSession _session;
    private readonly LocalizationService _localization;
    private readonly AssetService _assets;

    public AbilityTabViewModel(
        GameSnapshot snapshot,
        SlotInfo slot,
        GameSession session,
        LocalizationService localization,
        AssetService assets)
    {
        _snapshot = snapshot;
        _slot = slot;
        _session = session;
        _localization = localization;
        _assets = assets;

        var rows = new List<AbilityRowViewModel>(RowDefinitions.Length);
        foreach (var (labelKey, statId, growthId, weaponKey) in RowDefinitions)
        {
            var label = _localization.Ui(labelKey);
            var stat = Num(statId, label);
            var growth = growthId is null ? null : Num(growthId, label);
            NumericFieldViewModel? proficiency = null;
            Bitmap? weaponIcon = null;
            string? weaponLabel = null;

            if (weaponKey is not null)
            {
                proficiency = Num($"weapon.{weaponKey.ToLowerInvariant()}", label);
                weaponIcon = _assets.GetWeaponIcon(weaponKey);
                weaponLabel = _localization.Ui($"Ui_Weapon_{weaponKey}");
            }

            rows.Add(new AbilityRowViewModel(
                label,
                stat,
                growth,
                weaponIcon,
                weaponLabel,
                proficiency));
        }

        Rows = rows;
    }

    public IReadOnlyList<AbilityRowViewModel> Rows { get; }

    private NumericFieldViewModel Num(string id, string label)
    {
        var field = _session.Catalog.GetField(id);
        return new NumericFieldViewModel(
            _session,
            field,
            _slot.Index,
            label,
            _snapshot.Slots.ReadField(field, _slot.Index));
    }
}

public sealed class AbilityRowViewModel(
    string label,
    NumericFieldViewModel stat,
    NumericFieldViewModel? growth,
    Bitmap? weaponIcon,
    string? weaponLabel,
    NumericFieldViewModel? proficiency)
{
    public string Label { get; } = label;

    public NumericFieldViewModel Stat { get; } = stat;

    public NumericFieldViewModel? Growth { get; } = growth;

    public Bitmap? WeaponIcon { get; } = weaponIcon;

    public bool HasWeapon => WeaponIcon is not null;

    public string? WeaponLabel { get; } = weaponLabel;

    public NumericFieldViewModel? Proficiency { get; } = proficiency;
}
