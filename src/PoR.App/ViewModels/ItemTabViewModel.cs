using System.ComponentModel;
using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels;

public sealed class ItemTabViewModel
{
    private readonly GameSnapshot _snapshot;
    private readonly SlotInfo _slot;
    private readonly GameSession _session;
    private readonly LocalizationService _localization;
    private readonly AssetService _assets;
    private readonly AbilityTabViewModel _ability;
    private readonly BoolFieldViewModel? _knifeEquipFeature;
    private readonly BoolFieldViewModel? _specialBowFeature;
    private readonly BoolFieldViewModel? _amitiFeature;
    private readonly BoolFieldViewModel? _animalizeFeature;
    private readonly BoolFieldViewModel? _eqCFeature;
    private readonly BoolFieldViewModel? _eqDFeature;
    private readonly WeaponSlotProfile _weaponProfile;
    private bool _suppressEquipRules;

    private static readonly HashSet<string> KnifeKeys = new(StringComparer.Ordinal)
    {
        "IID_KNIFE",
        "IID_DAGGER",
        "IID_STILETTO",
    };

    private enum WeaponSlotProfile
    {
        Human,
        Laguz,
        Heron,
    }

    public ItemTabViewModel(
        GameSnapshot snapshot,
        SlotInfo slot,
        GameSession session,
        LocalizationService localization,
        AssetService assets,
        AbilityTabViewModel ability,
        SkillTabViewModel skillTab)
    {
        _snapshot = snapshot;
        _slot = slot;
        _session = session;
        _localization = localization;
        _assets = assets;
        _ability = ability;
        _knifeEquipFeature = skillTab.FindField("SID_EQUIPKNIFE");
        _specialBowFeature = skillTab.FindField("SID_EQ_A");
        _amitiFeature = skillTab.FindField("SID_EQ_B");
        _animalizeFeature = skillTab.FindField("SID_ANIMALIZE");
        _eqCFeature = skillTab.FindField("SID_EQ_C");
        _eqDFeature = skillTab.FindField("SID_EQ_D");

        _weaponProfile = CharacterClassClassifier.IsHeron(slot.JidKey)
            ? WeaponSlotProfile.Heron
            : CharacterClassClassifier.IsLaguz(slot.JidKey)
                ? WeaponSlotProfile.Laguz
                : WeaponSlotProfile.Human;

        IReadOnlyList<MappingItem> allItems = _snapshot.Mappings.TryGetValue("iid", out var items)
            ? items.Where(item => item.Value != 0).ToList()
            : [];
        var itemLabel = _localization.Ui("Ui_Item_Item");
        var equippedLabel = _localization.Ui("Ui_Item_Equipped");
        var droppedLabel = _localization.Ui("Ui_Item_Dropped");
        var rows = new List<ItemRowViewModel>(8);

        for (var index = 1; index <= 8; index++)
        {
            var currentValue = Value($"item{index}");
            var weaponSlot = index <= 4;
            var item = new EnumFieldViewModel(
                session,
                Field($"item{index}"),
                slot.Index,
                itemLabel,
                BuildItemMapping(
                    allItems,
                    weaponSlot
                        ? key => IsAllowedWeaponItem(index, key)
                        : IsItemSlotItem,
                    currentValue),
                _localization,
                _assets,
                currentValue);

            var durability = new NumericFieldViewModel(
                session,
                Field($"durability{index}"),
                slot.Index,
                _localization.Ui("Ui_Item_Durability"),
                Value($"durability{index}"));

            var equipped = new BoolFieldViewModel(
                session,
                Field($"equipped{index}"),
                slot.Index,
                equippedLabel,
                Value($"equipped{index}"));

            var dropped = new BoolFieldViewModel(
                session,
                Field($"dropped{index}"),
                slot.Index,
                droppedLabel,
                Value($"dropped{index}"));

            rows.Add(new ItemRowViewModel(index, item, durability, equipped, dropped));
        }

        Rows = rows;
        WeaponRows = rows.Take(4).ToList();
        ItemRows = rows.Skip(4).ToList();

        ApplyAllRules(resetDurability: false);
        foreach (var row in Rows)
        {
            row.Item.PropertyChanged += OnItemChanged;
        }

        foreach (var row in WeaponRows)
        {
            row.Equipped.PropertyChanged += OnWeaponEquippedChanged;
        }

        foreach (var row in ItemRows)
        {
            row.Equipped.PropertyChanged += OnItemEquippedChanged;
        }

        foreach (var field in _ability.Proficiencies.Values)
        {
            field.PropertyChanged += OnProficiencyChanged;
        }

        foreach (var field in skillTab.ActiveSkills.Concat(skillTab.Features))
        {
            field.PropertyChanged += OnSkillFieldChanged;
        }
    }

    public IReadOnlyList<ItemRowViewModel> Rows { get; }

    public IReadOnlyList<ItemRowViewModel> WeaponRows { get; }

    public IReadOnlyList<ItemRowViewModel> ItemRows { get; }

    private FieldDefinition Field(string id) => _session.Catalog.GetField(id);

    private long Value(string id)
        => _snapshot.Slots.ReadField(Field(id), _slot.Index);

    private bool IsAllowedWeaponItem(int index, string key)
    {
        var category = ItemCategoryClassifier.Classify(key);

        if (_weaponProfile == WeaponSlotProfile.Laguz)
        {
            // The first slot is the species-specific laguz strike; the other
            // three slots are ordinary non-laguz weapon slots.
            if (index == 1)
            {
                var speciesWeapon = LaguzWeaponCatalog.GetWeaponKey(_slot.JidKey);
                return category == ItemCategory.LaguzWeapon &&
                       (speciesWeapon is null || key == speciesWeapon);
            }

            return category is ItemCategory.HumanWeapon or ItemCategory.Staff;
        }

        return category is ItemCategory.HumanWeapon or ItemCategory.Staff;
    }

    private static bool IsItemSlotItem(string key)
        => !ItemCategoryClassifier.IsWeaponSlotItem(key);

    private static IReadOnlyList<MappingItem> BuildItemMapping(
        IReadOnlyList<MappingItem> allItems,
        Func<string, bool> isAllowed,
        long currentValue)
    {
        var mapping = new List<MappingItem>
        {
            new(0, "IID_NONE", "——"),
        };

        var allowed = allItems
            .Where(item => !ItemCategoryClassifier.IsHidden(item.Key) && isAllowed(item.Key))
            .ToList();

        var current = allItems.FirstOrDefault(item => item.Value == (uint)currentValue);
        if (current is not null &&
            !ItemCategoryClassifier.IsHidden(current.Key) &&
            !isAllowed(current.Key))
        {
            // Keep an existing out-of-category value visible; the list is
            // still restricted to the correct slot type for new selections.
            allowed.Insert(0, current);
        }

        mapping.AddRange(allowed);
        return mapping;
    }

    private void ApplyAllRules(bool resetDurability)
    {
        _suppressEquipRules = true;
        try
        {
            foreach (var row in Rows)
            {
                ApplyRowRules(row, resetDurability);
            }

            NormalizeWeaponEquip();
            NormalizeItemEquip();
        }
        finally
        {
            _suppressEquipRules = false;
        }
    }

    private void ApplyRowRules(ItemRowViewModel row, bool resetDurability)
    {
        if (row.Item.SelectedItem is null || row.Item.SelectedItem.Value == 0)
        {
            ApplyEmptyRowRules(row);
            return;
        }

        if (row.Index <= 4)
        {
            ApplyWeaponRowRules(row, resetDurability);
        }
        else
        {
            ApplyItemRowRules(row, resetDurability);
        }
    }

    private void ApplyEmptyRowRules(ItemRowViewModel row)
    {
        // Force an empty slot to zero uses and lock all related editors.
        row.Durability.Value = 0;
        row.Durability.SetRange(0, 0);
        row.Durability.IsValueEnabled = false;

        row.Equipped.IsToggleEnabled = false;
        if (row.Equipped.IsChecked)
        {
            row.Equipped.IsChecked = false;
        }

        row.Dropped.IsToggleEnabled = false;
        if (row.Dropped.IsChecked)
        {
            row.Dropped.IsChecked = false;
        }
    }

    private void ApplyWeaponRowRules(ItemRowViewModel row, bool resetDurability)
    {
        var forced = IsForcedEquip(row);
        var canEquip = CanEquip(row);

        row.Equipped.IsToggleEnabled = canEquip && !forced;
        if (forced)
        {
            row.Equipped.IsChecked = true;
        }
        else if (!canEquip && row.Equipped.IsChecked)
        {
            row.Equipped.IsChecked = false;
        }

        var dropLocked = _weaponProfile == WeaponSlotProfile.Laguz && row.Index == 1;
        row.Dropped.IsToggleEnabled = !dropLocked;
        if (dropLocked && row.Dropped.IsChecked)
        {
            row.Dropped.IsChecked = false;
        }

        ApplyWeaponDurabilityRule(row, resetDurability);
    }

    private void ApplyItemRowRules(ItemRowViewModel row, bool resetDurability)
    {
        var key = row.Item.SelectedItem?.Key;
        if (ItemDurabilityCatalog.TryGet(key, out var durability))
        {
            row.Durability.SetRange(durability.Minimum, durability.Uses);
            row.Durability.IsValueEnabled = durability.Editable;

            if (resetDurability)
            {
                row.Durability.Value = durability.Uses;
            }
            else if (row.Durability.Value > durability.Uses)
            {
                row.Durability.Load(durability.Uses);
            }
        }
        else
        {
            row.Durability.SetRange(0, 255);
            row.Durability.IsValueEnabled = true;
        }

        ItemCategory? category = key is null
            ? null
            : ItemCategoryClassifier.Classify(key);
        var canEquip = category == ItemCategory.Accessory &&
                       IsAccessoryFeatureAllowed(key);
        row.Equipped.IsToggleEnabled = canEquip;
        if (!canEquip && row.Equipped.IsChecked)
        {
            row.Equipped.IsChecked = false;
        }

        row.Dropped.IsToggleEnabled = true;
    }

    private bool IsAccessoryFeatureAllowed(string? key)
        => key switch
        {
            "IID_VSBEAST" => _animalizeFeature?.IsChecked == true,
            "IID_VSHUMAN" => _animalizeFeature?.IsChecked != true,
            "IID_KNIGHTSHIELD" => _eqDFeature?.IsChecked == true,
            "IID_CHANGERING" => _animalizeFeature?.IsChecked == true,
            "IID_CHANGERING2" => _eqCFeature?.IsChecked == true,
            _ => true,
        };

    private void OnWeaponEquippedChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressEquipRules ||
            e.PropertyName != nameof(BoolFieldViewModel.IsChecked) ||
            sender is not BoolFieldViewModel field ||
            !field.IsChecked)
        {
            return;
        }

        _suppressEquipRules = true;
        try
        {
            foreach (var row in WeaponRows)
            {
                if (!ReferenceEquals(row.Equipped, field) && row.Equipped.IsChecked)
                {
                    row.Equipped.IsChecked = false;
                }
            }
        }
        finally
        {
            _suppressEquipRules = false;
        }
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(EnumFieldViewModel.SelectedItem))
        {
            return;
        }

        var row = Rows.FirstOrDefault(itemRow => ReferenceEquals(itemRow.Item, sender));
        if (row is null)
        {
            return;
        }

        _suppressEquipRules = true;
        try
        {
            ApplyRowRules(row, resetDurability: true);
            NormalizeWeaponEquip();
            NormalizeItemEquip();
        }
        finally
        {
            _suppressEquipRules = false;
        }
    }

    private void OnItemEquippedChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressEquipRules ||
            e.PropertyName != nameof(BoolFieldViewModel.IsChecked) ||
            sender is not BoolFieldViewModel field ||
            !field.IsChecked)
        {
            return;
        }

        _suppressEquipRules = true;
        try
        {
            foreach (var row in ItemRows)
            {
                if (!ReferenceEquals(row.Equipped, field) && row.Equipped.IsChecked)
                {
                    row.Equipped.IsChecked = false;
                }
            }
        }
        finally
        {
            _suppressEquipRules = false;
        }
    }

    private void NormalizeItemEquip()
    {
        var checkedRows = ItemRows.Where(row => row.Equipped.IsChecked).ToList();
        foreach (var row in checkedRows.Skip(1))
        {
            row.Equipped.IsChecked = false;
        }
    }

    private void NormalizeWeaponEquip()
    {
        if (_weaponProfile == WeaponSlotProfile.Laguz)
        {
            foreach (var row in WeaponRows)
            {
                var shouldBeChecked = IsForcedEquip(row);
                if (row.Equipped.IsChecked != shouldBeChecked)
                {
                    row.Equipped.IsChecked = shouldBeChecked;
                }
            }

            return;
        }

        var checkedRows = WeaponRows.Where(row => row.Equipped.IsChecked).ToList();
        foreach (var row in checkedRows.Skip(1))
        {
            row.Equipped.IsChecked = false;
        }
    }

    private bool IsForcedEquip(ItemRowViewModel row)
        => _weaponProfile == WeaponSlotProfile.Laguz &&
           row.Index == 1 &&
           IsMatchingLaguzWeapon(row.Item.SelectedItem?.Key);

    private bool CanEquip(ItemRowViewModel row)
    {
        if (_weaponProfile == WeaponSlotProfile.Heron)
        {
            return false;
        }

        var key = row.Item.SelectedItem?.Key;
        if (key is null ||
            !PersonalWeaponCatalog.IsOwnerAllowed(key, _slot.PidKey) ||
            !MeetsSpecialEquipFeature(key) ||
            !MeetsRequiredRank(key))
        {
            return false;
        }

        if (_weaponProfile == WeaponSlotProfile.Laguz)
        {
            return row.Index == 1 && IsMatchingLaguzWeapon(key);
        }

        return ItemCategoryClassifier.Classify(key) == ItemCategory.HumanWeapon;
    }

    private bool MeetsSpecialEquipFeature(string key)
    {
        if (KnifeKeys.Contains(key))
        {
            return _knifeEquipFeature?.IsChecked == true;
        }

        if (string.Equals(key, "IID_LOFABOW", StringComparison.Ordinal))
        {
            return _specialBowFeature?.IsChecked == true;
        }

        if (string.Equals(key, "IID_CRIMEASWORD", StringComparison.Ordinal))
        {
            return _amitiFeature?.IsChecked == true;
        }

        return true;
    }

    private bool MeetsRequiredRank(string key)
    {
        if (!WeaponCatalog.TryGet(key, out var stats) ||
            stats.Category is not { } category)
        {
            return true;
        }

        var requirement = WeaponCatalog.GetRankRequirement(stats.Rank);
        if (requirement is null ||
            !_ability.Proficiencies.TryGetValue(category, out var proficiency))
        {
            return true;
        }

        return proficiency.Value >= requirement;
    }

    private bool IsMatchingLaguzWeapon(string? key)
    {
        if (!IsLaguzWeaponKey(key))
        {
            return false;
        }

        var expected = LaguzWeaponCatalog.GetWeaponKey(_slot.JidKey);
        return expected is null ||
               string.Equals(key, expected, StringComparison.Ordinal);
    }

    private static bool IsLaguzWeaponKey(string? key)
        => key is not null &&
           ItemCategoryClassifier.Classify(key) == ItemCategory.LaguzWeapon;

    private void OnProficiencyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NumericFieldViewModel.Value))
        {
            RefreshWeaponEquipRules();
        }
    }

    private void OnSkillFieldChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BoolFieldViewModel.IsChecked))
        {
            RefreshAllEquipRules();
        }
    }

    private void RefreshAllEquipRules()
    {
        _suppressEquipRules = true;
        try
        {
            foreach (var row in WeaponRows)
            {
                ApplyRowRules(row, resetDurability: false);
            }

            NormalizeWeaponEquip();

            foreach (var row in ItemRows)
            {
                ApplyRowRules(row, resetDurability: false);
            }

            NormalizeItemEquip();
        }
        finally
        {
            _suppressEquipRules = false;
        }
    }

    private void RefreshWeaponEquipRules()
    {
        _suppressEquipRules = true;
        try
        {
            foreach (var row in WeaponRows)
            {
                ApplyRowRules(row, resetDurability: false);
            }

            NormalizeWeaponEquip();
        }
        finally
        {
            _suppressEquipRules = false;
        }
    }

    private static void ApplyWeaponDurabilityRule(
        ItemRowViewModel row,
        bool resetDurability)
    {
        if (!WeaponCatalog.TryGet(row.Item.SelectedItem?.Key, out var stats))
        {
            row.Durability.SetRange(0, 255);
            row.Durability.IsValueEnabled = true;
            return;
        }

        row.Durability.SetRange(1, stats.Uses);
        row.Durability.IsValueEnabled = !stats.InfiniteDurability;

        if (resetDurability)
        {
            row.Durability.Value = stats.Uses;
        }
        else if (row.Durability.Value > stats.Uses)
        {
            // Do not overwrite the stored value on the first read; just keep
            // the editor display within the weapon's valid range.
            row.Durability.Load(stats.Uses);
        }
    }
}

public sealed class ItemRowViewModel(
    int index,
    EnumFieldViewModel item,
    NumericFieldViewModel durability,
    BoolFieldViewModel equipped,
    BoolFieldViewModel dropped)
{
    public int Index { get; } = index;

    public EnumFieldViewModel Item { get; } = item;

    public NumericFieldViewModel Durability { get; } = durability;

    public BoolFieldViewModel Equipped { get; } = equipped;

    public BoolFieldViewModel Dropped { get; } = dropped;
}
