using BitChat.Maui.Core.Models;

namespace BitChat.Maui.Core.Interfaces;

/// <summary>
/// Service interface for managing application settings and user preferences.
/// Provides abstraction over platform-specific storage mechanisms.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current user identity containing nickname and other user information
    /// </summary>
    /// <returns>The current user identity, or a default identity if none is set</returns>
    Task<UserIdentity> GetUserIdentityAsync();

    /// <summary>
    /// Updates the user's identity information
    /// </summary>
    /// <param name="identity">The new user identity to save</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown if the identity contains invalid data</exception>
    Task SetUserIdentityAsync(UserIdentity identity);

    /// <summary>
    /// Gets the user's current nickname
    /// </summary>
    /// <returns>The current nickname, or a default if none is set</returns>
    Task<string> GetNicknameAsync();

    /// <summary>
    /// Sets the user's nickname
    /// </summary>
    /// <param name="nickname">The new nickname to set</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown if the nickname is invalid</exception>
    Task SetNicknameAsync(string nickname);

    /// <summary>
    /// Clears all user settings and resets to defaults
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ClearAllSettingsAsync();

    /// <summary>
    /// Event raised when the user identity changes
    /// </summary>
    event EventHandler<UserIdentity>? UserIdentityChanged;
}