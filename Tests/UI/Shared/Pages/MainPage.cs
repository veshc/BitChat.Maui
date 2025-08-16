using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace BitChat.Maui.Tests.UI.Shared.Pages;

/// <summary>
/// Cross-platform Page Object Model for the Main page
/// Uses platform-aware locators for iOS (name/label/value) and Android (text/content-desc)
/// Compatible with Appium.WebDriver 5.0.0 + Selenium.WebDriver 4.x
/// </summary>
public class MainPage
{
    private readonly AppiumDriver _driver;
    private readonly Platform _platform;

    public MainPage(AppiumDriver driver, Platform platform)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _platform = platform;
    }

    // Backward compatibility constructor (assumes iOS for existing tests)
    public MainPage(AppiumDriver driver) : this(driver, Platform.iOS)
    {
    }

    #region Cross-Platform Element Locators

    // BitChat title and elements - using platform-aware locators
    private By BitChatTitleBy => PlatformLocatorStrategy.GetElementByText("BitChat", _platform);
    private By BitChatIconBy => PlatformLocatorStrategy.GetElementByText("💬", _platform);
    private By DescriptionBy => PlatformLocatorStrategy.GetElementContainingText("Decentralized messaging", _platform);
    
    // Status section
    private By StatusLabelBy => PlatformLocatorStrategy.GetElementContainingText("Status:", _platform);
    private By WelcomeMessageBy => PlatformLocatorStrategy.GetElementContainingText("Welcome to BitChat", _platform);
    
    // Settings navigation buttons (using ID first, then fallback to text)
    private By SettingsButtonBy => By.Id("SettingsButton");
    private By SettingsBtnBy => By.Id("SettingsBtn");
    
    // Alternative locators for Settings buttons - now platform-aware
    private By SettingsButtonByText => PlatformLocatorStrategy.GetElementByText("Configure Settings", _platform);
    private By SettingsGearButtonBy => PlatformLocatorStrategy.GetElementByText("⚙️", _platform);
    
    // Page title
    private By PageTitleBy => PlatformLocatorStrategy.GetElementByText("BitChat", _platform);

    #endregion

    #region Page Actions

    /// <summary>
    /// Checks if the Main page is displayed
    /// </summary>
    public bool IsDisplayed => IsBitChatTitleDisplayed() || IsWelcomeMessageDisplayed();

    /// <summary>
    /// Checks if the BitChat title is displayed
    /// </summary>
    private bool IsBitChatTitleDisplayed()
    {
        try
        {
            return _driver.FindElements(BitChatTitleBy).Count > 0;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the welcome message is displayed
    /// </summary>
    private bool IsWelcomeMessageDisplayed()
    {
        try
        {
            return _driver.FindElements(WelcomeMessageBy).Count > 0;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Navigates to the Settings page with retry logic and cross-platform support
    /// </summary>
    public async Task NavigateToSettingsAsync()
    {
        // Define locator strategies in priority order
        var locatorStrategies = new By[]
        {
            SettingsButtonBy, // Try ID first (most reliable)
            SettingsButtonByText, // Then platform-aware text locator
            SettingsBtnBy, // Try alternate ID
            SettingsGearButtonBy // Finally try gear icon
        };

        Exception? lastException = null;
        
        foreach (var locator in locatorStrategies)
        {
            try
            {
                var elements = _driver.FindElements(locator);
                if (elements.Count > 0)
                {
                    var settingsButton = elements.First();
                    
                    // Ensure element is clickable
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(d => settingsButton.Displayed && settingsButton.Enabled);
                    
                    settingsButton.Click();
                    
                    // Add small delay to allow navigation to complete
                    await Task.Delay(500);
                    return; // Success!
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                continue; // Try next locator strategy
            }
        }

        throw new NoSuchElementException(
            $"Settings button not found with any locator strategy on {_platform}. Last exception: {lastException?.Message}");
    }

    /// <summary>
    /// Synchronous version for backward compatibility
    /// </summary>
    public void NavigateToSettings()
    {
        NavigateToSettingsAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the current nickname displayed on the main page (if available)
    /// Uses platform-appropriate attribute for text retrieval
    /// </summary>
    public string GetDisplayedNickname()
    {
        try
        {
            // Try to find nickname display element (if it exists in future versions)
            var nicknameElement = _driver.FindElement(PlatformLocatorStrategy.GetElementContainingText("Nickname:", _platform));
            var textAttribute = PlatformLocatorStrategy.GetTextAttribute(_platform);
            return nicknameElement.GetAttribute(textAttribute) ?? string.Empty;
        }
        catch (NoSuchElementException)
        {
            // Current MainPage doesn't display nickname
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if Settings button is visible and enabled using platform-appropriate attributes
    /// </summary>
    public bool IsSettingsButtonEnabled()
    {
        try
        {
            // Try multiple locator strategies
            var locatorStrategies = new By[] { SettingsButtonBy, SettingsButtonByText, SettingsBtnBy };
            
            foreach (var locator in locatorStrategies)
            {
                var elements = _driver.FindElements(locator);
                if (elements.Count > 0)
                {
                    var element = elements.First();
                    var enabledAttribute = PlatformLocatorStrategy.GetEnabledAttribute(_platform);
                    var enabledValue = element.GetAttribute(enabledAttribute);
                    return bool.TryParse(enabledValue, out bool result) && result;
                }
            }

            return false;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    #endregion

    #region Wait Methods

    /// <summary>
    /// Waits for the Main page to be displayed
    /// Compatible with Selenium.WebDriver 4.x
    /// </summary>
    /// <param name="timeout">Maximum time to wait</param>
    public void WaitForPageToLoad(TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(_driver, timeout ?? TimeSpan.FromSeconds(30));
        wait.Until(driver => IsDisplayed);
    }

    /// <summary>
    /// Waits for the Settings button to be clickable with improved resilience
    /// </summary>
    /// <param name="timeout">Maximum time to wait</param>
    public async Task WaitForSettingsButtonAsync(TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(_driver, timeout ?? TimeSpan.FromSeconds(10));
        
        wait.Until(driver =>
        {
            try
            {
                return IsSettingsButtonEnabled();
            }
            catch (StaleElementReferenceException)
            {
                return false; // Element went stale, keep waiting
            }
        });
        
        // Add small delay to ensure button is fully ready
        await Task.Delay(100);
    }

    /// <summary>
    /// Synchronous version for backward compatibility
    /// </summary>
    public void WaitForSettingsButton(TimeSpan? timeout = null)
    {
        WaitForSettingsButtonAsync(timeout).GetAwaiter().GetResult();
    }

    #endregion
}