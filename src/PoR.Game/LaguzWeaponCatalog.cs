namespace PoR.Game;

/// <summary>
/// The first weapon slot of a laguz character is limited to the strike weapon
/// belonging to its species.
/// </summary>
public static class LaguzWeaponCatalog
{
    public static string? GetWeaponKey(string? classKey)
        => classKey switch
        {
            "JID_BEAST_L" or "JID_LION" => "IID_LIONNAIL",
            "JID_BEAST_T" or "JID_TIGER" => "IID_TIGERNAIL",
            "JID_BEAST_C" or "JID_BEAST_C/F" or
                "JID_CAT" or "JID_CAT/F" => "IID_CATNAIL",
            "JID_DRAGON_W" or "JID_WHITEDRAGON" => "IID_WHITEBREATH",
            "JID_DRAGON_R" or "JID_DRAGON_R/F" or
                "JID_REDDRAGON" or "JID_REDDRAGON/F" => "IID_REDBREATH",
            "JID_BIRD_HA" or "JID_HAWK" => "IID_HAWKBEAK",
            "JID_BIRD_HA_TIB" or "JID_HAWK_TIB" => "IID_HAWKBEAK_KING",
            "JID_BIRD_C" or "JID_CROW" => "IID_CROWBEAK",
            "JID_BIRD_C_NES" or "JID_CROW_NES" => "IID_CROWBEAK_KING",
            _ => null,
        };
}
