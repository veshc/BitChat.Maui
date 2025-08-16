using BitChat.Maui.Tests.UI.Shared.Pages;
using Xunit;

namespace BitChat.Maui.Tests.UI.Settings;

/// <summary>
/// iOS-specific UI tests for US-3.1.1: Nickname Management user story
/// Tests the complete user workflow for nickname validation and persistence on iOS
/// Compatible with Appium.WebDriver 5.0.0 + Selenium.WebDriver 4.x
/// </summary>
[Collection("UI Tests")]
[Trait("Category", "UI")]
[Trait("Platform", "iOS")]
[Trait("UserStory", "US-3.1.1")]
public class IOSNicknameManagementUITests : NicknameManagementUITests
{
    public override Shared.Platform CurrentPlatform => Shared.Platform.iOS;

    protected override void InitializeDriver()
    {
        InitializeIOSDriver();
        
        if (Driver != null)
        {
            _mainPage = new MainPage(Driver, CurrentPlatform);
            _settingsPage = new SettingsPage(Driver, CurrentPlatform);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}