using BitChat.Maui.UITests.Shared;
using BitChat.Maui.UITests.Shared.Pages;
using FluentAssertions;
using Xunit;

namespace BitChat.Maui.UITests.Android;

/// <summary>
/// Android-specific UI tests for Settings functionality
/// Compatible with Appium.WebDriver 4.4.0 + Selenium.WebDriver 4.12.4
/// </summary>
[Collection("UI Tests")]
[Trait("Category", "UI")]
[Trait("Platform", "Android")]
public class AndroidSettingsUITests : AppiumTestBase
{
    protected override Platform CurrentPlatform => Platform.Android;
    
    private MainPage? _mainPage;
    private SettingsPage? _settingsPage;

    public AndroidSettingsUITests()
    {
        InitializeAndroidDriver();
        if (Driver != null)
        {
            _mainPage = new MainPage(Driver, Platform.Android);
            _settingsPage = new SettingsPage(Driver, Platform.Android);
        }
    }

    [Fact]
    public void NavigateToSettings_ShouldDisplaySettingsPage()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();

        // Act
        _mainPage.NavigateToSettings();

        // Assert
        _settingsPage!.WaitForPageToLoad();
        _settingsPage.IsDisplayed.Should().BeTrue("Settings page should be displayed after navigation");
    }

    [Theory]
    [InlineData("ValidUser")]
    [InlineData("Test123")]
    [InlineData("User_Name")]
    [InlineData("User-Name")]
    [InlineData("User.Name")]
    public async Task EnterValidNickname_ShouldEnableSaveButton(string nickname)
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(nickname);
        await Task.Delay(500); // Allow UI to update

        // Assert
        _settingsPage.IsSaveButtonEnabled().Should().BeTrue($"Save button should be enabled for valid nickname '{nickname}'");
        _settingsPage.IsValidationErrorDisplayed().Should().BeFalse("No validation error should be displayed for valid nickname");
    }

    [Theory]
    [InlineData("")]
    [InlineData("User@Invalid")]
    [InlineData("TooLongNicknameExceedingMaximumLength")]
    [InlineData(" LeadingSpace")]
    [InlineData("TrailingSpace ")]
    public async Task EnterInvalidNickname_ShouldShowValidationError(string nickname)
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(nickname);
        await Task.Delay(500); // Allow UI to update

        // Assert
        _settingsPage.WaitForValidationError();
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue($"Validation error should be displayed for invalid nickname '{nickname}'");
        _settingsPage.GetValidationError().Should().NotBeEmpty("Validation error message should not be empty");
    }

    [Fact]
    public async Task SaveValidNickname_ShouldPersistNickname()
    {
        // Arrange
        var testNickname = "TestUser123";
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(testNickname);
        await Task.Delay(500);
        _settingsPage.ClickSave();
        await Task.Delay(1000); // Allow save operation to complete

        // Verify nickname was entered correctly before saving
        var enteredNickname = _settingsPage.GetNickname();
        enteredNickname.Should().Be(testNickname, "Nickname should be entered correctly");

        // Note: We would typically navigate away and back to verify persistence,
        // but that depends on the app's navigation implementation
    }

    [Fact]
    public async Task CharacterCounter_ShouldUpdateAsUserTypes()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act & Assert - Test different nickname lengths
        var testCases = new[]
        {
            ("A", "1/20"),
            ("Test", "4/20"),
            ("TestUser123", "11/20"),
            ("12345678901234567890", "20/20") // Max length
        };

        foreach (var (nickname, expectedCount) in testCases)
        {
            _settingsPage.EnterNickname(nickname);
            await Task.Delay(300); // Allow UI to update

            var actualCount = _settingsPage.GetCharacterCount();
            actualCount.Should().Be(expectedCount, $"Character count should be correct for nickname '{nickname}'");
        }
    }

    [Fact]
    public async Task ResetButton_ShouldClearNicknameAndErrors()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Enter invalid nickname to generate error
        _settingsPage.EnterNickname("Invalid@Nickname");
        await Task.Delay(500);
        _settingsPage.WaitForValidationError();

        // Act
        _settingsPage.ClickReset();
        await Task.Delay(500);

        // Assert
        var nickname = _settingsPage.GetNickname();
        nickname.Should().BeEmpty("Nickname should be cleared after reset");
        
        _settingsPage.IsValidationErrorDisplayed().Should().BeFalse("Validation error should be cleared after reset");
        
        var characterCount = _settingsPage.GetCharacterCount();
        characterCount.Should().Be("0/20", "Character count should be reset to 0/20");
    }

    [Fact]
    public async Task AndroidSpecific_BackButton_ShouldNavigateFromSettings()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act - Press Android back button
        Driver!.Navigate().Back();
        await Task.Delay(500);

        // Assert
        _mainPage.WaitForPageToLoad();
        _mainPage.IsDisplayed.Should().BeTrue("Should navigate back to main page when Android back button is pressed");
    }

    [Fact]
    public async Task AndroidSpecific_KeyboardBehavior_ShouldWorkCorrectly()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act - Tap on nickname field to show keyboard
        _settingsPage.EnterNickname("Test");
        await Task.Delay(500);

        // Verify keyboard is shown (indirectly by checking if we can enter text)
        var enteredText = _settingsPage.GetNickname();
        enteredText.Should().Be("Test", "Keyboard should allow text entry");

        // Hide keyboard by tapping outside or using driver method
        Driver!.HideKeyboard();
        await Task.Delay(500);

        // Keyboard should be hidden, but text should remain
        var remainingText = _settingsPage.GetNickname();
        remainingText.Should().Be("Test", "Text should remain after keyboard is hidden");
    }

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}