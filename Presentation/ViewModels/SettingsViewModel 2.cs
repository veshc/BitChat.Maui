using BitChat.Maui.Core.Interfaces;
using BitChat.Maui.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BitChat.Maui.Presentation.ViewModels;

/// <summary>
/// ViewModel for the Settings page, handling user identity management
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private string _nickname = string.Empty;

    [ObservableProperty]
    private string _nicknameError = string.Empty;

    [ObservableProperty]
    private bool _hasNicknameError;

    [ObservableProperty]
    private bool _canSave = true;

    [ObservableProperty]
    private int _characterCount;

    private string _originalNickname = string.Empty;

    public SettingsViewModel(ISettingsService settingsService, ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the ViewModel by loading current settings
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var identity = await _settingsService.GetUserIdentityAsync();
            Nickname = identity.Nickname;
            _originalNickname = identity.Nickname;
            UpdateCharacterCount();
            _logger.LogDebug("Settings initialized with nickname: {Nickname}", Nickname);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize settings");
            await Application.Current.MainPage.DisplayAlert("Error", 
                "Failed to load settings. Please try again.", "OK");
        }
    }

    /// <summary>
    /// Called when the nickname property changes
    /// </summary>
    partial void OnNicknameChanged(string value)
    {
        UpdateCharacterCount();
        ValidateNickname();
        UpdateCanSave();
    }

    /// <summary>
    /// Validates the current nickname and updates error state
    /// </summary>
    private void ValidateNickname()
    {
        var validationErrors = UserIdentity.ValidateNickname(Nickname);
        
        if (validationErrors.Any())
        {
            NicknameError = validationErrors.First().ErrorMessage ?? "Invalid nickname";
            HasNicknameError = true;
        }
        else
        {
            NicknameError = string.Empty;
            HasNicknameError = false;
        }
    }

    /// <summary>
    /// Updates the character count display
    /// </summary>
    private void UpdateCharacterCount()
    {
        CharacterCount = Nickname?.Length ?? 0;
    }

    /// <summary>
    /// Updates whether the save button should be enabled
    /// </summary>
    private void UpdateCanSave()
    {
        CanSave = !HasNicknameError && 
                  !string.IsNullOrWhiteSpace(Nickname) && 
                  Nickname != _originalNickname;
    }

    /// <summary>
    /// Command to save the current settings
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (HasNicknameError || string.IsNullOrWhiteSpace(Nickname))
        {
            return;
        }

        try
        {
            await _settingsService.SetNicknameAsync(Nickname);
            _originalNickname = Nickname;
            UpdateCanSave();

            _logger.LogInformation("Nickname saved: {Nickname}", Nickname);
            
            await Application.Current.MainPage.DisplayAlert("Success", 
                "Settings saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save nickname: {Nickname}", Nickname);
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Failed to save settings: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Command to reset all settings to defaults
    /// </summary>
    [RelayCommand]
    private async Task ResetAsync()
    {
        var confirm = await Application.Current.MainPage.DisplayAlert("Confirm Reset", 
            "Are you sure you want to reset all settings? This action cannot be undone.", 
            "Yes", "No");

        if (!confirm)
            return;

        try
        {
            await _settingsService.ClearAllSettingsAsync();
            await InitializeAsync(); // Reload with default values

            _logger.LogInformation("All settings reset to defaults");
            
            await Application.Current.MainPage.DisplayAlert("Reset Complete", 
                "All settings have been reset to defaults.", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset settings");
            await Application.Current.MainPage.DisplayAlert("Error", 
                "Failed to reset settings. Please try again.", "OK");
        }
    }
}