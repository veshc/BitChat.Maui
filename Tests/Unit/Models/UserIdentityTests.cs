using BitChat.Maui.Core.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace BitChat.Maui.Tests.Unit.Models;

/// <summary>
/// Unit tests for UserIdentity model, focusing on nickname validation logic
/// </summary>
public class UserIdentityTests : TestBase
{
    #region Nickname Validation Tests

    [Fact]
    public void ValidateNickname_WithValidNickname_ShouldReturnNoErrors()
    {
        // Arrange
        var nickname = "ValidUser123";

        // Act
        var errors = UserIdentity.ValidateNickname(nickname);

        // Assert
        errors.Should().BeEmpty("a valid nickname should not produce validation errors");
    }

    [Theory]
    [InlineData("A")]                    // Minimum length
    [InlineData("User")]                 // Regular name
    [InlineData("User123")]              // With numbers
    [InlineData("User_Name")]            // With underscore
    [InlineData("User-Name")]            // With dash
    [InlineData("User.Name")]            // With dot
    [InlineData("User Name")]            // With space
    [InlineData("12345678901234567890")]  // Maximum length (20 chars)
    public void ValidateNickname_WithValidFormats_ShouldReturnNoErrors(string nickname)
    {
        // Act
        var errors = UserIdentity.ValidateNickname(nickname);

        // Assert
        errors.Should().BeEmpty($"nickname '{nickname}' should be valid");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ValidateNickname_WithEmptyOrWhitespace_ShouldReturnError(string? nickname)
    {
        // Act
        var errors = UserIdentity.ValidateNickname(nickname ?? string.Empty);

        // Assert
        errors.Should().ContainSingle("empty or whitespace nicknames should produce exactly one error");
        errors.First().ErrorMessage.Should().Be("Nickname cannot be empty");
    }

    [Fact]
    public void ValidateNickname_WithTooLongNickname_ShouldReturnLengthError()
    {
        // Arrange
        var nickname = new string('A', 21); // 21 characters, exceeds max of 20

        // Act
        var errors = UserIdentity.ValidateNickname(nickname);

        // Assert
        errors.Should().ContainSingle("nicknames exceeding 20 characters should produce exactly one error");
        errors.First().ErrorMessage.Should().Be("Nickname cannot exceed 20 characters");
    }

    [Theory]
    [InlineData("User@Name")]      // At symbol
    [InlineData("User#Name")]      // Hash symbol
    [InlineData("User$Name")]      // Dollar symbol
    [InlineData("User%Name")]      // Percent symbol
    [InlineData("User&Name")]      // Ampersand
    [InlineData("User*Name")]      // Asterisk
    [InlineData("User(Name")]      // Parenthesis
    [InlineData("User)Name")]      // Parenthesis
    [InlineData("User+Name")]      // Plus sign
    [InlineData("User=Name")]      // Equals sign
    [InlineData("User[Name")]      // Bracket
    [InlineData("User]Name")]      // Bracket
    [InlineData("User{Name")]      // Brace
    [InlineData("User}Name")]      // Brace
    [InlineData("User|Name")]      // Pipe
    [InlineData("User\\Name")]     // Backslash
    [InlineData("User/Name")]      // Forward slash
    [InlineData("User:Name")]      // Colon
    [InlineData("User;Name")]      // Semicolon
    [InlineData("User\"Name")]     // Quote
    [InlineData("User'Name")]      // Apostrophe
    [InlineData("User<Name")]      // Less than
    [InlineData("User>Name")]      // Greater than
    [InlineData("User,Name")]      // Comma
    [InlineData("User?Name")]      // Question mark
    [InlineData("User!Name")]      // Exclamation
    [InlineData("User~Name")]      // Tilde
    [InlineData("User`Name")]      // Backtick
    public void ValidateNickname_WithInvalidCharacters_ShouldReturnCharacterError(string nickname)
    {
        // Act
        var errors = UserIdentity.ValidateNickname(nickname);

        // Assert
        errors.Should().ContainSingle($"nickname '{nickname}' with invalid characters should produce exactly one error");
        errors.First().ErrorMessage.Should().Be("Nickname can only contain letters, numbers, spaces, dots, dashes, and underscores");
    }

    [Theory]
    [InlineData(" ValidUser")]     // Leading space
    [InlineData("ValidUser ")]     // Trailing space
    [InlineData(" ValidUser ")]    // Leading and trailing space
    [InlineData("\tValidUser")]    // Leading tab
    [InlineData("ValidUser\t")]    // Trailing tab
    [InlineData("\nValidUser")]    // Leading newline
    [InlineData("ValidUser\n")]    // Trailing newline
    public void ValidateNickname_WithLeadingOrTrailingWhitespace_ShouldReturnWhitespaceError(string nickname)
    {
        // Act
        var errors = UserIdentity.ValidateNickname(nickname);

        // Assert
        errors.Should().ContainSingle($"nickname '{nickname}' with leading/trailing whitespace should produce exactly one error");
        errors.First().ErrorMessage.Should().Be("Nickname cannot start or end with spaces");
    }

    [Fact]
    public void ValidateNickname_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange - nickname that is too long AND has invalid characters
        var nickname = "@" + new string('A', 21); // 22 chars with invalid character

        // Act
        var errors = UserIdentity.ValidateNickname(nickname).ToList();

        // Assert
        errors.Should().HaveCount(2, "nickname with multiple issues should produce multiple errors");
        errors.Should().Contain(e => e.ErrorMessage == "Nickname cannot exceed 20 characters");
        errors.Should().Contain(e => e.ErrorMessage == "Nickname can only contain letters, numbers, spaces, dots, dashes, and underscores");
    }

    #endregion

    #region IsValidNickname Tests

    [Fact]
    public void IsValidNickname_WithValidNickname_ShouldReturnTrue()
    {
        // Arrange
        var nickname = "ValidUser123";

        // Act
        var isValid = UserIdentity.IsValidNickname(nickname);

        // Assert
        isValid.Should().BeTrue("a valid nickname should return true");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("User@Invalid")]
    [InlineData("TooLongNicknameExceedsMaxLength")]
    [InlineData(" LeadingSpace")]
    [InlineData("TrailingSpace ")]
    public void IsValidNickname_WithInvalidNickname_ShouldReturnFalse(string? nickname)
    {
        // Act
        var isValid = UserIdentity.IsValidNickname(nickname ?? string.Empty);

        // Assert
        isValid.Should().BeFalse($"invalid nickname '{nickname}' should return false");
    }

    #endregion

    #region WithNickname Factory Method Tests

    [Fact]
    public void WithNickname_WithValidNickname_ShouldCreateUserIdentity()
    {
        // Arrange
        var nickname = "ValidUser123";

        // Act
        var identity = UserIdentity.WithNickname(nickname);

        // Assert
        identity.Should().NotBeNull("factory method should create an instance");
        identity.Nickname.Should().Be(nickname, "nickname should be set correctly");
    }

    [Fact]
    public void WithNickname_WithNicknameWithWhitespace_ShouldTrimNickname()
    {
        // Arrange - nickname with internal whitespace but valid otherwise
        var nickname = "Valid User";

        // Act
        var identity = UserIdentity.WithNickname(nickname);

        // Assert
        identity.Nickname.Should().Be(nickname, "internal whitespace should be preserved");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("User@Invalid")]
    [InlineData("TooLongNicknameExceedsMaxLengthLimit")]
    [InlineData(" LeadingSpace")]
    [InlineData("TrailingSpace ")]
    public void WithNickname_WithInvalidNickname_ShouldThrowArgumentException(string? nickname)
    {
        // Act & Assert
        var action = () => UserIdentity.WithNickname(nickname ?? string.Empty);
        action.Should().Throw<ArgumentException>($"invalid nickname '{nickname}' should throw ArgumentException")
            .And.Message.Should().StartWith("Invalid nickname:");
    }

    [Fact]
    public void WithNickname_WithInvalidNickname_ShouldIncludeValidationErrorsInException()
    {
        // Arrange
        var nickname = "User@Invalid";

        // Act & Assert
        var action = () => UserIdentity.WithNickname(nickname);
        action.Should().Throw<ArgumentException>()
            .And.Message.Should().Contain("Nickname can only contain letters, numbers, spaces, dots, dashes, and underscores");
    }

    #endregion

    #region Record Behavior Tests

    [Fact]
    public void UserIdentity_AsRecord_ShouldSupportValueEquality()
    {
        // Arrange
        var nickname = CreateTestNickname();
        var identity1 = new UserIdentity { Nickname = nickname };
        var identity2 = new UserIdentity { Nickname = nickname };

        // Act & Assert
        identity1.Should().Be(identity2, "records with same values should be equal");
        identity1.GetHashCode().Should().Be(identity2.GetHashCode(), "records with same values should have same hash code");
    }

    [Fact]
    public void UserIdentity_AsRecord_ShouldSupportValueInequality()
    {
        // Arrange
        var identity1 = new UserIdentity { Nickname = CreateTestNickname("1") };
        var identity2 = new UserIdentity { Nickname = CreateTestNickname("2") };

        // Act & Assert
        identity1.Should().NotBe(identity2, "records with different values should not be equal");
    }

    [Fact]
    public void UserIdentity_AsRecord_ShouldSupportWith()
    {
        // Arrange
        var originalNickname = CreateTestNickname("Original");
        var newNickname = CreateTestNickname("New");
        var identity = new UserIdentity { Nickname = originalNickname };

        // Act
        var newIdentity = identity with { Nickname = newNickname };

        // Assert
        identity.Nickname.Should().Be(originalNickname, "original record should be unchanged");
        newIdentity.Nickname.Should().Be(newNickname, "new record should have updated nickname");
        newIdentity.Should().NotBe(identity, "records should be different instances");
    }

    #endregion

    #region Edge Cases and Performance Tests

    [Fact]
    public void ValidateNickname_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var nickname = "Ůşér123"; // Unicode characters

        // Act
        var errors = UserIdentity.ValidateNickname(nickname);

        // Assert
        errors.Should().BeEmpty("Unicode letters should be considered valid");
    }

    [Fact]
    public void ValidateNickname_WithEmojis_ShouldRejectEmojis()
    {
        // Arrange
        var nickname = "User😀123";

        // Act
        var errors = UserIdentity.ValidateNickname(nickname);

        // Assert
        errors.Should().ContainSingle("emojis should not be allowed in nicknames");
        errors.First().ErrorMessage.Should().Be("Nickname can only contain letters, numbers, spaces, dots, dashes, and underscores");
    }

    [Fact]
    public void ValidateNickname_PerformanceTest_ShouldBeEfficient()
    {
        // Arrange
        var nickname = CreateTestNickname();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Run validation 1000 times
        for (int i = 0; i < 1000; i++)
        {
            UserIdentity.ValidateNickname(nickname);
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "validation should be efficient even with multiple calls");
    }

    #endregion
}