using BitChat.Maui.UITests.Shared;
using Xunit;

namespace BitChat.Maui.UITests.Android;

/// <summary>
/// Android-specific implementation of nickname management UI tests
/// Inherits all test cases from the shared base class but provides Android-specific driver initialization and platform-aware locators
/// </summary>
[Collection("Android UI Tests")]
[Trait("Category", "UI")]
[Trait("Platform", "Android")]
[Trait("UserStory", "US-3.1.1")]
public class AndroidNicknameManagementUITests : NicknameManagementUITests
{
    protected override Platform CurrentPlatform => Platform.Android;

    protected override void InitializeDriver()
    {
        InitializeAndroidDriver();
        
        // Update page objects to use Android platform-specific locators
        if (Driver != null)
        {
            _mainPage = new Shared.Pages.MainPage(Driver, Platform.Android);
            _settingsPage = new Shared.Pages.SettingsPage(Driver, Platform.Android);
        }
    }
}