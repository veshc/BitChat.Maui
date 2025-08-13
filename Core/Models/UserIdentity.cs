using System.ComponentModel.DataAnnotations;

namespace BitChat.Maui.Core.Models;

/// <summary>
/// Represents a user's identity information in the BitChat system.
/// This model encapsulates the user's nickname and will be extended 
/// with cryptographic identity information in future features.
/// </summary>
public record UserIdentity
{
    /// <summary>
    /// The display name chosen by the user, shown to other peers
    /// </summary>
    public string Nickname { get; init; } = string.Empty;

    /// <summary>
    /// Validates the nickname according to BitChat rules
    /// </summary>
    /// <param name="nickname">The nickname to validate</param>
    /// <returns>A collection of validation errors, empty if valid</returns>
    public static IEnumerable<ValidationResult> ValidateNickname(string nickname)
    {
        var results = new List<ValidationResult>();

        // Check for null or empty
        if (string.IsNullOrWhiteSpace(nickname))
        {
            results.Add(new ValidationResult("Nickname cannot be empty"));
            return results;
        }

        // Check length constraints
        if (nickname.Length < 1)
        {
            results.Add(new ValidationResult("Nickname must be at least 1 character long"));
        }
        else if (nickname.Length > 20)
        {
            results.Add(new ValidationResult("Nickname cannot exceed 20 characters"));
        }

        // Check for valid characters (alphanumeric, spaces, basic punctuation)
        var allowedChars = nickname.All(c => 
            char.IsLetterOrDigit(c) || 
            char.IsWhiteSpace(c) || 
            ".-_".Contains(c));

        if (!allowedChars)
        {
            results.Add(new ValidationResult("Nickname can only contain letters, numbers, spaces, dots, dashes, and underscores"));
        }

        // Check for leading/trailing whitespace
        if (nickname != nickname.Trim())
        {
            results.Add(new ValidationResult("Nickname cannot start or end with spaces"));
        }

        return results;
    }

    /// <summary>
    /// Checks if a nickname is valid according to BitChat rules
    /// </summary>
    /// <param name="nickname">The nickname to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidNickname(string nickname)
    {
        return !ValidateNickname(nickname).Any();
    }

    /// <summary>
    /// Creates a UserIdentity with a validated nickname
    /// </summary>
    /// <param name="nickname">The nickname to set</param>
    /// <returns>A new UserIdentity instance</returns>
    /// <exception cref="ArgumentException">Thrown if nickname is invalid</exception>
    public static UserIdentity WithNickname(string nickname)
    {
        var validationErrors = ValidateNickname(nickname);
        if (validationErrors.Any())
        {
            throw new ArgumentException($"Invalid nickname: {string.Join(", ", validationErrors.Select(v => v.ErrorMessage))}");
        }

        return new UserIdentity { Nickname = nickname.Trim() };
    }
}