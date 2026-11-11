using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PoR.App.Services;

/// <summary>
/// Loads and caches the game icons copied from the original resource directory.
/// </summary>
public sealed class AssetService
{
    private readonly Dictionary<string, Bitmap?> _cache = new(StringComparer.Ordinal);

    public Bitmap? GetPidIcon(string? key)
        => GetIcon("PID", key, ".png") ?? GetIcon("PID", key, ".gif");

    public Bitmap? GetItemIcon(string? key)
        => GetIcon("IID", key, ".gif");

    public Bitmap? GetSkillIcon(string? key)
        => GetIcon("SID", key, ".gif");

    public Bitmap? GetAffinityIcon(string? key)
        => GetIcon("AFF", key, ".gif");

    public Bitmap? GetWeaponIcon(string? key)
        => GetIcon("WP", key is null ? null : "WP_" + key, ".png");

    public Bitmap? GetBlankIcon()
        => GetAsset("blank.png");

    private Bitmap? GetIcon(string folder, string? key, string suffix)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return GetBlankIcon();
        }

        return GetAsset($"{folder}/{key}{suffix}");
    }

    private Bitmap? GetAsset(string relativePath)
    {
        if (_cache.TryGetValue(relativePath, out var cached))
        {
            return cached;
        }

        Bitmap? bitmap = null;
        try
        {
            var uri = new Uri($"avares://PoR.App/Assets/{relativePath}");
            using var stream = AssetLoader.Open(uri);
            bitmap = new Bitmap(stream);
        }
        catch (Exception)
        {
            // Missing icon: callers can render the blank placeholder.
            bitmap = null;
        }

        _cache[relativePath] = bitmap;
        return bitmap;
    }
}
