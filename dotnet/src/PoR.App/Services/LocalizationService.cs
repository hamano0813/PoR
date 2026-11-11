using System.ComponentModel;
using System.Text.Json;

namespace PoR.App.Services;

/// <summary>
/// Loads UI strings and game-term translations generated from the original
/// Qt .ts files. Changing the language notifies indexer bindings.
/// </summary>
public sealed class LocalizationService : INotifyPropertyChanged
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private IReadOnlyDictionary<string, string> _terms =
        new Dictionary<string, string>(StringComparer.Ordinal);
    private IReadOnlyDictionary<string, string> _ui =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public static LocalizationService Instance { get; } = new();

    public string Language { get; private set; } = "zh";

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;

    private LocalizationService()
    {
        SetLanguage("zh");
    }

    /// <summary>
    /// UI strings use plain keys; game terms can be read with the Term. prefix.
    /// </summary>
    public string this[string key]
    {
        get
        {
            if (key.StartsWith("Term.", StringComparison.Ordinal))
            {
                return Term(key[5..]);
            }

            return Ui(key);
        }
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

    public string Ui(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        return _ui.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : key;
    }

    public void SetLanguage(string language)
    {
        Language = language;
        _terms = Load("terms", language);
        _ui = Load("ui", language);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private static IReadOnlyDictionary<string, string> Load(
        string prefix,
        string language)
    {
        var assembly = typeof(LocalizationService).Assembly;
        var resourceName = $"PoR.App.Localization.{prefix}-{language}.json";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(stream, JsonOptions)
               ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }
}
