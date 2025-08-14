using BitChat.Maui.UITests.Shared.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace BitChat.Maui.UITests.Shared;

/// <summary>
/// Comprehensive UI tests for US-3.1.1: Nickname Management user story
/// Tests the complete user workflow for nickname validation and persistence
/// Compatible with Appium.WebDriver 4.4.0 + Selenium.WebDriver 4.12.4
/// </summary>
[Collection("UI Tests")]
[Trait("Category", "UI")]
[Trait("UserStory", "US-3.1.1")]
public abstract class NicknameManagementUITests : AppiumTestBase
{
    protected MainPage? _mainPage;
    protected SettingsPage? _settingsPage;

    protected abstract void InitializeDriver();

    public NicknameManagementUITests()
    {
        InitializeDriver();
        if (Driver != null && _mainPage == null && _settingsPage == null)
        {
            // Default to iOS platform if not overridden by derived class
            _mainPage = new MainPage(Driver, CurrentPlatform);
            _settingsPage = new SettingsPage(Driver, CurrentPlatform);
        }
    }

    protected override Platform CurrentPlatform => Platform.iOS; // Default platform

    #region AC1: User can navigate to settings page

    [Fact]
    public void AC1_NavigateToSettings_ShouldDisplaySettingsPage()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad(TimeSpan.FromSeconds(30));

        // Act
        _mainPage.NavigateToSettings();

        // Assert
        _settingsPage!.WaitForPageToLoad();
        _settingsPage.IsDisplayed.Should().BeTrue("Settings page should be displayed after navigation");
    }

    [Fact]
    public void AC1_SettingsPage_ShouldDisplayAllRequiredElements()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act & Assert - Verify all UI elements are present
        _settingsPage.IsDisplayed.Should().BeTrue("Settings page should be displayed");
        
        // Verify nickname input field exists
        var nicknameFieldExists = IsElementPresent(By.Id("NicknameEntry"));
        nicknameFieldExists.Should().BeTrue("Nickname input field should be present");
        
        // Verify character counter exists - using platform-aware locator
        var characterCounterExists = IsElementPresent(PlatformLocatorStrategy.GetElementContainingText("/20 characters", CurrentPlatform));
        characterCounterExists.Should().BeTrue("Character counter should be present");
        
        // Verify save button exists - using platform-aware locator
        var saveButtonExists = IsElementPresent(PlatformLocatorStrategy.GetElementByText("Save Changes", CurrentPlatform));
        saveButtonExists.Should().BeTrue("Save button should be present");
    }

    #endregion

    #region AC2: User can enter a valid nickname

    [Theory]
    [InlineData("User")]
    [InlineData("TestUser123")]
    [InlineData("User_Name")]
    [InlineData("User-Name")]
    [InlineData("User.Name")]
    [InlineData("User Name")]
    [InlineData("A")] // Minimum length
    [InlineData("12345678901234567890")] // Maximum length (20 chars)
    public async Task AC2_EnterValidNickname_ShouldAcceptNickname(string validNickname)
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(validNickname);
        await Task.Delay(500); // Allow UI to update

        // Assert
        var enteredNickname = _settingsPage.GetNickname();
        enteredNickname.Should().Be(validNickname, $"Valid nickname '{validNickname}' should be accepted");
        
        _settingsPage.IsValidationErrorDisplayed().Should().BeFalse("No validation error should be displayed for valid nickname");
        _settingsPage.IsSaveButtonEnabled().Should().BeTrue("Save button should be enabled for valid nickname");
    }

    #endregion

    #region AC3: System validates nickname format

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task AC3_EnterEmptyNickname_ShouldShowValidationError(string emptyNickname)
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(emptyNickname);
        await Task.Delay(500);

        // Assert
        _settingsPage.WaitForValidationError();
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue("Validation error should be displayed for empty nickname");
        var errorMessage = _settingsPage.GetValidationError();
        errorMessage.Should().NotBeEmpty("Error message should not be empty");
        errorMessage.Should().Contain("empty", "Error message should mention that nickname cannot be empty");
    }

    [Theory]
    [InlineData("User@Invalid")]
    [InlineData("User#Hash")]
    [InlineData("User$Dollar")]
    [InlineData("User%Percent")]
    [InlineData("User&Ampersand")]
    [InlineData("User*Asterisk")]
    [InlineData("User+Plus")]
    [InlineData("User=Equals")]
    [InlineData("User!Exclamation")]
    [InlineData("User?Question")]
    [InlineData("User<Less")]
    [InlineData("User>Greater")]
    [InlineData("User/Slash")]
    [InlineData("User\\Backslash")]
    [InlineData("User|Pipe")]
    [InlineData("User:Colon")]
    [InlineData("User;Semicolon")]
    [InlineData("User\"Quote")]
    [InlineData("User'Apostrophe")]
    [InlineData("User,Comma")]
    [InlineData("User~Tilde")]
    [InlineData("User`Backtick")]
    public async Task AC3_EnterInvalidCharacters_ShouldShowValidationError(string invalidNickname)
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(invalidNickname);
        await Task.Delay(500);

        // Assert
        _settingsPage.WaitForValidationError();
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue($"Validation error should be displayed for nickname with invalid characters: '{invalidNickname}'");
        var errorMessage = _settingsPage.GetValidationError();
        errorMessage.Should().NotBeEmpty("Error message should not be empty");
        errorMessage.Should().Contain("can only contain", "Error message should explain allowed characters");
    }

    [Fact]
    public async Task AC3_EnterTooLongNickname_ShouldShowValidationError()
    {
        // Arrange
        var tooLongNickname = new string('A', 21); // 21 characters, exceeds limit of 20
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(tooLongNickname);
        await Task.Delay(500);

        // Assert
        _settingsPage.WaitForValidationError();
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue("Validation error should be displayed for nickname that is too long");
        var errorMessage = _settingsPage.GetValidationError();
        errorMessage.Should().NotBeEmpty("Error message should not be empty");
        errorMessage.Should().Contain("cannot exceed", "Error message should mention length limit");
    }

    [Theory]
    [InlineData(" LeadingSpace")]
    [InlineData("TrailingSpace ")]
    [InlineData(" BothSides ")]
    [InlineData("\tTabLeading")]
    [InlineData("TabTrailing\t")]
    [InlineData("\nNewlineLeading")]
    [InlineData("NewlineTrailing\n")]
    public async Task AC3_EnterNicknameWithLeadingTrailingWhitespace_ShouldShowValidationError(string nickname)
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(nickname);
        await Task.Delay(500);

        // Assert
        _settingsPage.WaitForValidationError();
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue($"Validation error should be displayed for nickname with leading/trailing whitespace: '{nickname}'");
        var errorMessage = _settingsPage.GetValidationError();
        errorMessage.Should().NotBeEmpty("Error message should not be empty");
        errorMessage.Should().Contain("cannot start or end", "Error message should mention leading/trailing spaces");
    }

    #endregion

    #region AC4: System shows character count

    [Fact]
    public void AC4_CharacterCounter_ShouldStartAtZero()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act & Assert
        var characterCount = _settingsPage.GetCharacterCount();
        characterCount.Should().MatchRegex(@"0/20( characters)?", "Character counter should start at 0/20");
    }

    [Theory]
    [InlineData("A", "1")]
    [InlineData("Test", "4")]
    [InlineData("TestUser", "8")]
    [InlineData("TestUser123", "11")]
    [InlineData("VeryLongNickname", "16")]
    [InlineData("12345678901234567890", "20")] // Maximum length
    public async Task AC4_CharacterCounter_ShouldUpdateAsUserTypes(string nickname, string expectedCount)
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(nickname);
        await Task.Delay(300); // Allow UI to update

        // Assert
        var characterCount = _settingsPage.GetCharacterCount();
        characterCount.Should().MatchRegex($@"{expectedCount}/20( characters)?", 
            $"Character count should be {expectedCount}/20 for nickname '{nickname}'");
    }

    [Fact]
    public async Task AC4_CharacterCounter_ShouldUpdateInRealTime()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act & Assert - Test progressive typing
        var testCases = new[]
        {
            ("T", "1/20"),
            ("Te", "2/20"),
            ("Tes", "3/20"),
            ("Test", "4/20"),
            ("TestU", "5/20")
        };

        foreach (var (nickname, expectedCount) in testCases)
        {
            _settingsPage.EnterNickname(nickname);
            await Task.Delay(200);

            var actualCount = _settingsPage.GetCharacterCount();
            actualCount.Should().MatchRegex($@"{expectedCount.Split('/')[0]}/20( characters)?", 
                $"Character count should update to {expectedCount} when typing '{nickname}'");
        }
    }

    #endregion

    #region AC5: User can save valid nickname

    [Fact]
    public async Task AC5_SaveValidNickname_ShouldPersistNickname()
    {
        // Arrange
        var testNickname = "TestUser123";
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(testNickname);
        await Task.Delay(500);
        
        // Verify Save button is enabled
        _settingsPage.IsSaveButtonEnabled().Should().BeTrue("Save button should be enabled for valid nickname");
        
        _settingsPage.ClickSave();
        await Task.Delay(1000); // Allow save operation to complete

        // Assert
        var savedNickname = _settingsPage.GetNickname();
        savedNickname.Should().Be(testNickname, "Nickname should be persisted after saving");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Invalid@Nickname")]
    [InlineData("TooLongNicknameExceedingMaximumLength")]
    [InlineData(" LeadingSpace")]
    [InlineData("TrailingSpace ")]
    public async Task AC5_SaveInvalidNickname_ShouldNotAllowSave(string invalidNickname)
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(invalidNickname);
        await Task.Delay(500);

        // Assert
        _settingsPage.IsSaveButtonEnabled().Should().BeFalse($"Save button should be disabled for invalid nickname: '{invalidNickname}'");
    }

    #endregion

    #region AC6: System prevents saving invalid nicknames

    [Fact]
    public async Task AC6_SaveButton_ShouldBeDisabledForEmptyNickname()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname("");
        await Task.Delay(500);

        // Assert
        _settingsPage.IsSaveButtonEnabled().Should().BeFalse("Save button should be disabled when nickname is empty");
    }

    [Fact]
    public async Task AC6_SaveButton_ShouldBeEnabledOnlyForValidNicknames()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Test invalid -> valid -> invalid cycle
        
        // Start with invalid nickname
        _settingsPage.EnterNickname("Invalid@Nick");
        await Task.Delay(500);
        _settingsPage.IsSaveButtonEnabled().Should().BeFalse("Save button should be disabled for invalid nickname");

        // Change to valid nickname
        _settingsPage.EnterNickname("ValidNick");
        await Task.Delay(500);
        _settingsPage.IsSaveButtonEnabled().Should().BeTrue("Save button should be enabled for valid nickname");

        // Change back to invalid nickname
        _settingsPage.EnterNickname("");
        await Task.Delay(500);
        _settingsPage.IsSaveButtonEnabled().Should().BeFalse("Save button should be disabled when nickname becomes invalid again");
    }

    #endregion

    #region AC7: User can reset/clear nickname

    [Fact]
    public async Task AC7_ResetButton_ShouldClearNickname()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Enter a nickname first
        _settingsPage.EnterNickname("TestUser123");
        await Task.Delay(500);

        // Verify nickname was entered
        var enteredNickname = _settingsPage.GetNickname();
        enteredNickname.Should().Be("TestUser123", "Nickname should be entered before reset");

        // Act
        _settingsPage.ClickReset();
        await Task.Delay(500);

        // Assert
        var clearedNickname = _settingsPage.GetNickname();
        clearedNickname.Should().BeEmpty("Nickname should be cleared after reset");
        
        var characterCount = _settingsPage.GetCharacterCount();
        characterCount.Should().MatchRegex(@"0/20( characters)?", "Character count should be reset to 0/20");
    }

    [Fact]
    public async Task AC7_ResetButton_ShouldClearValidationErrors()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Enter invalid nickname to generate error
        _settingsPage.EnterNickname("Invalid@Nickname");
        await Task.Delay(500);
        _settingsPage.WaitForValidationError();
        
        // Verify error is displayed
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue("Validation error should be displayed before reset");

        // Act
        _settingsPage.ClickReset();
        await Task.Delay(500);

        // Assert
        _settingsPage.IsValidationErrorDisplayed().Should().BeFalse("Validation error should be cleared after reset");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task EdgeCase_UnicodeCharacters_ShouldBeAccepted()
    {
        // Arrange
        var unicodeNickname = "Üser123"; // Unicode characters
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(unicodeNickname);
        await Task.Delay(500);

        // Assert
        _settingsPage.IsValidationErrorDisplayed().Should().BeFalse("Unicode characters should be accepted in nicknames");
        _settingsPage.IsSaveButtonEnabled().Should().BeTrue("Save button should be enabled for valid Unicode nickname");
    }

    [Fact]
    public async Task EdgeCase_EmojiCharacters_ShouldBeRejected()
    {
        // Arrange
        var emojiNickname = "User😀123";
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Act
        _settingsPage.EnterNickname(emojiNickname);
        await Task.Delay(500);

        // Assert
        _settingsPage.WaitForValidationError();
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue("Emoji characters should be rejected in nicknames");
        _settingsPage.IsSaveButtonEnabled().Should().BeFalse("Save button should be disabled for nickname with emojis");
    }

    [Fact]
    public async Task EdgeCase_MaxLengthBoundaryTest()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();

        // Test exactly 20 characters (valid)
        var exactMax = "12345678901234567890"; // Exactly 20 chars
        _settingsPage.EnterNickname(exactMax);
        await Task.Delay(500);
        
        _settingsPage.IsValidationErrorDisplayed().Should().BeFalse("20 characters should be valid (boundary test)");
        _settingsPage.IsSaveButtonEnabled().Should().BeTrue("Save button should be enabled for 20-character nickname");

        // Test 21 characters (invalid)
        var overMax = exactMax + "1"; // 21 chars
        _settingsPage.EnterNickname(overMax);
        await Task.Delay(500);
        
        _settingsPage.WaitForValidationError();
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue("21 characters should be invalid (boundary test)");
        _settingsPage.IsSaveButtonEnabled().Should().BeFalse("Save button should be disabled for 21-character nickname");
    }

    [Fact]
    public async Task Integration_CompleteNicknameWorkflow()
    {
        // Arrange
        _mainPage!.WaitForPageToLoad();

        // Act & Assert - Complete user workflow
        
        // 1. Navigate to settings
        _mainPage.NavigateToSettings();
        _settingsPage!.WaitForPageToLoad();
        _settingsPage.IsDisplayed.Should().BeTrue("Should navigate to settings page");

        // 2. Enter invalid nickname and see error
        _settingsPage.EnterNickname("Invalid@Nickname");
        await Task.Delay(500);
        _settingsPage.WaitForValidationError();
        _settingsPage.IsValidationErrorDisplayed().Should().BeTrue("Should show validation error for invalid nickname");
        _settingsPage.IsSaveButtonEnabled().Should().BeFalse("Save button should be disabled for invalid nickname");

        // 3. Correct to valid nickname
        _settingsPage.EnterNickname("ValidUser123");
        await Task.Delay(500);
        _settingsPage.IsValidationErrorDisplayed().Should().BeFalse("Validation error should disappear for valid nickname");
        _settingsPage.IsSaveButtonEnabled().Should().BeTrue("Save button should be enabled for valid nickname");

        // 4. Verify character count
        var characterCount = _settingsPage.GetCharacterCount();
        characterCount.Should().MatchRegex(@"12/20( characters)?", "Character count should show 12/20");

        // 5. Save the nickname
        _settingsPage.ClickSave();
        await Task.Delay(1000);
        var savedNickname = _settingsPage.GetNickname();
        savedNickname.Should().Be("ValidUser123", "Nickname should be persisted after save");

        // 6. Test reset functionality
        _settingsPage.ClickReset();
        await Task.Delay(500);
        var clearedNickname = _settingsPage.GetNickname();
        clearedNickname.Should().BeEmpty("Nickname should be cleared after reset");
    }

    #endregion

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}