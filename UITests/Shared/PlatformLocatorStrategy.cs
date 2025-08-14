using OpenQA.Selenium;

namespace BitChat.Maui.UITests.Shared;

/// <summary>
/// Cross-platform locator strategy that creates appropriate locators for different platforms
/// Handles the differences between iOS (name/label/value) and Android (text/content-desc) attributes
/// </summary>
public static class PlatformLocatorStrategy
{
    /// <summary>
    /// Creates a locator that finds elements by visible text across platforms
    /// </summary>
    /// <param name="text">The text to search for</param>
    /// <param name="platform">The target platform</param>
    /// <returns>Platform-appropriate By locator</returns>
    public static By GetElementByText(string text, Platform platform)
    {
        return platform switch
        {
            Platform.iOS => By.XPath($"//*[@name='{text}' or @label='{text}' or @value='{text}']"),
            Platform.Android => By.XPath($"//*[@text='{text}' or @content-desc='{text}']"),
            Platform.Windows => By.XPath($"//*[@Name='{text}' or @AutomationId='{text}']"),
            Platform.macOS => By.XPath($"//*[@name='{text}' or @label='{text}' or @value='{text}']"),
            _ => throw new NotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// Creates a locator for text containing a specific value (partial match)
    /// </summary>
    /// <param name="containsText">The text that should be contained</param>
    /// <param name="platform">The target platform</param>
    /// <returns>Platform-appropriate By locator</returns>
    public static By GetElementContainingText(string containsText, Platform platform)
    {
        return platform switch
        {
            Platform.iOS => By.XPath($"//*[contains(@name, '{containsText}') or contains(@label, '{containsText}') or contains(@value, '{containsText}')]"),
            Platform.Android => By.XPath($"//*[contains(@text, '{containsText}') or contains(@content-desc, '{containsText}')]"),
            Platform.Windows => By.XPath($"//*[contains(@Name, '{containsText}') or contains(@AutomationId, '{containsText}')]"),
            Platform.macOS => By.XPath($"//*[contains(@name, '{containsText}') or contains(@label, '{containsText}') or contains(@value, '{containsText}')]"),
            _ => throw new NotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// Creates a locator for elements by ID (works across all platforms)
    /// </summary>
    /// <param name="id">The element ID</param>
    /// <param name="platform">The target platform (for consistency, though ID is universal)</param>
    /// <returns>By.Id locator</returns>
    public static By GetElementById(string id, Platform platform)
    {
        // ID locators work the same way across platforms
        return By.Id(id);
    }

    /// <summary>
    /// Creates a platform-specific locator for button elements
    /// </summary>
    /// <param name="buttonText">The button text</param>
    /// <param name="platform">The target platform</param>
    /// <returns>Platform-appropriate button locator</returns>
    public static By GetButtonByText(string buttonText, Platform platform)
    {
        return platform switch
        {
            Platform.iOS => By.XPath($"//XCUIElementTypeButton[@name='{buttonText}' or @label='{buttonText}']"),
            Platform.Android => By.XPath($"//android.widget.Button[@text='{buttonText}' or @content-desc='{buttonText}']"),
            Platform.Windows => By.XPath($"//Button[@Name='{buttonText}' or @AutomationId='{buttonText}']"),
            Platform.macOS => By.XPath($"//XCUIElementTypeButton[@name='{buttonText}' or @label='{buttonText}']"),
            _ => throw new NotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// Creates a platform-specific locator for input/text field elements
    /// </summary>
    /// <param name="placeholder">The placeholder or label text</param>
    /// <param name="platform">The target platform</param>
    /// <returns>Platform-appropriate input field locator</returns>
    public static By GetTextFieldByPlaceholder(string placeholder, Platform platform)
    {
        return platform switch
        {
            Platform.iOS => By.XPath($"//XCUIElementTypeTextField[@name='{placeholder}' or @label='{placeholder}' or @placeholder='{placeholder}']"),
            Platform.Android => By.XPath($"//android.widget.EditText[@hint='{placeholder}' or @content-desc='{placeholder}']"),
            Platform.Windows => By.XPath($"//TextBox[@Name='{placeholder}' or @AutomationId='{placeholder}']"),
            Platform.macOS => By.XPath($"//XCUIElementTypeTextField[@name='{placeholder}' or @label='{placeholder}' or @placeholder='{placeholder}']"),
            _ => throw new NotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// Gets the appropriate attribute name for retrieving text content from elements
    /// </summary>
    /// <param name="platform">The target platform</param>
    /// <returns>The attribute name to use for getting text content</returns>
    public static string GetTextAttribute(Platform platform)
    {
        return platform switch
        {
            Platform.iOS => "value", // iOS primarily uses 'value' for text content
            Platform.Android => "text", // Android uses 'text' attribute
            Platform.Windows => "Name", // Windows uses 'Name' for text content
            Platform.macOS => "value", // macOS similar to iOS
            _ => throw new NotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// Gets the appropriate attribute name for checking if an element is enabled
    /// </summary>
    /// <param name="platform">The target platform</param>
    /// <returns>The attribute name to use for checking enabled state</returns>
    public static string GetEnabledAttribute(Platform platform)
    {
        return platform switch
        {
            Platform.iOS => "enabled",
            Platform.Android => "enabled", 
            Platform.Windows => "IsEnabled",
            Platform.macOS => "enabled",
            _ => throw new NotSupportedException($"Platform {platform} is not supported")
        };
    }
}