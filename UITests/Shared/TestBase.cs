using System.Text;

namespace BitChat.Maui.UITests.Shared;

/// <summary>
/// Base class for all UI tests providing common utilities and test data generation
/// </summary>
public abstract class TestBase
{
    private static readonly Random _random = new();

    /// <summary>
    /// Creates a valid test nickname with optional suffix
    /// </summary>
    protected static string CreateTestNickname(string? suffix = null)
    {
        var baseNickname = "TestUser";
        if (!string.IsNullOrEmpty(suffix))
        {
            baseNickname += suffix;
        }
        
        // Add random number to ensure uniqueness
        baseNickname += _random.Next(1000, 9999);
        
        // Ensure it meets validation rules (1-20 characters)
        return baseNickname.Length > 20 ? baseNickname[..20] : baseNickname;
    }

    /// <summary>
    /// Generates invalid nickname for negative testing
    /// </summary>
    protected static string CreateInvalidNickname(string type)
    {
        return type switch
        {
            "empty" => "",
            "whitespace" => "   ",
            "toolong" => "ThisNicknameIsWayTooLongAndExceedsTheMaximumLengthLimit",
            "invalidchars" => "Test@User#123",
            "leadingspace" => " ValidUser",
            "trailingspace" => "ValidUser ",
            _ => "Invalid"
        };
    }
}