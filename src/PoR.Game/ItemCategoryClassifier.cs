namespace PoR.Game;

/// <summary>
/// Internal Path of Radiance item categories. These names are only used to
/// decide which equipment slot may hold an item; they are not shown in the UI.
/// </summary>
public enum ItemCategory
{
    HumanWeapon, // A
    LaguzWeapon, // B
    Staff,       // C
    Accessory,   // D
    Item,        // E
}

/// <summary>
/// Classifies item IDs for the four weapon slots and four item slots.
/// Weapon slots accept categories A-C; item slots accept D-E.
/// </summary>
public static class ItemCategoryClassifier
{
    private static readonly HashSet<string> HumanWeaponKeys = new(StringComparer.Ordinal)
    {
        // Swords
        "IID_IRONSWORD",
        "IID_PRACTICESWORD",
        "IID_SLIMSWORD",
        "IID_STEELSWORD",
        "IID_SILVERSWORD",
        "IID_IRONBLADE",
        "IID_SILVERBLADE",
        "IID_POISONSWORD",
        "IID_LONGSWORD",
        "IID_BRAVESWORD",
        "IID_WATOU",
        "IID_KILLSWORD",
        "IID_ARMORKILLER",
        "IID_RANCEBASTER",
        "IID_HORSEKILLER",
        "IID_RUNESWORD",
        "IID_ALONDITE",
        "IID_RAGNELL",
        "IID_GURGURANT",
        "IID_CRIMEASWORD",
        "IID_STEELBLADE",
        "IID_SONICBLADE",

        // Lances
        "IID_IRONLANCE",
        "IID_SLIMLANCE",
        "IID_STEELLANCE",
        "IID_SILVERLANCE",
        "IID_POISONLANCE",
        "IID_BRAVELANCE",
        "IID_KILLERLANCE",
        "IID_KNIGHTKILLER",
        "IID_HANDSPEAR",
        "IID_SLENDERSPEAR",
        "IID_FRAMELANCE",
        "IID_DRAGONRANCE",
        "IID_AXEBASTER",
        "IID_ZANEZPHTE",
        "IID_ARMORBREAK",

        // Axes
        "IID_IRONAXE",
        "IID_PRACTICEAXE",
        "IID_STEELAXE",
        "IID_SILVERAXE",
        "IID_POISONAXE",
        "IID_BRAVEAXE",
        "IID_KILLERAXE",
        "IID_POLEAXE",
        "IID_HAMMER",
        "IID_DEVILAXE",
        "IID_HANDAXE",
        "IID_TOMAHAWK",
        "IID_SONICAXE",
        "IID_SWORDBASTER",
        "IID_BEASTAXE",
        "IID_URVAN",

        // Bows and ballistae
        "IID_IRONBOW",
        "IID_STEELBOW",
        "IID_SILVERBOW",
        "IID_POISONBOW",
        "IID_KILLERBOW",
        "IID_BRAVEBOW",
        "IID_SHORTBOW",
        "IID_LONGBOW",
        "IID_LOFABOW",
        "IID_LONGLONGBOW",
        "IID_LIGHTNINGBOW",
        "IID_LONGARCH",
        "IID_IRONARCH",
        "IID_KILLERARCH",
        "IID_ONAGER",

        // Daggers
        "IID_KNIFE",
        "IID_DAGGER",
        "IID_STILETTO",

        // Fire magic
        "IID_FIRE",
        "IID_ELFIRE",
        "IID_BOLGANONE",
        "IID_FORBLAZE",
        "IID_METEOR",

        // Wind magic
        "IID_WIND",
        "IID_FLUTTER",
        "IID_ELWIND",
        "IID_TORNADO",
        "IID_FIMBULVETR",
        "IID_BLIZZARD",

        // Thunder magic
        "IID_THUNDER",
        "IID_ELTHUNDER",
        "IID_THORON",
        "IID_MJOLNIR",
        "IID_THUNDERSTORM",

        // Light magic
        "IID_LIGHTNING",
        "IID_SHINE",
        "IID_RESIRE",
        "IID_AURA",
        "IID_PURGE",
    };

    private static readonly HashSet<string> LaguzWeaponKeys = new(StringComparer.Ordinal)
    {
        "IID_LIONNAIL",
        "IID_TIGERNAIL",
        "IID_CATNAIL",
        "IID_WHITEBREATH",
        "IID_REDBREATH",
        "IID_HAWKBEAK",
        "IID_CROWBEAK",
        "IID_HAWKBEAK_KING",
        "IID_CROWBEAK_KING",
    };

    private static readonly HashSet<string> StaffKeys = new(StringComparer.Ordinal)
    {
        "IID_GODDESSROD",
        "IID_LIVE",
        "IID_RELIVE",
        "IID_RECOVER",
        "IID_REBLOW",
        "IID_RESERVE",
        "IID_REST",
        "IID_SILENCE",
        "IID_SLEEP",
        "IID_BERSERK",
        "IID_WARP",
        "IID_RESCUE",
        "IID_TORCH",
        "IID_HAMMERNE",
        "IID_UNLOCK",
        "IID_MSHIELD",
    };

    private static readonly HashSet<string> AccessoryKeys = new(StringComparer.Ordinal)
    {
        "IID_KNIGHTRING",
        "IID_VSBEAST",
        "IID_VSHUMAN",
        "IID_DELPHISHIELD",
        "IID_SWORDERSRING",
        "IID_SOLDIERRING",
        "IID_FIGHTERSRING",
        "IID_ARCHERSRING",
        "IID_ARMORSRING",
        "IID_SOCIALKNIGHTSRING",
        "IID_PEGASUSKNIGHTSRING",
        "IID_DRAGONKNIGHTSRING",
        "IID_MAGESRING",
        "IID_PRIESTSRING",
        "IID_THIEFSRING",
        "IID_KNIGHTSHIELD",
        "IID_CHANGERING",
        "IID_CHANGERING2",
    };

    public static ItemCategory Classify(string key)
    {
        if (HumanWeaponKeys.Contains(key))
        {
            return ItemCategory.HumanWeapon;
        }

        if (LaguzWeaponKeys.Contains(key))
        {
            return ItemCategory.LaguzWeapon;
        }

        if (StaffKeys.Contains(key))
        {
            return ItemCategory.Staff;
        }

        if (AccessoryKeys.Contains(key))
        {
            return ItemCategory.Accessory;
        }

        return ItemCategory.Item;
    }

    public static bool IsWeaponSlotItem(string key)
    {
        var category = Classify(key);
        return category is ItemCategory.HumanWeapon
            or ItemCategory.LaguzWeapon
            or ItemCategory.Staff;
    }

    /// <summary>
    /// Internal items that exist in the game data but must not appear in any
    /// equipment drop-down.
    /// </summary>
    public static bool IsHidden(string key)
        => string.Equals(key, "IID_FLUTTER", StringComparison.Ordinal);
}
