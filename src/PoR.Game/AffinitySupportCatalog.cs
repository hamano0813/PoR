namespace PoR.Game;

/// <summary>
/// Per-support-level affinity bonuses stored in half points so that the 0.5
/// values can be summed exactly before the final truncation.
/// </summary>
public readonly record struct AffinitySupportBonus(
    int AttackHalf,
    int DefenseHalf,
    int HitHalf,
    int AvoidHalf);

/// <summary>
/// Final, integer support bonuses after truncating the fractional part.
/// </summary>
public readonly record struct SupportBonus(
    int Attack,
    int Defense,
    int Hit,
    int Avoid);

/// <summary>
/// Support bonuses granted by each affinity for one support level.
/// Both characters in a support pair receive the sum of their own and their
/// partner's affinity effect.
/// </summary>
public static class AffinitySupportCatalog
{
    private static readonly Dictionary<string, AffinitySupportBonus> Bonuses =
        new(StringComparer.Ordinal)
        {
            ["AFF_HEAVEN"] = new(0, 0, 10, 0),  // 天: 命中 +5
            ["AFF_EARTH"] = new(0, 0, 0, 10),   // 地: 回避 +5
            ["AFF_FIRE"] = new(1, 0, 5, 0),     // 炎: 攻击 +0.5, 命中 +2.5
            ["AFF_DARK"] = new(1, 0, 0, 5),     // 暗: 攻击 +0.5, 回避 +2.5
            ["AFF_LIGHT"] = new(0, 1, 5, 0),    // 光: 防御 +0.5, 命中 +2.5
            ["AFF_THUNDER"] = new(0, 1, 0, 5),  // 雷: 防御 +0.5, 回避 +2.5
            ["AFF_WATER"] = new(1, 1, 0, 0),    // 水: 攻击 +0.5, 防御 +0.5
            ["AFF_WIND"] = new(0, 0, 5, 5),     // 风: 命中 +2.5, 回避 +2.5
        };

    public static bool TryGet(string? affinityKey, out AffinitySupportBonus bonus)
    {
        if (affinityKey is not null)
        {
            return Bonuses.TryGetValue(affinityKey, out bonus);
        }

        bonus = default;
        return false;
    }

    /// <summary>
    /// Sums both characters' affinity effects for the given support level and
    /// truncates the fractional part. Level 0 or an unknown affinity yields
    /// no bonus.
    /// </summary>
    public static SupportBonus Calculate(
        string? currentAffinity,
        string? partnerAffinity,
        int level)
    {
        if (level <= 0 ||
            !TryGet(currentAffinity, out var current) ||
            !TryGet(partnerAffinity, out var partner))
        {
            return default;
        }

        return new SupportBonus(
            (current.AttackHalf + partner.AttackHalf) * level / 2,
            (current.DefenseHalf + partner.DefenseHalf) * level / 2,
            (current.HitHalf + partner.HitHalf) * level / 2,
            (current.AvoidHalf + partner.AvoidHalf) * level / 2);
    }
}
