namespace PoR.Game;

/// <summary>
/// Weapon metadata used by the editor. Rank is kept for future weapon-level
/// rules; Uses is the durability capacity and the value restored when the
/// weapon is selected. InfiniteDurability weapons cannot have their durability
/// edited.
/// </summary>
public sealed record WeaponStats(
    int Uses,
    bool InfiniteDurability,
    string Rank,
    WeaponCategory? Category = null);

public static class WeaponCatalog
{
    private static readonly Dictionary<string, WeaponStats> Stats = new(StringComparer.Ordinal)
    {
        // Swords
        ["IID_IRONSWORD"] = new(46, false, "E", WeaponCategory.Sword),
        ["IID_PRACTICESWORD"] = new(46, true, "E", WeaponCategory.Sword),
        ["IID_SLIMSWORD"] = new(35, false, "E", WeaponCategory.Sword),
        ["IID_STEELSWORD"] = new(35, false, "D", WeaponCategory.Sword),
        ["IID_SILVERSWORD"] = new(25, false, "A", WeaponCategory.Sword),
        ["IID_IRONBLADE"] = new(35, false, "D", WeaponCategory.Sword),
        ["IID_SILVERBLADE"] = new(15, false, "A", WeaponCategory.Sword),
        ["IID_POISONSWORD"] = new(40, false, "D", WeaponCategory.Sword),
        ["IID_LONGSWORD"] = new(40, true, "★", WeaponCategory.Sword),
        ["IID_BRAVESWORD"] = new(30, false, "B", WeaponCategory.Sword),
        ["IID_WATOU"] = new(25, false, "S", WeaponCategory.Sword),
        ["IID_KILLSWORD"] = new(20, false, "C", WeaponCategory.Sword),
        ["IID_ARMORKILLER"] = new(18, false, "D", WeaponCategory.Sword),
        ["IID_RANCEBASTER"] = new(15, false, "C", WeaponCategory.Sword),
        ["IID_HORSEKILLER"] = new(18, false, "D", WeaponCategory.Sword),
        ["IID_RUNESWORD"] = new(15, false, "A", WeaponCategory.Sword),
        ["IID_ALONDITE"] = new(20, true, "★", WeaponCategory.Sword),
        ["IID_RAGNELL"] = new(20, true, "★", WeaponCategory.Sword),
        ["IID_GURGURANT"] = new(20, true, "★", WeaponCategory.Sword),
        ["IID_CRIMEASWORD"] = new(20, true, "★", WeaponCategory.Sword),
        ["IID_STEELBLADE"] = new(35, false, "C", WeaponCategory.Sword),
        ["IID_SONICBLADE"] = new(40, false, "B", WeaponCategory.Sword),

        // Daggers
        ["IID_KNIFE"] = new(30, false, "★", WeaponCategory.Sword),
        ["IID_DAGGER"] = new(30, false, "★", WeaponCategory.Sword),
        ["IID_STILETTO"] = new(20, false, "★", WeaponCategory.Sword),

        // Lances
        ["IID_IRONLANCE"] = new(45, false, "E", WeaponCategory.Lance),
        ["IID_SLIMLANCE"] = new(35, false, "E", WeaponCategory.Lance),
        ["IID_STEELLANCE"] = new(35, false, "D", WeaponCategory.Lance),
        ["IID_SILVERLANCE"] = new(25, false, "A", WeaponCategory.Lance),
        ["IID_POISONLANCE"] = new(40, false, "D", WeaponCategory.Lance),
        ["IID_BRAVELANCE"] = new(30, false, "B", WeaponCategory.Lance),
        ["IID_KILLERLANCE"] = new(20, false, "C", WeaponCategory.Lance),
        ["IID_KNIGHTKILLER"] = new(18, false, "D", WeaponCategory.Lance),
        ["IID_HANDSPEAR"] = new(25, false, "E", WeaponCategory.Lance),
        ["IID_SLENDERSPEAR"] = new(15, false, "B", WeaponCategory.Lance),
        ["IID_FRAMELANCE"] = new(25, false, "B", WeaponCategory.Lance),
        ["IID_DRAGONRANCE"] = new(15, false, "C", WeaponCategory.Lance),
        ["IID_AXEBASTER"] = new(15, false, "C", WeaponCategory.Lance),
        ["IID_ZANEZPHTE"] = new(20, false, "S", WeaponCategory.Lance),
        ["IID_ARMORBREAK"] = new(18, false, "D", WeaponCategory.Lance),

        // Axes
        ["IID_IRONAXE"] = new(45, false, "E", WeaponCategory.Axe),
        ["IID_PRACTICEAXE"] = new(45, true, "E", WeaponCategory.Axe),
        ["IID_STEELAXE"] = new(35, false, "E", WeaponCategory.Axe),
        ["IID_SILVERAXE"] = new(25, false, "A", WeaponCategory.Axe),
        ["IID_POISONAXE"] = new(40, false, "D", WeaponCategory.Axe),
        ["IID_BRAVEAXE"] = new(30, false, "B", WeaponCategory.Axe),
        ["IID_KILLERAXE"] = new(20, false, "C", WeaponCategory.Axe),
        ["IID_POLEAXE"] = new(18, false, "D", WeaponCategory.Axe),
        ["IID_HAMMER"] = new(20, false, "D", WeaponCategory.Axe),
        ["IID_DEVILAXE"] = new(20, false, "E", WeaponCategory.Axe),
        ["IID_HANDAXE"] = new(25, false, "E", WeaponCategory.Axe),
        ["IID_TOMAHAWK"] = new(15, false, "B", WeaponCategory.Axe),
        ["IID_SONICAXE"] = new(25, false, "B", WeaponCategory.Axe),
        ["IID_SWORDBASTER"] = new(15, false, "C", WeaponCategory.Axe),
        ["IID_BEASTAXE"] = new(15, false, "C", WeaponCategory.Axe),
        ["IID_URVAN"] = new(20, false, "S", WeaponCategory.Axe),

        // Bows
        ["IID_IRONBOW"] = new(45, false, "E", WeaponCategory.Bow),
        ["IID_STEELBOW"] = new(35, false, "D", WeaponCategory.Bow),
        ["IID_SILVERBOW"] = new(25, false, "A", WeaponCategory.Bow),
        ["IID_POISONBOW"] = new(40, false, "D", WeaponCategory.Bow),
        ["IID_KILLERBOW"] = new(15, false, "C", WeaponCategory.Bow),
        ["IID_BRAVEBOW"] = new(30, false, "B", WeaponCategory.Bow),
        ["IID_SHORTBOW"] = new(15, false, "C", WeaponCategory.Bow),
        ["IID_LONGBOW"] = new(20, false, "D", WeaponCategory.Bow),
        ["IID_LOFABOW"] = new(45, false, "★", WeaponCategory.Bow),
        ["IID_LONGLONGBOW"] = new(20, false, "S", WeaponCategory.Bow),
        ["IID_LIGHTNINGBOW"] = new(25, false, "B", WeaponCategory.Bow),
        ["IID_LONGARCH"] = new(5, false, "E", WeaponCategory.Bow),
        ["IID_IRONARCH"] = new(5, false, "E", WeaponCategory.Bow),
        ["IID_KILLERARCH"] = new(5, false, "E", WeaponCategory.Bow),
        ["IID_ONAGER"] = new(5, false, "E", WeaponCategory.Bow),

        // Staves
        ["IID_LIVE"] = new(30, false, "E", WeaponCategory.Staff),
        ["IID_RELIVE"] = new(20, false, "D", WeaponCategory.Staff),
        ["IID_RECOVER"] = new(15, false, "B", WeaponCategory.Staff),
        ["IID_REBLOW"] = new(15, false, "C", WeaponCategory.Staff),
        ["IID_RESERVE"] = new(8, false, "A", WeaponCategory.Staff),
        ["IID_REST"] = new(10, false, "C", WeaponCategory.Staff),
        ["IID_SILENCE"] = new(3, false, "B", WeaponCategory.Staff),
        ["IID_SLEEP"] = new(3, false, "B", WeaponCategory.Staff),
        ["IID_BERSERK"] = new(3, false, "B", WeaponCategory.Staff),
        ["IID_WARP"] = new(5, false, "A", WeaponCategory.Staff),
        ["IID_RESCUE"] = new(3, false, "B", WeaponCategory.Staff),
        ["IID_TORCH"] = new(10, false, "D", WeaponCategory.Staff),
        ["IID_HAMMERNE"] = new(3, false, "C", WeaponCategory.Staff),
        ["IID_UNLOCK"] = new(10, false, "D", WeaponCategory.Staff),
        ["IID_MSHIELD"] = new(15, false, "C", WeaponCategory.Staff),
        ["IID_GODDESSROD"] = new(3, false, "S", WeaponCategory.Staff),

        // Fire magic
        ["IID_FIRE"] = new(40, false, "E", WeaponCategory.Fire),
        ["IID_ELFIRE"] = new(30, false, "D", WeaponCategory.Fire),
        ["IID_BOLGANONE"] = new(20, false, "A", WeaponCategory.Fire),
        ["IID_FORBLAZE"] = new(20, false, "S", WeaponCategory.Fire),
        ["IID_METEOR"] = new(5, false, "C", WeaponCategory.Fire),

        // Thunder magic
        ["IID_THUNDER"] = new(40, false, "E", WeaponCategory.Thunder),
        ["IID_ELTHUNDER"] = new(30, false, "D", WeaponCategory.Thunder),
        ["IID_THORON"] = new(20, false, "A", WeaponCategory.Thunder),
        ["IID_MJOLNIR"] = new(20, false, "S", WeaponCategory.Thunder),
        ["IID_THUNDERSTORM"] = new(5, false, "C", WeaponCategory.Thunder),

        // Wind magic
        ["IID_WIND"] = new(40, false, "E", WeaponCategory.Wind),
        ["IID_ELWIND"] = new(30, false, "D", WeaponCategory.Wind),
        ["IID_TORNADO"] = new(20, false, "A", WeaponCategory.Wind),
        ["IID_FIMBULVETR"] = new(20, false, "S", WeaponCategory.Wind),
        ["IID_BLIZZARD"] = new(5, false, "C", WeaponCategory.Wind),

        // Light magic
        ["IID_LIGHTNING"] = new(40, false, "D", WeaponCategory.Staff),
        ["IID_SHINE"] = new(30, false, "C", WeaponCategory.Staff),
        ["IID_RESIRE"] = new(20, false, "A", WeaponCategory.Staff),
        ["IID_AURA"] = new(20, false, "S", WeaponCategory.Staff),
        ["IID_PURGE"] = new(5, false, "B", WeaponCategory.Staff),

        // Laguz weapons
        ["IID_LIONNAIL"] = new(46, true, ""),
        ["IID_TIGERNAIL"] = new(46, true, ""),
        ["IID_CATNAIL"] = new(46, true, ""),
        ["IID_WHITEBREATH"] = new(30, true, ""),
        ["IID_REDBREATH"] = new(46, true, ""),
        ["IID_HAWKBEAK"] = new(30, true, ""),
        ["IID_CROWBEAK"] = new(30, true, ""),
        ["IID_HAWKBEAK_KING"] = new(30, true, ""),
        ["IID_CROWBEAK_KING"] = new(30, true, ""),
    };

    public static bool TryGet(string? key, out WeaponStats stats)
    {
        if (key is not null)
        {
            return Stats.TryGetValue(key, out stats!);
        }

        stats = null!;
        return false;
    }

    public static int? GetRankRequirement(string rank)
        => rank switch
        {
            "E" => 1,
            "D" => 31,
            "C" => 71,
            "B" => 121,
            "A" => 181,
            "S" => 251,
            _ => null,
        };
}
