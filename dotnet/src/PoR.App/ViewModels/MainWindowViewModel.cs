using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoR.App.Services;
using PoR.Core.Game;
using PoR.Core.Memory;
using PoR.Game;
using PoR.Memory.Dolphin;

namespace PoR.App.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly LocalizationService _localization;
    private readonly AssetService _assets;
    private readonly WindowsDolphinMemory _memory;
    private readonly GameSession _session;
    private GameSnapshot? _snapshot;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _connectionText = "未连接";

    [ObservableProperty]
    private string _lastRefreshText = "尚未刷新";

    [ObservableProperty]
    private CharacterSlotViewModel? _selectedSlot;

    [ObservableProperty]
    private StatusTabViewModel? _statusTab;

    [ObservableProperty]
    private LanguageOption _selectedLanguage;

    [ObservableProperty]
    private bool _isDarkTheme;

    public MainWindowViewModel()
    {
        _localization = LocalizationService.Instance;
        _assets = new AssetService();
        _memory = new WindowsDolphinMemory();
        _session = new GameSession(_memory);
        Languages = [new LanguageOption("zh", "中文"), new LanguageOption("jp", "日本語")];
        _selectedLanguage = Languages[0];
        _connectionText = _localization.Ui("Ui_Status_Disconnected");
        _lastRefreshText = _localization.Ui("Ui_Status_NotRefreshed");

        if (Application.Current is { } application)
        {
            IsDarkTheme = application.ActualThemeVariant == ThemeVariant.Dark;
            application.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        }
    }

    public ObservableCollection<CharacterSlotViewModel> Slots { get; } = [];

    public IReadOnlyList<LanguageOption> Languages { get; }

    public bool IsNotBusy => !IsBusy;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string ThemeButtonText => _localization.Ui(
        IsDarkTheme ? "Ui_Action_ThemeLight" : "Ui_Action_ThemeDark");

    public string ThemeIcon => IsDarkTheme ? "" : "";

    public string LanguageButtonText => SelectedLanguage.Name;

    public IBrush ConnectionBrush => _session.Status switch
    {
        DolphinStatus.Hooked => Brushes.LimeGreen,
        DolphinStatus.NoEmu => Brushes.Goldenrod,
        DolphinStatus.AccessDenied => Brushes.IndianRed,
        _ => Brushes.Gray,
    };

    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var snapshot = await _session.RefreshAsync();
            _snapshot = snapshot;
            BuildSlots(snapshot);
        }
        catch (Exception exception)
        {
            _snapshot = null;
            Slots.Clear();
            SelectedSlot = null;
            StatusTab = null;
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
            UpdateConnection();
            UpdateLastRefreshText();
        }
    }

    [RelayCommand]
    private async Task ReconnectAsync()
    {
        if (IsBusy)
        {
            return;
        }

        _memory.Unhook();
        await RefreshAsync();
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        var nextCode = SelectedLanguage.Code == "zh" ? "jp" : "zh";
        SelectedLanguage =
            Languages.FirstOrDefault(language => language.Code == nextCode) ??
            SelectedLanguage;
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        OnPropertyChanged(nameof(ThemeButtonText));
        OnPropertyChanged(nameof(ThemeIcon));

        if (Application.Current is { } application)
        {
            application.RequestedThemeVariant = value
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }
    }

    partial void OnSelectedSlotChanged(CharacterSlotViewModel? value)
    {
        BuildStatus();
    }

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (value is null)
        {
            return;
        }

        _localization.SetLanguage(value.Code);

        if (_snapshot is not null)
        {
            BuildSlots(_snapshot);
        }
        else
        {
            foreach (var slot in Slots)
            {
                slot.RefreshText();
            }
        }

        UpdateConnection();
        UpdateLastRefreshText();
        OnPropertyChanged(nameof(ThemeButtonText));
        OnPropertyChanged(nameof(LanguageButtonText));
    }

    public void Dispose()
    {
        if (Application.Current is { } application)
        {
            application.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        }

        try
        {
            _memory.Unhook();
        }
        catch
        {
            // Closing the app must not throw.
        }

        _memory.Dispose();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (Application.Current is not { } application)
        {
            return;
        }

        var isDark = application.ActualThemeVariant == ThemeVariant.Dark;
        if (IsDarkTheme == isDark)
        {
            return;
        }

        // Keep the toggle in sync with system theme changes.
        IsDarkTheme = isDark;
    }

    private void BuildSlots(GameSnapshot snapshot)
    {
        var selectedIndex = SelectedSlot?.Index;
        Slots.Clear();

        foreach (var info in snapshot.SlotInfos.Where(info => !info.IsEmpty))
        {
            Slots.Add(new CharacterSlotViewModel(info, _localization, _assets));
        }

        SelectedSlot = selectedIndex is { } index
            ? Slots.FirstOrDefault(slot => slot.Index == index) ?? Slots.FirstOrDefault()
            : Slots.FirstOrDefault();
    }

    private void BuildStatus()
    {
        StatusTab = _snapshot is null || SelectedSlot is null
            ? null
            : new StatusTabViewModel(
                _snapshot,
                SelectedSlot.Info,
                _session,
                _localization);
    }

    private void UpdateConnection()
    {
        ConnectionText = _session.Status switch
        {
            DolphinStatus.Hooked =>
                $"{_localization.Ui("Ui_Status_Connected")} · PID {_memory.ProcessId} · " +
                $"MEM1 0x{_memory.Mem1HostBase:X}",
            DolphinStatus.NotRunning => _localization.Ui("Ui_Status_NotRunning"),
            DolphinStatus.NoEmu => _localization.Ui("Ui_Status_NoEmu"),
            DolphinStatus.AccessDenied => _localization.Ui("Ui_Status_AccessDenied"),
            _ => _localization.Ui("Ui_Status_Disconnected"),
        };
        OnPropertyChanged(nameof(ConnectionBrush));
    }

    private void UpdateLastRefreshText()
    {
        LastRefreshText = _snapshot is null
            ? _localization.Ui("Ui_Status_NotRefreshed")
            : $"{_localization.Ui("Ui_Status_LastRefresh")} {_snapshot.Timestamp:HH:mm:ss}";
    }
}
