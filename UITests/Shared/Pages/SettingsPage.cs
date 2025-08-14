using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace BitChat.Maui.UITests.Shared.Pages;

/// <summary>
/// Cross-platform Page Object Model for the Settings page
/// Uses platform-aware locators for iOS (name/label/value) and Android (text/content-desc)
/// Compatible with Appium.WebDriver 4.4.0 + Selenium.WebDriver 4.12.4
/// </summary>
public class SettingsPage
{
    private readonly AppiumDriver<IWebElement> _driver;
    private readonly Platform _platform;

    public SettingsPage(AppiumDriver<IWebElement> driver, Platform platform)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _platform = platform;
    }

    // Backward compatibility constructor (assumes iOS for existing tests)
    public SettingsPage(AppiumDriver<IWebElement> driver) : this(driver, Platform.iOS)
    {
    }

    #region Cross-Platform Element Locators

    // Page title - using platform-aware locators
    private By PageTitleBy => PlatformLocatorStrategy.GetElementByText("Settings", _platform);
    
    // Header elements
    private By HeaderTitleBy => PlatformLocatorStrategy.GetElementByText("BitChat Settings", _platform);
    private By UserIdentitySectionBy => PlatformLocatorStrategy.GetElementByText("User Identity", _platform);
    
    // Nickname input field (ID works across platforms)
    private By NicknameEntryBy => By.Id("NicknameEntry");
    
    // Character counter - platform-aware
    private By CharacterCounterBy => PlatformLocatorStrategy.GetElementContainingText("/20 characters", _platform);
    
    // Validation error - simplified platform-aware approach
    private By ValidationErrorBy => PlatformLocatorStrategy.GetElementContainingText("error", _platform);
    
    // Save button - platform-aware
    private By SaveButtonBy => PlatformLocatorStrategy.GetElementByText("Save Changes", _platform);
    
    // Reset button - platform-aware
    private By ResetButtonBy => PlatformLocatorStrategy.GetElementByText("Reset All Settings", _platform);
    
    // About section - platform-aware
    private By AboutSectionBy => PlatformLocatorStrategy.GetElementByText("About BitChat", _platform);
    private By VersionLabelBy => PlatformLocatorStrategy.GetElementByText("Version 1.0.0", _platform);

    #endregion

    #region Page Actions

    /// <summary>
    /// Checks if the Settings page is displayed
    /// </summary>
    public bool IsDisplayed => _driver.FindElements(HeaderTitleBy).Count > 0 || _driver.FindElements(PageTitleBy).Count > 0;

    /// <summary>
    /// Gets the current nickname from the input field using platform-appropriate attributes
    /// </summary>
    public string GetNickname()
    {
        var element = _driver.FindElement(NicknameEntryBy);
        var textAttribute = PlatformLocatorStrategy.GetTextAttribute(_platform);
        return element.GetAttribute(textAttribute) ?? string.Empty;
    }

    /// <summary>
    /// Enters a nickname in the input field with validation and retry logic
    /// </summary>
    /// <param name="nickname">The nickname to enter</param>
    public async Task EnterNicknameAsync(string nickname)
    {
        const int maxAttempts = 3;
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var element = _driver.FindElement(NicknameEntryBy);
                
                // Ensure element is ready for interaction
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                wait.Until(d => element.Displayed && element.Enabled);
                
                element.Clear();
                
                // Small delay after clear to ensure it completed
                await Task.Delay(100);
                
                element.SendKeys(nickname);
                
                // Verify text was entered correctly
                var textAttribute = PlatformLocatorStrategy.GetTextAttribute(_platform);
                var actualText = element.GetAttribute(textAttribute) ?? string.Empty;
                
                if (actualText == nickname)
                {
                    return; // Success!
                }
                
                if (attempt == maxAttempts)
                {
                    throw new InvalidOperationException($"Failed to enter nickname after {maxAttempts} attempts. Expected: '{nickname}', Actual: '{actualText}'");
                }
                
                // Retry with exponential backoff
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100));
            }
            catch (Exception ex) when (attempt < maxAttempts && 
                (ex is StaleElementReferenceException || ex is ElementNotInteractableException))
            {
                // Retry on transient errors
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100));
            }
        }
    }
    
    /// <summary>
    /// Synchronous version for backward compatibility
    /// </summary>
    public void EnterNickname(string nickname)
    {
        EnterNicknameAsync(nickname).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Clicks the Save button
    /// </summary>
    public void ClickSave()
    {
        var element = _driver.FindElement(SaveButtonBy);
        element.Click();
    }

    /// <summary>
    /// Clicks the Reset button
    /// </summary>
    public void ClickReset()
    {
        var element = _driver.FindElement(ResetButtonBy);
        element.Click();
    }

    /// <summary>
    /// Gets the current character count text using platform-appropriate attributes
    /// </summary>
    public string GetCharacterCount()
    {
        try
        {
            var element = _driver.FindElement(CharacterCounterBy);
            var textAttribute = PlatformLocatorStrategy.GetTextAttribute(_platform);
            return element.GetAttribute(textAttribute) ?? string.Empty;
        }
        catch (NoSuchElementException)
        {
            // If character counter not found, try alternative approach
            var alternativeBy = _platform switch
            {
                Platform.iOS => By.XPath("//*[contains(@name, '/20') or contains(@label, '/20') or contains(@value, '/20')]"),
                Platform.Android => By.XPath("//*[contains(@text, '/20') or contains(@content-desc, '/20')]"),
                _ => CharacterCounterBy
            };
            
            var element = _driver.FindElement(alternativeBy);
            var textAttribute = PlatformLocatorStrategy.GetTextAttribute(_platform);
            return element.GetAttribute(textAttribute) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the validation error message using platform-appropriate attributes
    /// </summary>
    public string GetValidationError()
    {
        try
        {
            // Try multiple strategies to find error messages
            var errorStrategies = new By[]
            {
                ValidationErrorBy,
                PlatformLocatorStrategy.GetElementContainingText("cannot", _platform),
                PlatformLocatorStrategy.GetElementContainingText("invalid", _platform),
                PlatformLocatorStrategy.GetElementContainingText("must", _platform)
            };
            
            foreach (var strategy in errorStrategies)
            {
                try
                {
                    var elements = _driver.FindElements(strategy);
                    if (elements.Count > 0)
                    {
                        var element = elements.First();
                        var textAttribute = PlatformLocatorStrategy.GetTextAttribute(_platform);
                        var errorText = element.GetAttribute(textAttribute) ?? string.Empty;
                        if (!string.IsNullOrEmpty(errorText))
                        {
                            return errorText;
                        }
                    }
                }
                catch (NoSuchElementException)
                {
                    continue; // Try next strategy
                }
            }
            
            return string.Empty;
        }
        catch (NoSuchElementException)
        {
            return string.Empty; // No validation error displayed
        }
    }

    /// <summary>
    /// Checks if the Save button is enabled using platform-appropriate attributes
    /// </summary>
    public bool IsSaveButtonEnabled()
    {
        try
        {
            var element = _driver.FindElement(SaveButtonBy);
            var enabledAttribute = PlatformLocatorStrategy.GetEnabledAttribute(_platform);
            var enabledValue = element.GetAttribute(enabledAttribute);
            return bool.TryParse(enabledValue, out bool result) && result;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a validation error is displayed using platform-appropriate detection
    /// </summary>
    public bool IsValidationErrorDisplayed()
    {
        try
        {
            var errorText = GetValidationError();
            return !string.IsNullOrEmpty(errorText);
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region Wait Methods

    /// <summary>
    /// Waits for the Settings page to be displayed
    /// Compatible with Selenium.WebDriver 4.12.4
    /// </summary>
    /// <param name="timeout">Maximum time to wait</param>
    public void WaitForPageToLoad(TimeSpan? timeout = null)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, timeout ?? TimeSpan.FromSeconds(10));
        wait.Until(driver => driver.FindElements(HeaderTitleBy).Count > 0 || driver.FindElements(PageTitleBy).Count > 0);
    }

    /// <summary>
    /// Waits for validation error to appear
    /// </summary>
    /// <param name="timeout">Maximum time to wait</param>
    public void WaitForValidationError(TimeSpan? timeout = null)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, timeout ?? TimeSpan.FromSeconds(5));
        wait.Until(driver => IsValidationErrorDisplayed());
    }

    /// <summary>
    /// Waits for validation error to disappear
    /// </summary>
    /// <param name="timeout">Maximum time to wait</param>
    public void WaitForValidationErrorToClear(TimeSpan? timeout = null)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, timeout ?? TimeSpan.FromSeconds(5));
        wait.Until(driver => !IsValidationErrorDisplayed());
    }

    #endregion
}