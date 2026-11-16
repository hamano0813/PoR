using System.ComponentModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
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

    private static readonly WeaponCategory[] CategoryOrder =
    [
        WeaponCategory.Sword,
        WeaponCategory.Lance,
        WeaponCategory.Axe,
        WeaponCategory.Bow,
        WeaponCategory.Fire,
        WeaponCategory.Thunder,
        WeaponCategory.Wind,
        WeaponCategory.Staff,
    ];

    private readonly GameSnapshot _snapshot;
    private readonly SlotInfo _slot;
    private readonly GameSession _session;
    private readonly LocalizationService _localization;
    private readonly AssetService _assets;
    private readonly EnumFieldViewModel _classField;
    private readonly BoolFieldViewModel? _higherFeature;
    private readonly IReadOnlyDictionary<WeaponCategory, BoolFieldViewModel> _equipFeatures;
    private readonly Dictionary<WeaponCategory, NumericFieldViewModel> _proficiencies = [];
    private readonly HashSet<WeaponCategory> _revokedEquipFeatures = [];

    public AbilityTabViewModel(
        GameSnapshot snapshot,
        SlotInfo slot,
        GameSession session,
        LocalizationService localization,
        AssetService assets,
        EnumFieldViewModel classField,
        SkillTabViewModel skillTab)
    {
        _snapshot = snapshot;
        _slot = slot;
        _session = session;
        _localization = localization;
        _assets = assets;
        _classField = classField;

        var staffLightLabel =
            $"{_localization.Ui("Ui_Weapon_STAFF")}／{_localization.Ui("Ui_Weapon_LIGHT")}";
        var rows = new List<AbilityRowViewModel>(RowDefinitions.Length);
        foreach (var (labelKey, statId, growthId, weaponKey) in RowDefinitions)
        {
            var label = _localization.Ui(labelKey);
            var stat = Num(statId, label);
            stat.SetRange(0, 50);

            var growth = growthId is null ? null : Num(growthId, label);
            growth?.SetRange(0, 255);
            NumericFieldViewModel? proficiency = null;
            Bitmap? weaponIcon = null;
            string? weaponLabel = null;

            if (weaponKey is not null)
            {
                proficiency = Num($"weapon.{weaponKey.ToLowerInvariant()}", label);
                _proficiencies[Category(weaponKey)] = proficiency;
                weaponIcon = _assets.GetWeaponIcon(weaponKey);
                weaponLabel = _localization.Ui($"Ui_Weapon_{weaponKey}");
            }

            rows.Add(new AbilityRowViewModel(
                label,
                stat,
                growth,
                weaponIcon,
                weaponLabel,
                proficiency,
                weaponKey == "STAFF" ? _classField : null,
                weaponKey == "STAFF" ? staffLightLabel : null));
        }

        Rows = rows;

        _higherFeature = skillTab.FindFeature("SID_HIGHER");
        var equipFeatures = new Dictionary<WeaponCategory, BoolFieldViewModel>();
        AddFeature(equipFeatures, skillTab, WeaponCategory.Sword, "SID_EQSW");
        AddFeature(equipFeatures, skillTab, WeaponCategory.Lance, "SID_EQLA");
        AddFeature(equipFeatures, skillTab, WeaponCategory.Axe, "SID_EQAX");
        AddFeature(equipFeatures, skillTab, WeaponCategory.Bow, "SID_EQBW");
        AddFeature(equipFeatures, skillTab, WeaponCategory.Staff, "SID_EQRD");
        _equipFeatures = equipFeatures;

        ApplyProficiencyRules();
        _classField.PropertyChanged += OnClassChanged;
        _higherFeature?.PropertyChanged += OnFeatureChanged;
        foreach (var (category, feature) in _equipFeatures)
        {
            feature.PropertyChanged += (_, e) =>
                OnEquipFeatureChanged(category, feature, e);
        }

        foreach (var field in _proficiencies.Values)
        {
            field.PropertyChanged += OnProficiencyChanged;
        }
    }

    public IReadOnlyList<AbilityRowViewModel> Rows { get; }

    public IReadOnlyDictionary<WeaponCategory, NumericFieldViewModel> Proficiencies
        => _proficiencies;

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

    private static WeaponCategory Category(string weaponKey)
        => weaponKey switch
        {
            "SWORD" => WeaponCategory.Sword,
            "LANCE" => WeaponCategory.Lance,
            "AXE" => WeaponCategory.Axe,
            "BOW" => WeaponCategory.Bow,
            "FIRE" => WeaponCategory.Fire,
            "THUNDER" => WeaponCategory.Thunder,
            "WIND" => WeaponCategory.Wind,
            "STAFF" => WeaponCategory.Staff,
            _ => throw new ArgumentOutOfRangeException(
                nameof(weaponKey),
                weaponKey,
                "Unknown weapon category."),
        };

    private static void AddFeature(
        IDictionary<WeaponCategory, BoolFieldViewModel> features,
        SkillTabViewModel skillTab,
        WeaponCategory category,
        string fieldId)
    {
        if (skillTab.FindFeature(fieldId) is { } feature)
        {
            features[category] = feature;
        }
    }

    private void ApplyProficiencyRules()
    {
        var classKey = _classField.SelectedItem?.Key;
        var locked = ClassProficiencyCatalog.IsProficiencyLocked(classKey);
        ClassProficiencyCatalog.TryGet(classKey, out var classCategories);
        var higher = _higherFeature?.IsChecked == true;

        var editable = new Dictionary<
            WeaponCategory,
            (NumericFieldViewModel Field, bool IsEditable, int BaseUpper)>();
        foreach (var category in CategoryOrder)
        {
            if (!_proficiencies.TryGetValue(category, out var field))
            {
                continue;
            }

            var featureUnlocked =
                _equipFeatures.TryGetValue(category, out var feature) &&
                feature.IsChecked;
            var isEditable = !locked &&
                             !_revokedEquipFeatures.Contains(category) &&
                             (classCategories.Contains(category) || featureUnlocked);
            var baseUpper = higher || featureUnlocked
                ? ClassProficiencyCatalog.RankS
                : ClassProficiencyCatalog.RankB;

            editable[category] = (field, isEditable, baseUpper);
        }

        WeaponCategory? sCategory = null;
        foreach (var category in CategoryOrder)
        {
            if (!editable.TryGetValue(category, out var info) ||
                !info.IsEditable ||
                info.BaseUpper < ClassProficiencyCatalog.RankS ||
                info.Field.Value < ClassProficiencyCatalog.RankS)
            {
                continue;
            }

            sCategory = category;
            break;
        }

        foreach (var category in CategoryOrder)
        {
            if (!editable.TryGetValue(category, out var info))
            {
                continue;
            }

            if (!info.IsEditable)
            {
                info.Field.IsValueEnabled = false;
                continue;
            }

            var upper = info.BaseUpper;
            if (sCategory is not null && category != sCategory.Value)
            {
                upper = Math.Min(upper, ClassProficiencyCatalog.RankS - 1);
            }

            info.Field.IsValueEnabled = true;
            info.Field.SetRange(ClassProficiencyCatalog.RankE, upper);
        }
    }

    private void OnClassChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EnumFieldViewModel.SelectedItem))
        {
            ApplyProficiencyRules();
        }
    }

    private void OnFeatureChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BoolFieldViewModel.IsChecked))
        {
            ApplyProficiencyRules();
        }
    }

    private void OnEquipFeatureChanged(
        WeaponCategory category,
        BoolFieldViewModel feature,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(BoolFieldViewModel.IsChecked) ||
            !_proficiencies.TryGetValue(category, out var field))
        {
            return;
        }

        var classKey = _classField.SelectedItem?.Key;
        if (ClassProficiencyCatalog.IsProficiencyLocked(classKey))
        {
            ApplyProficiencyRules();
            return;
        }

        if (feature.IsChecked)
        {
            _revokedEquipFeatures.Remove(category);
            field.SetValueDirect(1);
        }
        else
        {
            // The feature is the access switch for this category, so revoke
            // class access too until it is checked again.
            _revokedEquipFeatures.Add(category);
            field.SetValueDirect(0);
        }

        ApplyProficiencyRules();
    }

    private void OnProficiencyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NumericFieldViewModel.Value))
        {
            ApplyProficiencyRules();
        }
    }
}

public sealed class AbilityRowViewModel : ObservableObject
{
    private readonly string? _weaponLabel;
    private readonly EnumFieldViewModel? _staffClassField;
    private readonly string? _staffLightLabel;

    public AbilityRowViewModel(
        string label,
        NumericFieldViewModel stat,
        NumericFieldViewModel? growth,
        Bitmap? weaponIcon,
        string? weaponLabel,
        NumericFieldViewModel? proficiency,
        EnumFieldViewModel? staffClassField,
        string? staffLightLabel)
    {
        Label = label;
        Stat = stat;
        Growth = growth;
        WeaponIcon = weaponIcon;
        _weaponLabel = weaponLabel;
        _staffClassField = staffClassField;
        _staffLightLabel = staffLightLabel;
        Proficiency = proficiency;

        if (proficiency is not null)
        {
            proficiency.PropertyChanged += OnProficiencyChanged;
        }

        if (staffClassField is not null)
        {
            staffClassField.PropertyChanged += OnClassChanged;
        }
    }

    public string Label { get; }

    public NumericFieldViewModel Stat { get; }

    public NumericFieldViewModel? Growth { get; }

    public bool HasGrowth => Growth is not null;

    public Bitmap? WeaponIcon { get; }

    public bool HasWeapon => WeaponIcon is not null;

    public string? WeaponLabel => _weaponLabel;

    public string ProficiencyLabel
    {
        get
        {
            if (Proficiency is null || Proficiency.Value <= 0)
            {
                return string.Empty;
            }

            return $"{GetWeaponLabel()} {GetWeaponRank(Proficiency.Value)}";
        }
    }

    public NumericFieldViewModel? Proficiency { get; }

    private string GetWeaponLabel()
    {
        if (_staffClassField?.SelectedItem?.Key is "JID_BISHOP" or "JID_BISHOP/F")
        {
            return _staffLightLabel ?? _weaponLabel ?? string.Empty;
        }

        return _weaponLabel ?? string.Empty;
    }

    private static string GetWeaponRank(decimal value)
    {
        if (value >= ClassProficiencyCatalog.RankS)
        {
            return "Ｓ";
        }

        if (value >= ClassProficiencyCatalog.RankA)
        {
            return "Ａ";
        }

        if (value >= ClassProficiencyCatalog.RankB)
        {
            return "Ｂ";
        }

        if (value >= ClassProficiencyCatalog.RankC)
        {
            return "Ｃ";
        }

        if (value >= ClassProficiencyCatalog.RankD)
        {
            return "Ｄ";
        }

        return "Ｅ";
    }

    private void OnProficiencyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NumericFieldViewModel.Value))
        {
            OnPropertyChanged(nameof(ProficiencyLabel));
        }
    }

    private void OnClassChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EnumFieldViewModel.SelectedItem))
        {
            OnPropertyChanged(nameof(ProficiencyLabel));
        }
    }
}
