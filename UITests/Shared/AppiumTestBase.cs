using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Support.UI;

namespace BitChat.Maui.UITests.Shared;

/// <summary>
/// Enhanced base class for Appium UI tests with cross-platform support and smart wait strategies
/// Configured for stable Appium.WebDriver 4.4.0 + Selenium.WebDriver 3.141.0 combination
/// Provides robust session management, platform-aware locators, and retry mechanisms
/// </summary>
public abstract class AppiumTestBase : IDisposable
{
    protected AppiumDriver<IWebElement>? Driver { get; private set; }
    protected TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(30);
    protected abstract Platform CurrentPlatform { get; }
    
    private bool _disposed;
    private static readonly SemaphoreSlim _sessionSemaphore = new(1, 1);
    private static readonly object _lockObject = new();

    /// <summary>
    /// Initializes the Appium driver for iOS testing
    /// Uses AppiumOptions with both legacy and W3C capability formats
    /// </summary>
    protected void InitializeIOSDriver()
    {
        var appiumOptions = new AppiumOptions();
        
        // Add to legacy desiredCapabilities format
        appiumOptions.AddAdditionalCapability("platformName", "iOS");
        appiumOptions.AddAdditionalCapability("platformVersion", "18.6");
        appiumOptions.AddAdditionalCapability("deviceName", "iPhone 16 Pro");
        appiumOptions.AddAdditionalCapability("automationName", "XCUITest");
        appiumOptions.AddAdditionalCapability("app", GetIOSAppPath());
        appiumOptions.AddAdditionalCapability("newCommandTimeout", 300);
        appiumOptions.AddAdditionalCapability("noReset", false);
        
        // CRITICAL: Also set W3C capabilities for modern Appium compatibility
        appiumOptions.PlatformName = "iOS";
        appiumOptions.AddAdditionalCapability("appium:platformVersion", "18.6");  // Set W3C platformVersion explicitly
        appiumOptions.AddAdditionalCapability("appium:automationName", "XCUITest");
        appiumOptions.AddAdditionalCapability("appium:deviceName", "iPhone 16 Pro");
        appiumOptions.AddAdditionalCapability("appium:app", GetIOSAppPath());
        
        Driver = new IOSDriver<IWebElement>(new Uri("http://127.0.0.1:4723"), appiumOptions, DefaultTimeout);
    }

    /// <summary>
    /// Initializes the Appium driver for Android testing
    /// Uses AppiumOptions with both legacy and W3C capability formats
    /// </summary>
    protected void InitializeAndroidDriver()
    {
        var appiumOptions = new AppiumOptions();
        
        // Add to legacy desiredCapabilities format
        appiumOptions.AddAdditionalCapability("platformName", "Android");
        appiumOptions.AddAdditionalCapability("platformVersion", "14.0");
        appiumOptions.AddAdditionalCapability("deviceName", "Android Emulator");
        appiumOptions.AddAdditionalCapability("automationName", "UiAutomator2");
        appiumOptions.AddAdditionalCapability("app", GetAndroidAppPath());
        appiumOptions.AddAdditionalCapability("newCommandTimeout", 300);
        appiumOptions.AddAdditionalCapability("noReset", false);
        
        // CRITICAL: Also set W3C capabilities for modern Appium compatibility
        appiumOptions.PlatformName = "Android";
        appiumOptions.AddAdditionalCapability("appium:platformVersion", "14.0");  // Set W3C platformVersion explicitly
        appiumOptions.AddAdditionalCapability("appium:automationName", "UiAutomator2");
        appiumOptions.AddAdditionalCapability("appium:deviceName", "Android Emulator");
        appiumOptions.AddAdditionalCapability("appium:app", GetAndroidAppPath());
        
        Driver = new AndroidDriver<IWebElement>(new Uri("http://127.0.0.1:4723"), appiumOptions, DefaultTimeout);
    }

    /// <summary>
    /// Gets the path to the iOS app bundle for testing
    /// </summary>
    /// <returns>Path to the iOS app</returns>
    private string GetIOSAppPath()
    {
        // This should point to the built iOS app
        var basePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "";
        var projectRoot = Path.Combine(basePath, "..", "..", "..", "..");
        var appPath = Path.Combine(projectRoot, "bin", "Debug", "net8.0-ios", "iossimulator-arm64", "BitChat.Maui.app");
        
        // If the specific architecture path doesn't exist, try the general iOS path
        if (!Directory.Exists(appPath))
        {
            appPath = Path.Combine(projectRoot, "bin", "Debug", "net8.0-ios", "BitChat.Maui.app");
        }
        
        return Path.GetFullPath(appPath);
    }

    /// <summary>
    /// Gets the path to the Android APK for testing
    /// </summary>
    /// <returns>Path to the Android APK</returns>
    private string GetAndroidAppPath()
    {
        // This should point to the built Android APK
        var basePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "";
        var projectRoot = Path.Combine(basePath, "..", "..", "..", "..");
        var apkPath = Path.Combine(projectRoot, "bin", "Debug", "net8.0-android", "com.bitchat.maui-Signed.apk");
        
        // If the signed APK doesn't exist, try the unsigned APK
        if (!File.Exists(apkPath))
        {
            apkPath = Path.Combine(projectRoot, "bin", "Debug", "net8.0-android", "com.bitchat.maui.apk");
        }
        
        return Path.GetFullPath(apkPath);
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
                    return elem.Displayed && elem.Enabled ? elem : null;
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
    protected IWebElement WaitForElement(By by, TimeSpan? timeout = null)
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
            // Clear any cached data
            await Task.Run(() => Driver.ResetApp());
            
            // Add small delay for app to fully reset
            await Task.Delay(1000);
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