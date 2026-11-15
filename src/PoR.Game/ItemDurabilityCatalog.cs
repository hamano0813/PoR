namespace PoR.Game;

public readonly record struct ItemDurability(
    int Uses,
    bool Editable,
    int Minimum = 1);

/// <summary>
/// Default/maximum durability for consumable items in the four item slots.
/// Items not listed here keep the generic 0..255 editor range for now.
/// </summary>
public static class ItemDurabilityCatalog
{
    private static readonly Dictionary<string, ItemDurability> Entries =
        new(StringComparer.Ordinal)
        {
            // First batch: editable durability.
            ["IID_HPDROP"] = new(1, true),
            ["IID_POWERDROP"] = new(1, true),
            ["IID_MAGICDROP"] = new(1, true),
            ["IID_TECHDROP"] = new(1, true),
            ["IID_SPEEDDROP"] = new(1, true),
            ["IID_LUCKDROP"] = new(1, true),
            ["IID_DEFDROP"] = new(1, true),
            ["IID_MDDROP"] = new(1, true),
            ["IID_BOOTS"] = new(1, true),
            ["IID_BODYRING"] = new(1, true),
            ["IID_MASTERPROOF"] = new(1, true),
            ["IID_WEAPONDROP"] = new(1, true),
            ["IID_CHANGESTONE"] = new(2, true),
            ["IID_CHANGESTONE1"] = new(1, true),
            ["IID_TREASUREKEY"] = new(2, true),
            ["IID_DOORKEY"] = new(2, true),
            ["IID_VULNERARY"] = new(3, true),
            ["IID_ELIXIR"] = new(3, true),
            ["IID_HOLYWATER"] = new(3, true),
            ["IID_DETOXDRUG"] = new(3, true),
            ["IID_JACKLIGHT"] = new(5, true),

            // Second batch: fixed at one use and not editable.
            ["IID_SILVERCARD"] = new(1, false),
            ["IID_WHITEGEM"] = new(1, false),
            ["IID_BLUEGEM"] = new(1, false),
            ["IID_REDGEM"] = new(1, false),
            ["IID_ELITE"] = new(1, false),
            ["IID_SWIFT"] = new(1, false),
            ["IID_ESOTERIC"] = new(1, false),
            ["IID_BLAVE"] = new(1, false),
            ["IID_IMPREGNABLE"] = new(1, false),
            ["IID_TEMPER"] = new(1, false),
            ["IID_CALM"] = new(1, false),
            ["IID_RESCUEP"] = new(1, false),
            ["IID_AMBUSH"] = new(1, false),
            ["IID_GRASP"] = new(1, false),
            ["IID_ANGER"] = new(1, false),
            ["IID_WEAPONDESTROY"] = new(1, false),
            ["IID_PRAY"] = new(1, false),
            ["IID_COUNTER"] = new(1, false),
            ["IID_HORROR"] = new(1, false),
            ["IID_PROVOKE"] = new(1, false),
            ["IID_SHADE"] = new(1, false),
            ["IID_GAMBLE"] = new(1, false),
            ["IID_FAIRNESS"] = new(1, false),
            ["IID_TACKLE2"] = new(1, false),
            ["IID_CHANT"] = new(1, false),
            ["IID_DEFENCE"] = new(1, false),
            ["IID_CONTINUATION"] = new(1, false),
            ["IID_FRAC90"] = new(1, false),
            ["IID_REINFORCEMENTS"] = new(1, false),
            ["IID_TELEGNOSIS"] = new(1, false),
            ["IID_BIGEAR"] = new(1, false),
            ["IID_TURNREGENE"] = new(1, false),
            ["IID_COIN"] = new(1, false),

            // Accessories: fixed at one use and not editable.
            ["IID_CHANGERING"] = new(1, false),
            ["IID_CHANGERING2"] = new(1, false),
            ["IID_KNIGHTRING"] = new(1, false),
            ["IID_VSBEAST"] = new(1, false),
            ["IID_VSHUMAN"] = new(1, false),
            ["IID_DELPHISHIELD"] = new(1, false),
            ["IID_SWORDERSRING"] = new(1, false),
            ["IID_SOLDIERRING"] = new(1, false),
            ["IID_FIGHTERSRING"] = new(1, false),
            ["IID_ARCHERSRING"] = new(1, false),
            ["IID_ARMORSRING"] = new(1, false),
            ["IID_SOCIALKNIGHTSRING"] = new(1, false),
            ["IID_PEGASUSKNIGHTSRING"] = new(1, false),
            ["IID_DRAGONKNIGHTSRING"] = new(1, false),
            ["IID_MAGESRING"] = new(1, false),
            ["IID_PRIESTSRING"] = new(1, false),
            ["IID_THIEFSRING"] = new(1, false),
            ["IID_KNIGHTSHIELD"] = new(1, false),
        };

    public static bool TryGet(string? key, out ItemDurability durability)
    {
        if (key is not null)
        {
            return Entries.TryGetValue(key, out durability);
        }

        durability = default;
        return false;
    }
}
