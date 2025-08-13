using BitChat.Maui.Core.Interfaces;
using BitChat.Maui.Core.Models;
using Microsoft.Extensions.Logging;

namespace BitChat.Maui.Core.Services;

/// <summary>
/// Implementation of ISettingsService using MAUI Preferences API for cross-platform storage
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    
    // Settings keys
    private const string NicknameKey = "user_nickname";
    private const string DefaultNickname = "Anonymous";

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler<UserIdentity>? UserIdentityChanged;

    /// <inheritdoc />
    public async Task<UserIdentity> GetUserIdentityAsync()
    {
        try
        {
            var nickname = await GetNicknameAsync();
            return new UserIdentity { Nickname = nickname };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user identity");
            return new UserIdentity { Nickname = DefaultNickname };
        }
    }

    /// <inheritdoc />
    public async Task SetUserIdentityAsync(UserIdentity identity)
    {
        if (identity == null)
            throw new ArgumentNullException(nameof(identity));

        await SetNicknameAsync(identity.Nickname);
    }

    /// <inheritdoc />
    public async Task<string> GetNicknameAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var nickname = Preferences.Get(NicknameKey, DefaultNickname);
                _logger.LogDebug("Retrieved nickname: {Nickname}", nickname);
                return nickname;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve nickname from preferences");
                return DefaultNickname;
            }
        });
    }

    /// <inheritdoc />
    public async Task SetNicknameAsync(string nickname)
    {
        // Validate the nickname before saving
        if (!UserIdentity.IsValidNickname(nickname))
        {
            var errors = UserIdentity.ValidateNickname(nickname);
            var errorMessage = string.Join(", ", errors.Select(e => e.ErrorMessage));
            throw new ArgumentException($"Invalid nickname: {errorMessage}");
        }

        await Task.Run(() =>
        {
            try
            {
                var trimmedNickname = nickname.Trim();
                var oldNickname = Preferences.Get(NicknameKey, DefaultNickname);
                
                Preferences.Set(NicknameKey, trimmedNickname);
                _logger.LogInformation("Nickname updated from '{OldNickname}' to '{NewNickname}'", 
                    oldNickname, trimmedNickname);

                // Raise the event on the UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var newIdentity = new UserIdentity { Nickname = trimmedNickname };
                    UserIdentityChanged?.Invoke(this, newIdentity);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save nickname to preferences");
                throw;
            }
        });
    }

    /// <inheritdoc />
    public async Task ClearAllSettingsAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                Preferences.Clear();
                _logger.LogInformation("All user settings cleared");

                // Raise event with default identity
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var defaultIdentity = new UserIdentity { Nickname = DefaultNickname };
                    UserIdentityChanged?.Invoke(this, defaultIdentity);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear all settings");
                throw;
            }
        });
    }
}