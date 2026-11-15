namespace PoR.Game;

/// <summary>
/// Personal weapons may be selected by anyone, but only their actual owner may
/// equip them.
/// </summary>
public static class PersonalWeaponCatalog
{
    private static readonly HashSet<string> Ike = new(StringComparer.Ordinal)
    {
        "PID_IKE",
        "PID_IKE_EV",
        "PID_IKE_EV1",
        "PID_IKE_EV2",
        "PID_IKE_CHILD_EV",
    };

    private static readonly HashSet<string> BlackKnight = new(StringComparer.Ordinal)
    {
        "PID_BLACKKNIGHT",
        "PID_BLACKKNIGHT_EV1",
        "PID_BLACKKNIGHT_EV2",
    };

    private static readonly HashSet<string> Ashnard = new(StringComparer.Ordinal)
    {
        "PID_ASHNARD",
        "PID_ASHNARD2",
    };

    private static readonly Dictionary<string, HashSet<string>> Owners =
        new(StringComparer.Ordinal)
        {
            ["IID_LONGSWORD"] = Ike,
            ["IID_RAGNELL"] = Ike,
            ["IID_ALONDITE"] = BlackKnight,
            ["IID_GURGURANT"] = Ashnard,
        };

    public static bool IsOwnerAllowed(string? weaponKey, string? personKey)
    {
        if (weaponKey is null || !Owners.TryGetValue(weaponKey, out var owners))
        {
            return true;
        }

        return personKey is not null && owners.Contains(personKey);
    }
}
