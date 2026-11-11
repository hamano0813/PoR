using System.Reflection;
using System.Text.Json;

namespace PoR.App.Services;

/// <summary>
/// Loads the game-term translations generated from the original Qt .ts files.
/// UI labels are intentionally Chinese for now; game terms switch zh/jp.
/// </summary>
public sealed class LocalizationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private IReadOnlyDictionary<string, string> _terms =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public static LocalizationService Instance { get; } = new();

    public string Language { get; private set; } = "zh";

    public event EventHandler? LanguageChanged;

    private LocalizationService()
    {
        SetLanguage("zh");
    }

    public string Term(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        return _terms.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : key;
    }

    public void SetLanguage(string language)
    {
        Language = language;
        _terms = LoadTerms(language);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private static IReadOnlyDictionary<string, string> LoadTerms(string language)
    {
        var assembly = typeof(LocalizationService).Assembly;
        var resourceName = $"PoR.App.Localization.terms.{language}.json";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(stream, JsonOptions)
               ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }
}
