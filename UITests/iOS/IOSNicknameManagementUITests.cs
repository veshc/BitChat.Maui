using BitChat.Maui.UITests.Shared;
using Xunit;

namespace BitChat.Maui.UITests.iOS;

/// <summary>
/// iOS-specific implementation of nickname management UI tests
/// Inherits all test cases from the shared base class but provides iOS-specific driver initialization and platform-aware locators
/// </summary>
[Collection("iOS UI Tests")]
[Trait("Category", "UI")]
[Trait("Platform", "iOS")]
[Trait("UserStory", "US-3.1.1")]
public class IOSNicknameManagementUITests : NicknameManagementUITests
{
    protected override Platform CurrentPlatform => Platform.iOS;

    protected override void InitializeDriver()
    {
        InitializeIOSDriver();
        
        // Update page objects to use iOS platform-specific locators
        if (Driver != null)
        {
            _mainPage = new Shared.Pages.MainPage(Driver, Platform.iOS);
            _settingsPage = new Shared.Pages.SettingsPage(Driver, Platform.iOS);
        }
    }
}