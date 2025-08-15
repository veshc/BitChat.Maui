using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;
using System.Drawing;
using Xunit;

namespace BitChat.Maui.Tests.UI.Shared;

/// <summary>
/// Screen orientation enumeration for mobile testing
/// </summary>
public enum ScreenOrientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Base class for Appium UI tests providing cross-platform driver setup
/// </summary>
public abstract class AppiumTestBase : IDisposable
{
    public AppiumDriver? Driver { get; private set; }
    public Platform CurrentPlatform { get; private set; }
    
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
    
    public enum Platform
    {
        iOS,
        Android
    }

    /// <summary>
    /// Initializes the Appium driver for the specified platform
    /// </summary>
    public void InitializeDriver(Platform platform)
    {
        CurrentPlatform = platform;
        
        var appiumOptions = new AppiumOptions();
        
        switch (platform)
        {
            case Platform.iOS:
                SetupiOSDriver(appiumOptions);
                break;
            case Platform.Android:
                SetupAndroidDriver(appiumOptions);
                break;
            default:
                throw new ArgumentException($"Unsupported platform: {platform}");
        }
    }
    
    private void SetupAndroidDriver(AppiumOptions options)
    {
        // Android configuration
        options.PlatformName = "Android";
        options.AutomationName = "UiAutomator2";
        options.DeviceName = "Android Emulator";
        options.App = GetAndroidAppPath();
        options.AddAdditionalAppiumOption("appPackage", "com.bitchat.maui");
        options.AddAdditionalAppiumOption("appActivity", "crc64b5e713d400f589b7.MainActivity");
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);
        options.AddAdditionalAppiumOption("noReset", false);
        options.AddAdditionalAppiumOption("fullReset", false);
        
        try
        {
            Driver = new AndroidDriver(new Uri("http://localhost:4723"), options, _defaultTimeout);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize Android driver: {ex.Message}", ex);
        }
    }
    
    private void SetupiOSDriver(AppiumOptions options)
    {
        // iOS configuration
        options.PlatformName = "iOS";
        options.AutomationName = "XCUITest";
        // Use UDID for existing simulator
        options.AddAdditionalAppiumOption("udid", "B275A02D-91BA-4D6F-94F6-FC49FB56ABC9");
        options.DeviceName = "iPhone 16 Pro";
        options.PlatformVersion = "18.6";
        // Try using bundle ID instead of app path for simulator
        // options.App = GetIOSAppPath();
        options.AddAdditionalAppiumOption("bundleId", "com.bitchat.maui");
        options.AddAdditionalAppiumOption("noReset", false);
        options.AddAdditionalAppiumOption("fullReset", false);
        options.AddAdditionalAppiumOption("newCommandTimeout", 30);
        options.AddAdditionalAppiumOption("wdaLaunchTimeout", 30000);
        options.AddAdditionalAppiumOption("wdaConnectionTimeout", 30000);
        
        try
        {
            Driver = new IOSDriver(new Uri("http://localhost:4723"), options, _defaultTimeout);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize iOS driver: {ex.Message}", ex);
        }
    }
    
    private string GetAndroidAppPath()
    {
        // Path to the Android APK
        var apkPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", 
            "bin", "Debug", "net8.0-android",
            "com.bitchat.maui-Signed.apk"
        );
        
        if (!File.Exists(apkPath))
        {
            throw new FileNotFoundException($"Android APK not found at: {apkPath}. Please build the Android app first.");
        }
        
        return Path.GetFullPath(apkPath);
    }
    
    private string GetIOSAppPath()
    {
        // Path to the iOS app
        var appPath = "/Users/vesh/Documents/src/aicoding/bitchatMaui/BitChat.Maui/bin/Debug/net8.0-ios/iossimulator-arm64/BitChat.Maui.app";
        
        if (!Directory.Exists(appPath))
        {
            throw new DirectoryNotFoundException($"iOS app not found at: {appPath}. Please build the iOS app first.");
        }
        
        return Path.GetFullPath(appPath);
    }
    
    /// <summary>
    /// Waits for an element to be present and visible
    /// </summary>
    public IWebElement WaitForElement(By locator, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(Driver!, timeout ?? _defaultTimeout);
        return wait.Until(driver => 
        {
            var element = driver.FindElement(locator);
            return element.Displayed ? element : null!;
        });
    }
    
    /// <summary>
    /// Waits for an element to be present and clickable
    /// </summary>
    public IWebElement WaitForClickableElement(By locator, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(Driver!, timeout ?? _defaultTimeout);
        return wait.Until(driver =>
        {
            var element = driver.FindElement(locator);
            return element.Displayed && element.Enabled ? element : null!;
        });
    }
    
    /// <summary>
    /// Waits for text to appear in an element
    /// </summary>
    public bool WaitForTextInElement(By locator, string expectedText, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(Driver!, timeout ?? _defaultTimeout);
        try
        {
            return wait.Until(driver =>
            {
                var element = driver.FindElement(locator);
                return element.Text.Contains(expectedText);
            });
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Takes a screenshot for debugging purposes
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
    /// Enters text into an element, clearing it first
    /// </summary>
    public void EnterText(By locator, string text)
    {
        var element = WaitForClickableElement(locator);
        element.Clear();
        element.SendKeys(text);
    }
    
    /// <summary>
    /// Taps/clicks an element
    /// </summary>
    public void TapElement(By locator)
    {
        var element = WaitForClickableElement(locator);
        element.Click();
    }
    
    /// <summary>
    /// Checks if an element exists without waiting
    /// </summary>
    public bool ElementExists(By locator)
    {
        try
        {
            Driver!.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets the text content of an element
    /// </summary>
    public string GetElementText(By locator)
    {
        var element = WaitForElement(locator);
        return element.Text;
    }

    /// <summary>
    /// Sets the device orientation
    /// </summary>
    public void SetOrientation(ScreenOrientation orientation)
    {
        if (Driver is AndroidDriver androidDriver)
        {
            var seleniumOrientation = orientation == ScreenOrientation.Portrait 
                ? OpenQA.Selenium.ScreenOrientation.Portrait 
                : OpenQA.Selenium.ScreenOrientation.Landscape;
            androidDriver.Orientation = seleniumOrientation;
        }
        else if (Driver is IOSDriver iosDriver)
        {
            var seleniumOrientation = orientation == ScreenOrientation.Portrait 
                ? OpenQA.Selenium.ScreenOrientation.Portrait 
                : OpenQA.Selenium.ScreenOrientation.Landscape;
            iosDriver.Orientation = seleniumOrientation;
        }
    }
    
    public virtual void Dispose()
    {
        Driver?.Quit();
        Driver?.Dispose();
    }
}

/// <summary>
/// WebDriverWait implementation for Appium
/// </summary>
public class WebDriverWait
{
    private readonly AppiumDriver _driver;
    private readonly TimeSpan _timeout;
    
    public WebDriverWait(AppiumDriver driver, TimeSpan timeout)
    {
        _driver = driver;
        _timeout = timeout;
    }
    
    public T Until<T>(Func<AppiumDriver, T> condition)
    {
        var endTime = DateTime.Now.Add(_timeout);
        
        while (DateTime.Now < endTime)
        {
            try
            {
                var result = condition(_driver);
                if (result != null && !result.Equals(default(T)))
                {
                    return result;
                }
            }
            catch (Exception ex) when (
                ex is NoSuchElementException || 
                ex is StaleElementReferenceException ||
                ex is ElementNotVisibleException)
            {
                // These exceptions are expected during waiting
            }
            
            Thread.Sleep(500); // Wait 500ms before retrying
        }
        
        throw new WebDriverTimeoutException($"Condition was not met within {_timeout}");
    }
}

/// <summary>
/// Custom exception for WebDriver timeout scenarios
/// </summary>
public class WebDriverTimeoutException : Exception
{
    public WebDriverTimeoutException(string message) : base(message) { }
    public WebDriverTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}