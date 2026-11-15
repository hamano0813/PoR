namespace PoR.Game;

/// <summary>
/// Classifies the character's current class for equipment-slot rules.
/// </summary>
public static class CharacterClassClassifier
{
    private static readonly HashSet<string> LaguzClasses = new(StringComparer.Ordinal)
    {
        // Beast tribe: cat, lion, tiger (tribe and transformed classes)
        "JID_BEAST_C",
        "JID_BEAST_C/F",
        "JID_BEAST_L",
        "JID_BEAST_T",
        "JID_CAT",
        "JID_CAT/F",
        "JID_LION",
        "JID_TIGER",

        // Bird tribe: hawk and raven (tribe and transformed classes)
        "JID_BIRD_C",
        "JID_BIRD_C_NES",
        "JID_BIRD_HA",
        "JID_BIRD_HA_TIB",
        "JID_CROW",
        "JID_CROW_NES",
        "JID_HAWK",
        "JID_HAWK_TIB",

        // Dragon tribe
        "JID_DRAGON_B",
        "JID_BLACKDRAGON",
        "JID_DRAGON_R",
        "JID_DRAGON_R/F",
        "JID_DRAGON_W",
        "JID_REDDRAGON",
        "JID_REDDRAGON/F",
        "JID_WHITEDRAGON",
    };

    private static readonly HashSet<string> HeronClasses = new(StringComparer.Ordinal)
    {
        // Bird tribe: heron (tribe and transformed classes)
        "JID_BIRD_HE",
        "JID_BIRD_HE_W",
        "JID_BIRD_HE_W/F",
        "JID_HERON",
        "JID_HERON_W",
        "JID_HERON_W/F",
    };

    public static bool IsLaguz(string? classKey)
        => classKey is not null && LaguzClasses.Contains(classKey);

    public static bool IsHeron(string? classKey)
        => classKey is not null && HeronClasses.Contains(classKey);
}
