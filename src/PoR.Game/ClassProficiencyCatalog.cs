namespace PoR.Game;

public enum WeaponCategory
{
    Sword,
    Lance,
    Axe,
    Bow,
    Fire,
    Thunder,
    Wind,
    Staff,
}

/// <summary>
/// Defines which weapon categories a class can use. Class-specific rank values
/// are intentionally not stored: every usable category starts at E. The
/// promoted/skill rules decide whether the cap is B or S.
/// </summary>
public static class ClassProficiencyCatalog
{
    public const int RankE = 1;
    public const int RankB = 121;
    public const int RankS = 251;

    private static readonly HashSet<string> LockedClasses = new(StringComparer.Ordinal)
    {
        "JID_CITIZEN",
        "JID_CITIZEN/F",
        "JID_CHILD",
        "JID_CHILD/F",
        "JID_UNUSED0/F",
        "JID_DUMMY",
        "JID_BIRD_HE",
        "JID_HERON",
    };

    private static readonly Dictionary<string, IReadOnlySet<WeaponCategory>> Classes =
        new(StringComparer.Ordinal)
        {
            ["JID_RANGER"] = A(WeaponCategory.Sword),
            ["JID_HERO"] = A(WeaponCategory.Sword),
            ["JID_HERO_G"] = A(WeaponCategory.Sword, WeaponCategory.Axe),

            ["JID_SWORDER"] = A(WeaponCategory.Sword),
            ["JID_SWORDER/F"] = A(WeaponCategory.Sword),
            ["JID_SWORDMASTER"] = A(WeaponCategory.Sword),
            ["JID_SWORDMASTER/F"] = A(WeaponCategory.Sword),
            ["JID_SOLDIER"] = A(WeaponCategory.Lance),
            ["JID_SOLDIER/F"] = A(WeaponCategory.Lance),
            ["JID_HALBERDIER"] = A(WeaponCategory.Lance),
            ["JID_HALBERDIER/F"] = A(WeaponCategory.Lance),
            ["JID_FIGHTER"] = A(WeaponCategory.Axe),
            ["JID_WARRIOR"] = A(WeaponCategory.Axe, WeaponCategory.Bow),
            ["JID_ARCHER"] = A(WeaponCategory.Bow),
            ["JID_SNIPER"] = A(WeaponCategory.Bow),
            ["JID_ARMOR"] = A(WeaponCategory.Sword),
            ["JID_GENERAL"] = A(WeaponCategory.Sword, WeaponCategory.Lance),

            ["JID_SOCIALKNIGHT_S"] = A(WeaponCategory.Sword),
            ["JID_SOCIALKNIGHT_S/F"] = A(WeaponCategory.Sword),
            ["JID_SOCIALKNIGHT_L"] = A(WeaponCategory.Lance),
            ["JID_SOCIALKNIGHT_L/F"] = A(WeaponCategory.Lance),
            ["JID_SOCIALKNIGHT_A"] = A(WeaponCategory.Axe),
            ["JID_SOCIALKNIGHT_A/F"] = A(WeaponCategory.Axe),
            ["JID_SOCIALKNIGHT_B"] = A(WeaponCategory.Bow),
            ["JID_SOCIALKNIGHT_B/F"] = A(WeaponCategory.Bow),

            ["JID_PALADIN_S"] = A(WeaponCategory.Sword),
            ["JID_PALADIN_S/F"] = A(WeaponCategory.Sword),
            ["JID_PALADIN_L"] = A(WeaponCategory.Lance),
            ["JID_PALADIN_L/F"] = A(WeaponCategory.Lance),
            ["JID_PALADIN_A"] = A(WeaponCategory.Axe),
            ["JID_PALADIN_A/F"] = A(WeaponCategory.Axe),
            ["JID_PALADIN_B"] = A(WeaponCategory.Bow),
            ["JID_PALADIN_B/F"] = A(WeaponCategory.Bow),
            ["JID_TIAMAT/F"] = A(WeaponCategory.Lance, WeaponCategory.Axe),

            ["JID_PEGASUSKNIGHT/F"] = A(WeaponCategory.Lance),
            ["JID_FALCONKNIGHT/F"] = A(WeaponCategory.Sword, WeaponCategory.Lance),
            ["JID_FALCONKNIGHT_E/F"] = A(WeaponCategory.Sword, WeaponCategory.Staff),
            ["JID_DRAGONKNIGHT"] = A(WeaponCategory.Lance),
            ["JID_DRAGONKNIGHT/F"] = A(WeaponCategory.Lance),
            ["JID_DRAGONMASTER"] = A(WeaponCategory.Lance, WeaponCategory.Axe),
            ["JID_DRAGONMASTER/F"] = A(WeaponCategory.Lance, WeaponCategory.Axe),
            ["JID_DRAGONMASTER_A"] = A(WeaponCategory.Sword, WeaponCategory.Axe),

            ["JID_MAGE_F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_MAGE_W"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_MAGE_T"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_MAGE"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_MAGE_F/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_MAGE_W/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_MAGE_T/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_MAGE/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),

            ["JID_SAGE_F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_W"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_T"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_F/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_W/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_T/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_R"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_R/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_R_F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_R_T"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_R_W"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_S"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_S/F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_S_F"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_S_T"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),
            ["JID_SAGE_S_W"] = A(WeaponCategory.Fire, WeaponCategory.Thunder, WeaponCategory.Wind),

            ["JID_PRIEST"] = A(WeaponCategory.Staff),
            ["JID_BISHOP"] = A(WeaponCategory.Staff),
            ["JID_BISHOP/F"] = A(WeaponCategory.Staff),
            ["JID_CLERIC/F"] = A(WeaponCategory.Staff),
            ["JID_VALKYRIE/F"] = A(WeaponCategory.Sword, WeaponCategory.Staff),

            ["JID_BANDIT"] = A(WeaponCategory.Axe),
            ["JID_BERSERKER"] = A(WeaponCategory.Axe),
            ["JID_BKNIGHT"] = A(WeaponCategory.Sword, WeaponCategory.Lance),
        };

    public static bool TryGet(
        string? classKey,
        out IReadOnlySet<WeaponCategory> categories)
    {
        if (classKey is not null && Classes.TryGetValue(classKey, out var value))
        {
            categories = value;
            return true;
        }

        categories = Empty;
        return false;
    }

    public static bool IsProficiencyLocked(string? classKey)
        => classKey is not null && LockedClasses.Contains(classKey);

    private static readonly IReadOnlySet<WeaponCategory> Empty =
        new HashSet<WeaponCategory>();

    private static IReadOnlySet<WeaponCategory> A(params WeaponCategory[] categories)
        => new HashSet<WeaponCategory>(categories);
}
