using BitChat.Maui.Tests.UI.Shared;
using OpenQA.Selenium;
using Xunit;

namespace BitChat.Maui.Tests.UI;

/// <summary>
/// Android app behavior documentation
/// </summary>
public class AndroidBehaviorDocumentation : AppiumTestBase
{
    public override Shared.Platform CurrentPlatform => Shared.Platform.Android;

    public AndroidBehaviorDocumentation()
    {
        InitializeAndroidDriver();
    }

    [Fact]
    [Trait("Category", "Documentation")]
    public void Document_Android_ActualBehavior()
    {
        try
        {
            Console.WriteLine("=== ANDROID APP BEHAVIOR DOCUMENTATION ===");
            
            // Give app time to fully load - Android may need longer
            Console.WriteLine("Waiting for BitChat app to launch...");
            Thread.Sleep(10000);
            
            // Check if we're still on the launcher
            var initialPageSource = Driver?.PageSource ?? "";
            if (initialPageSource.Contains("nexuslauncher"))
            {
                Console.WriteLine("⚠️ Still on Android launcher, attempting to launch BitChat app...");
                
                // Try to activate the BitChat app directly
                try
                {
                    Driver?.ActivateApp("com.bitchat.maui");
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to activate app: {ex.Message}");
                }
            }
            
            // 1. Document final app state
            Console.WriteLine("\n1. FINAL APP STATE:");
            var finalPageSource = Driver?.PageSource ?? "";
            Console.WriteLine($"Page source length: {finalPageSource.Length} characters");
            
            // Analyze what app we're connected to
            if (finalPageSource.Contains("nexuslauncher"))
            {
                Console.WriteLine("❌ STILL ON ANDROID LAUNCHER");
                Console.WriteLine("The BitChat app did not launch properly");
            }
            else if (finalPageSource.Contains("com.bitchat.maui"))
            {
                Console.WriteLine("✅ SUCCESSFULLY CONNECTED TO BITCHAT APP");
            }
            else
            {
                Console.WriteLine("⚠️ CONNECTED TO UNKNOWN APP");
            }
            
            // Save page source to file for analysis
            File.WriteAllText("android_initial_state.xml", finalPageSource);
            Console.WriteLine("✅ Saved final page source to android_initial_state.xml");
            
            TakeScreenshot("Android_Final_State");
            Console.WriteLine("✅ Saved final screenshot");
            
            // 2. Try to find elements using Android locators
            Console.WriteLine("\n2. ANDROID ELEMENT SEARCH:");
            
            // Try different Android locator strategies
            var strategies = new[]
            {
                ("ID com.bitchat.maui:id/messageInput", By.Id("com.bitchat.maui:id/messageInput")),
                ("ID messageInput", By.Id("messageInput")),
                ("Class EditText", By.ClassName("android.widget.EditText")),
                ("Class Button", By.ClassName("android.widget.Button")),
                ("XPath //android.widget.EditText", By.XPath("//android.widget.EditText")),
                ("XPath //android.widget.Button", By.XPath("//android.widget.Button"))
            };
            
            foreach (var (description, locator) in strategies)
            {
                try
                {
                    var elements = Driver?.FindElements(locator);
                    Console.WriteLine($"✅ {description}: Found {elements?.Count ?? 0} elements");
                    
                    if (elements != null && elements.Count > 0)
                    {
                        for (int i = 0; i < Math.Min(2, elements.Count); i++)
                        {
                            var element = elements[i];
                            Console.WriteLine($"  Element {i + 1}:");
                            Console.WriteLine($"    - Text: '{element.Text}'");
                            Console.WriteLine($"    - Enabled: {element.Enabled}");
                            Console.WriteLine($"    - Class: {element.GetAttribute("class")}");
                            Console.WriteLine($"    - Resource-ID: {element.GetAttribute("resource-id")}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {description}: {ex.Message}");
                }
            }
            
            Console.WriteLine("\n=== ANDROID DOCUMENTATION COMPLETE ===");
            Assert.True(true, "Android documentation completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Android documentation failed: {ex.Message}");
            throw;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}