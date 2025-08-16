using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Support.UI;

namespace BitChat.Maui.Tests.UI.Shared;

/// <summary>
/// Screen orientation enumeration for mobile testing (legacy compatibility)
/// </summary>
public enum ScreenOrientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Enhanced base class for Appium UI tests with cross-platform support and smart wait strategies
/// Configured for stable Appium.WebDriver 5.0.0 + Selenium.WebDriver 4.x combination
/// Provides robust session management, platform-aware locators, and retry mechanisms
/// </summary>
public abstract class AppiumTestBase : IDisposable
{
    protected AppiumDriver? Driver { get; private set; }
    protected TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(30);
    public abstract Platform CurrentPlatform { get; }
    
    // Legacy compatibility properties
    public Platform Platform => CurrentPlatform;
    
    private bool _disposed;
    private static readonly SemaphoreSlim _sessionSemaphore = new(1, 1);
    private static readonly object _lockObject = new();

    /// <summary>
    /// Initializes the Appium driver for iOS testing
    /// Uses AppiumOptions with W3C capability format for Appium 2.x compatibility
    /// </summary>
    protected void InitializeIOSDriver()
    {
        var appiumOptions = new AppiumOptions();
        
        // Set standard W3C capabilities using built-in properties
        appiumOptions.PlatformName = "iOS";
        appiumOptions.PlatformVersion = "18.6";
        appiumOptions.DeviceName = "iPhone 16 Pro";
        appiumOptions.AutomationName = "XCUITest";
        appiumOptions.App = GetIOSAppPath();
        
        // Set additional Appium-specific options
        appiumOptions.AddAdditionalAppiumOption("newCommandTimeout", 300);
        appiumOptions.AddAdditionalAppiumOption("noReset", false);
        
        Driver = new IOSDriver(new Uri("http://127.0.0.1:4723"), appiumOptions, DefaultTimeout);
    }

    /// <summary>
    /// Initializes the Appium driver for Android testing
    /// Uses AppiumOptions with W3C capability format for Appium 2.x compatibility
    /// </summary>
    protected void InitializeAndroidDriver()
    {
        var appiumOptions = new AppiumOptions();
        
        // Set standard W3C capabilities using built-in properties
        appiumOptions.PlatformName = "Android";
        appiumOptions.PlatformVersion = "14.0";
        appiumOptions.DeviceName = "Android Emulator";
        appiumOptions.AutomationName = "UiAutomator2";
        appiumOptions.App = GetAndroidAppPath();
        
        // Set Android-specific capabilities for MAUI app
        appiumOptions.AddAdditionalAppiumOption("appPackage", "com.bitchat.maui");
        appiumOptions.AddAdditionalAppiumOption("appActivity", "crc6484f8585808e33e7c.MainActivity");
        appiumOptions.AddAdditionalAppiumOption("newCommandTimeout", 300);
        appiumOptions.AddAdditionalAppiumOption("noReset", false);
        appiumOptions.AddAdditionalAppiumOption("autoGrantPermissions", true);
        appiumOptions.AddAdditionalAppiumOption("ensureWebviewsHavePages", true);
        appiumOptions.AddAdditionalAppiumOption("nativeWebScreenshot", true);
        
        Driver = new AndroidDriver(new Uri("http://127.0.0.1:4723"), appiumOptions, DefaultTimeout);
    }
    /// <summary>
    /// Gets the path to the iOS app bundle for testing
    /// </summary>
    /// <returns>Path to the iOS app</returns>
    private string GetIOSAppPath()
    {
        // Use a direct path calculation - Tests is a subdirectory of the main BitChat.Maui project
        var testProjectPath = Path.GetDirectoryName(typeof(AppiumTestBase).Assembly.Location) ?? "";
        var solutionRoot = Path.Combine(testProjectPath, "..", "..", "..", "..", "..");
        var bitChatMauiProjectPath = Path.Combine(solutionRoot, "BitChat.Maui");
        
        // Search for the iOS app in known build locations
        var possiblePaths = new[]
        {
            Path.Combine(bitChatMauiProjectPath, "bin", "Debug", "net8.0-ios", "iossimulator-arm64", "BitChat.Maui.app"),
            Path.Combine(bitChatMauiProjectPath, "bin", "iPhoneSimulator", "Debug", "net8.0-ios", "iossimulator-arm64", "BitChat.Maui.app"),
            Path.Combine(bitChatMauiProjectPath, "obj", "Debug", "net8.0-ios", "iossimulator-arm64", "codesign", "bin", "Debug", "net8.0-ios", "iossimulator-arm64", "BitChat.Maui.app")
        };
        
        foreach (var possiblePath in possiblePaths)
        {
            var resolvedPath = Path.GetFullPath(possiblePath);
            Console.WriteLine($"Checking iOS app path: {resolvedPath}");
            if (Directory.Exists(resolvedPath))
            {
                Console.WriteLine($"Found iOS app at: {resolvedPath}");
                return resolvedPath;
            }
        }
        
        // If no app found, return the first path for debugging  
        var fallbackPath = Path.GetFullPath(possiblePaths[0]);
        Console.WriteLine($"iOS App not found, using fallback path: {fallbackPath}");
        Console.WriteLine($"Test project path: {testProjectPath}");
        Console.WriteLine($"BitChat.Maui project path: {Path.GetFullPath(bitChatMauiProjectPath)}");
        return fallbackPath;
    }

    /// <summary>
    /// Gets the path to the Android APK for testing
    /// </summary>
    /// <returns>Path to the Android APK</returns>
    private string GetAndroidAppPath()
    {
        // Navigate to the parent BitChat.Maui project directory (one level up from Tests)
        var testProjectPath = Path.GetDirectoryName(typeof(AppiumTestBase).Assembly.Location) ?? "";
        var solutionRoot = Path.Combine(testProjectPath, "..", "..", "..", "..", "..");
        var bitChatMauiProjectPath = Path.Combine(solutionRoot, "BitChat.Maui");
        
        // Search for the Android APK in known build locations
        var possiblePaths = new[]
        {
            Path.Combine(bitChatMauiProjectPath, "bin", "Debug", "net8.0-android", "com.bitchat.maui-Signed.apk"),
            Path.Combine(bitChatMauiProjectPath, "bin", "Debug", "net8.0-android", "com.bitchat.maui.apk"),
            Path.Combine(bitChatMauiProjectPath, "obj", "Debug", "net8.0-android", "android", "bin", "com.bitchat.maui.apk")
        };
        
        foreach (var possiblePath in possiblePaths)
        {
            var resolvedPath = Path.GetFullPath(possiblePath);
            Console.WriteLine($"Checking Android APK path: {resolvedPath}");
            if (File.Exists(resolvedPath))
            {
                Console.WriteLine($"Found Android APK at: {resolvedPath}");
                return resolvedPath;
            }
        }
        
        // If no APK found, return the first path for debugging  
        var fallbackPath = Path.GetFullPath(possiblePaths[0]);
        Console.WriteLine($"Android APK not found, using fallback path: {fallbackPath}");
        Console.WriteLine($"BitChat.Maui project path: {Path.GetFullPath(bitChatMauiProjectPath)}");
        return fallbackPath;
    }
    /// <summary>
    /// Smart wait for an element that is present, visible, and enabled
    /// Uses exponential backoff for resilience against flaky elements
    /// </summary>
    /// <param name="by">The locator strategy</param>
    /// <param name="timeout">Optional timeout (uses default if not specified)</param>
    /// <returns>The found element</returns>
    protected async Task<IWebElement> WaitForElementAsync(By by, TimeSpan? timeout = null)
    {
        if (Driver == null)
            throw new InvalidOperationException("Driver not initialized. Call InitializeIOSDriver() or InitializeAndroidDriver() first.");

        return await ExecuteWithRetryAsync(async () =>
        {
            var wait = new WebDriverWait(Driver, timeout ?? DefaultTimeout);
            var element = wait.Until(driver =>
            {
                try
                {
                    var elem = driver.FindElement(by);
                    // Only require element to be displayed (not necessarily enabled)
                    // Buttons may be disabled initially but still present and testable
                    return elem.Displayed ? elem : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null; // Element went stale, retry
                }
                catch (NoSuchElementException)
                {
                    return null; // Element not found yet, keep waiting
                }
            })!; // We know this won't be null due to WebDriverWait behavior
            return await Task.FromResult(element);
        }, maxAttempts: 3);
    }

    /// <summary>
    /// Synchronous version of WaitForElement for backward compatibility
    /// </summary>
    public IWebElement WaitForElement(By by, TimeSpan? timeout = null)
    {
        return WaitForElementAsync(by, timeout).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Waits for an element to be clickable and clicks it with retry logic
    /// </summary>
    /// <param name="by">The locator strategy</param>
    /// <param name="timeout">Optional timeout</param>
    protected async Task WaitAndClickAsync(By by, TimeSpan? timeout = null)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var element = await WaitForElementAsync(by, timeout);
            element.Click();
            return true; // Success
        }, maxAttempts: 3);
    }

    /// <summary>
    /// Synchronous version of WaitAndClick for backward compatibility
    /// </summary>
    protected void WaitAndClick(By by, TimeSpan? timeout = null)
    {
        WaitAndClickAsync(by, timeout).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Waits for an element and enters text into it with retry and validation
    /// </summary>
    /// <param name="by">The locator strategy</param>
    /// <param name="text">The text to enter</param>
    /// <param name="timeout">Optional timeout</param>
    protected async Task WaitAndSendKeysAsync(By by, string text, TimeSpan? timeout = null)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var element = await WaitForElementAsync(by, timeout);
            element.Clear();
            
            // Add small delay to ensure clear operation completed
            await Task.Delay(100);
            
            element.SendKeys(text);
            
            // Verify text was entered correctly
            var textAttribute = PlatformLocatorStrategy.GetTextAttribute(CurrentPlatform);
            var actualText = element.GetAttribute(textAttribute) ?? string.Empty;
            
            if (actualText != text)
            {
                throw new InvalidOperationException($"Text entry failed. Expected: '{text}', Actual: '{actualText}'");
            }
            
            return true; // Success
        }, maxAttempts: 3);
    }

    /// <summary>
    /// Synchronous version of WaitAndSendKeys for backward compatibility
    /// </summary>
    protected void WaitAndSendKeys(By by, string text, TimeSpan? timeout = null)
    {
        WaitAndSendKeysAsync(by, text, timeout).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Checks if an element is present on the screen
    /// </summary>
    /// <param name="by">The locator strategy</param>
    /// <returns>True if element is present, false otherwise</returns>
    protected bool IsElementPresent(By by)
    {
        if (Driver == null)
            return false;

        try
        {
            Driver.FindElement(by);
            return true;
        }
        catch (OpenQA.Selenium.NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Executes an operation with exponential backoff retry logic
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">The operation to execute</param>
    /// <param name="maxAttempts">Maximum number of attempts</param>
    /// <returns>Result of the operation</returns>
    protected async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxAttempts = 3)
    {
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxAttempts && IsRetryableException(ex))
            {
                lastException = ex;
                var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100); // Exponential backoff
                await Task.Delay(delay);
            }
        }
        
        throw new Exception($"Operation failed after {maxAttempts} attempts", lastException);
    }

    /// <summary>
    /// Determines if an exception is retryable
    /// </summary>
    private static bool IsRetryableException(Exception ex)
    {
        return ex is StaleElementReferenceException ||
               ex is NoSuchElementException ||
               ex is ElementNotVisibleException ||
               ex is ElementNotInteractableException ||
               ex is WebDriverTimeoutException ||
               (ex is InvalidOperationException && ex.Message.Contains("Text entry failed"));
    }

    /// <summary>
    /// Creates platform-aware locators using the cross-platform strategy
    /// </summary>
    /// <param name="text">Text to find</param>
    /// <returns>Platform-appropriate locator</returns>
    protected By ByText(string text)
    {
        return PlatformLocatorStrategy.GetElementByText(text, CurrentPlatform);
    }

    /// <summary>
    /// Creates platform-aware locators for text containing a value
    /// </summary>
    /// <param name="containsText">Text that should be contained</param>
    /// <returns>Platform-appropriate locator</returns>
    protected By ByTextContains(string containsText)
    {
        return PlatformLocatorStrategy.GetElementContainingText(containsText, CurrentPlatform);
    }

    /// <summary>
    /// Gets text content from an element using platform-appropriate attributes
    /// </summary>
    /// <param name="element">The element to get text from</param>
    /// <returns>The text content</returns>
    protected string GetElementText(IWebElement element)
    {
        var textAttribute = PlatformLocatorStrategy.GetTextAttribute(CurrentPlatform);
        return element.GetAttribute(textAttribute) ?? string.Empty;
    }

    /// <summary>
    /// Checks if an element is enabled using platform-appropriate attributes
    /// </summary>
    /// <param name="element">The element to check</param>
    /// <returns>True if enabled, false otherwise</returns>
    protected bool IsElementEnabled(IWebElement element)
    {
        var enabledAttribute = PlatformLocatorStrategy.GetEnabledAttribute(CurrentPlatform);
        var enabledValue = element.GetAttribute(enabledAttribute);
        return bool.TryParse(enabledValue, out bool result) && result;
    }

    /// <summary>
    /// Resets the app to a clean state for test isolation
    /// </summary>
    protected virtual async Task ResetAppStateAsync()
    {
        if (Driver != null)
        {
            // Clear any cached data - use terminate and launch instead of ResetApp
            await Task.Run(() => {
                try 
                {
                    Driver.TerminateApp("com.bitchat.maui");
                    Driver.ActivateApp("com.bitchat.maui");
                }
                catch (Exception ex)
                {
                    // Fallback to generic reset if available
                    Console.WriteLine($"Could not reset app: {ex.Message}");
                }
            });
            
            // Add small delay for app to fully reset
            await Task.Delay(1000);
        }
    }

    // Legacy method compatibility for existing tests
    
    /// <summary>
    /// Checks if an element exists without waiting (legacy compatibility)
    /// </summary>
    public bool ElementExists(By by)
    {
        return IsElementPresent(by);
    }

    /// <summary>
    /// Takes a screenshot for debugging purposes (legacy compatibility)
    /// </summary>
    public void TakeScreenshot(string testName)
    {
        if (Driver is ITakesScreenshot screenshotDriver)
        {
            var screenshot = screenshotDriver.GetScreenshot();
            var filename = $"{testName}_{CurrentPlatform}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var path = Path.Combine("Screenshots", filename);
            
            Directory.CreateDirectory("Screenshots");
            screenshot.SaveAsFile(path);
        }
    }

    /// <summary>
    /// Enters text into an element, clearing it first (legacy compatibility)
    /// </summary>
    public void EnterText(By by, string text)
    {
        WaitAndSendKeys(by, text);
    }

    /// <summary>
    /// Taps/clicks an element (legacy compatibility)
    /// </summary>
    public void TapElement(By by)
    {
        WaitAndClick(by);
    }

    /// <summary>
    /// Gets the text content of an element (legacy compatibility)
    /// </summary>
    public string GetElementText(By by)
    {
        var element = WaitForElement(by);
        return GetElementText(element);
    }

    /// <summary>
    /// Waits for text to appear in an element (legacy compatibility)
    /// </summary>
    public bool WaitForTextInElement(By by, string expectedText, TimeSpan? timeout = null)
    {
        try
        {
            var wait = new WebDriverWait(Driver!, timeout ?? DefaultTimeout);
            return wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    var textAttribute = PlatformLocatorStrategy.GetTextAttribute(CurrentPlatform);
                    var actualText = element.GetAttribute(textAttribute) ?? element.Text;
                    return actualText.Contains(expectedText);
                }
                catch
                {
                    return false;
                }
            });
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Sets the device orientation (legacy compatibility)
    /// </summary>
    public void SetOrientation(ScreenOrientation orientation)
    {
        var seleniumOrientation = orientation == ScreenOrientation.Portrait 
            ? OpenQA.Selenium.ScreenOrientation.Portrait 
            : OpenQA.Selenium.ScreenOrientation.Landscape;
            
        if (Driver != null)
        {
            try
            {
                ((IRotatable)Driver).Orientation = seleniumOrientation;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not set orientation: {ex.Message}");
            }
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async Task DisposeAsyncCore()
    {
        if (!_disposed)
        {
            // Clean up any test data first
            try
            {
                await CleanupTestDataAsync();
            }
            catch (Exception ex)
            {
                // Log but don't fail disposal
                Console.WriteLine($"Warning: Failed to cleanup test data during disposal: {ex.Message}");
            }

            // Dispose driver safely
            if (Driver != null)
            {
                try
                {
                    Driver.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to quit driver gracefully: {ex.Message}");
                }
                
                try
                {
                    Driver.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to dispose driver: {ex.Message}");
                }
                
                Driver = null;
            }
            
            _disposed = true;
        }
    }

    /// <summary>
    /// Override this method to provide custom test data cleanup
    /// </summary>
    protected virtual async Task CleanupTestDataAsync()
    {
        // Default implementation - no cleanup needed
        await Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    ~AppiumTestBase()
    {
        Dispose();
    }
}