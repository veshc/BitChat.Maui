using BitChat.Maui.UITests.Shared;
using OpenQA.Selenium;
using System.Text;
using Xunit;

namespace BitChat.Maui.UITests;

/// <summary>
/// Minimal diagnostic test to validate environment and element accessibility
/// </summary>
public class DiagnosticTest : AppiumTestBase
{
    protected override Shared.Platform CurrentPlatform => Shared.Platform.iOS; // Default to iOS for diagnostics
    [Fact]
    [Trait("Category", "Diagnostic")]
    [Trait("Platform", "iOS")]
    public void iOS_DiagnosticTest_AppLaunchAndElementInspection()
    {
        // Initialize iOS driver
        InitializeIOSDriver();
        
        // Give app time to launch
        System.Threading.Thread.Sleep(5000);
        
        // Log current page source for analysis
        var pageSource = Driver?.PageSource ?? "NO PAGE SOURCE";
        Console.WriteLine("=== FULL PAGE SOURCE ===");
        Console.WriteLine(pageSource);
        Console.WriteLine("=== END PAGE SOURCE ===");
        
        // Try to find elements using different strategies
        var diagnosticResults = new StringBuilder();
        diagnosticResults.AppendLine("=== ELEMENT ACCESSIBILITY DIAGNOSTIC ===");
        
        // Test 1: Find by ID (should work with x:Name)
        TestElementAccess(diagnosticResults, "SettingsBtn", By.Id("SettingsBtn"));
        TestElementAccess(diagnosticResults, "SettingsButton", By.Id("SettingsButton"));
        
        // Test 2: Find by XPath text (may not work with MAUI)
        TestElementAccess(diagnosticResults, "BitChat text", By.XPath("//*[@text='BitChat']"));
        TestElementAccess(diagnosticResults, "Gear icon text", By.XPath("//*[@text='⚙️']"));
        TestElementAccess(diagnosticResults, "Configure Settings text", By.XPath("//*[@text='Configure Settings']"));
        
        // Test 3: Find by XPath with accessible attributes
        TestElementAccess(diagnosticResults, "BitChat accessible", By.XPath("//*[@name='BitChat']"));
        TestElementAccess(diagnosticResults, "BitChat label", By.XPath("//*[@label='BitChat']"));
        
        // Test 4: Find using accessibility attributes
        TestElementAccess(diagnosticResults, "Settings accessibility", By.XPath("//*[contains(@name, 'Settings')]"));
        TestElementAccess(diagnosticResults, "Settings hint", By.XPath("//*[contains(@hint, 'settings')]"));
        
        // Test 5: Test platform-aware locator strategy
        try
        {
            var platformAwareLocator = PlatformLocatorStrategy.GetElementByText("BitChat", CurrentPlatform);
            TestElementAccess(diagnosticResults, "BitChat (platform-aware)", platformAwareLocator);
            
            var settingsLocator = PlatformLocatorStrategy.GetElementByText("Configure Settings", CurrentPlatform);
            TestElementAccess(diagnosticResults, "Configure Settings (platform-aware)", settingsLocator);
        }
        catch (Exception ex)
        {
            diagnosticResults.AppendLine($"⚠️ Platform-aware locator test failed: {ex.Message}");
        }
        
        Console.WriteLine(diagnosticResults.ToString());
        
        // Basic assertion: Driver should be initialized
        Assert.NotNull(Driver);
        
        // Log test completion
        Console.WriteLine("=== DIAGNOSTIC TEST COMPLETED ===");
    }
    
    [Fact]
    [Trait("Category", "Diagnostic")]
    [Trait("Platform", "Android")]
    public void Android_DiagnosticTest_AppLaunchAndElementInspection()
    {
        // Initialize Android driver
        InitializeAndroidDriver();
        
        // Give app time to launch
        System.Threading.Thread.Sleep(5000);
        
        // Log current page source for analysis
        var pageSource = Driver?.PageSource ?? "NO PAGE SOURCE";
        Console.WriteLine("=== FULL PAGE SOURCE ===");
        Console.WriteLine(pageSource);
        Console.WriteLine("=== END PAGE SOURCE ===");
        
        // Basic assertion: Driver should be initialized
        Assert.NotNull(Driver);
        
        Console.WriteLine("=== ANDROID DIAGNOSTIC TEST COMPLETED ===");
    }
    
    private void TestElementAccess(StringBuilder results, string description, By locator)
    {
        try
        {
            var elements = Driver?.FindElements(locator);
            var count = elements?.Count ?? 0;
            
            if (count > 0)
            {
                var element = elements![0];
                var textAttribute = PlatformLocatorStrategy.GetTextAttribute(CurrentPlatform);
                var text = element.GetAttribute(textAttribute) ?? "NO_TEXT";
                var name = element.GetAttribute("name") ?? "NO_NAME";
                var label = element.GetAttribute("label") ?? "NO_LABEL";
                var enabled = element.GetAttribute("enabled") ?? "NO_ENABLED";
                var visible = element.GetAttribute("visible") ?? "NO_VISIBLE";
                
                results.AppendLine($"✅ {description}: FOUND ({count} elements)");
                results.AppendLine($"   - text: '{text}'");
                results.AppendLine($"   - name: '{name}'");
                results.AppendLine($"   - label: '{label}'");
                results.AppendLine($"   - enabled: '{enabled}'");
                results.AppendLine($"   - visible: '{visible}'");
            }
            else
            {
                results.AppendLine($"❌ {description}: NOT FOUND");
            }
        }
        catch (Exception ex)
        {
            results.AppendLine($"⚠️ {description}: ERROR - {ex.Message}");
        }
    }
}