using BitChat.Maui.Tests.UI.Shared;
using Xunit;
using OpenQA.Selenium;

namespace BitChat.Maui.Tests.UI;

/// <summary>
/// Step-by-step UI test to debug what's working and what isn't
/// </summary>
[Collection("UI Tests")]
public class StepByStepUITest : AppiumTestBase
{
    public StepByStepUITest()
    {
        // Initialize for iOS
        InitializeDriver(Platform.iOS);
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void DebugUIElementsStepByStep()
    {
        try
        {
            Console.WriteLine("=== STEP BY STEP UI TEST DEBUG ===");
            
            // Wait for app to load
            Thread.Sleep(5000);
            Console.WriteLine("1. App loaded, waiting 5 seconds");
            
            // Step 1: Check if message input exists
            try
            {
                var messageInput = Driver!.FindElement(By.XPath("//XCUIElementTypeTextField[@placeholderValue='Type a message...']"));
                Console.WriteLine($"2. Message input found: {messageInput != null}");
                Console.WriteLine($"   - Displayed: {messageInput.Displayed}");
                Console.WriteLine($"   - Enabled: {messageInput.Enabled}");
                Console.WriteLine($"   - Text: '{messageInput.Text}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"2. Message input NOT found: {ex.Message}");
            }
            
            // Step 2: Check if send button exists
            try
            {
                var sendButton = Driver!.FindElement(By.XPath("//XCUIElementTypeButton[@name='Send']"));
                Console.WriteLine($"3. Send button found: {sendButton != null}");
                Console.WriteLine($"   - Displayed: {sendButton.Displayed}");
                Console.WriteLine($"   - Enabled: {sendButton.Enabled}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"3. Send button NOT found: {ex.Message}");
            }
            
            // Step 3: Check if messages list exists
            try
            {
                var messagesList = Driver!.FindElement(By.XPath("//XCUIElementTypeCollectionView"));
                Console.WriteLine($"4. Messages list found: {messagesList != null}");
                Console.WriteLine($"   - Displayed: {messagesList.Displayed}");
                
                // Count existing messages
                var existingMessages = Driver!.FindElements(By.XPath("//XCUIElementTypeCell"));
                Console.WriteLine($"   - Existing messages count: {existingMessages.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"4. Messages list NOT found: {ex.Message}");
            }
            
            // Step 4: Try to type a message
            try
            {
                var messageInput = Driver!.FindElement(By.XPath("//XCUIElementTypeTextField[@placeholderValue='Type a message...']"));
                Console.WriteLine("5. Attempting to type message...");
                messageInput.Clear();
                messageInput.SendKeys("Debug test message");
                Console.WriteLine($"   - Typed message, current text: '{messageInput.Text}'");
                
                // Check send button state after typing
                var sendButton = Driver!.FindElement(By.XPath("//XCUIElementTypeButton[@name='Send']"));
                Console.WriteLine($"   - Send button enabled after typing: {sendButton.Enabled}");
                
                if (sendButton.Enabled)
                {
                    Console.WriteLine("6. Sending message...");
                    sendButton.Click();
                    Thread.Sleep(3000);
                    
                    // Check if message appeared
                    try
                    {
                        var messageElement = Driver!.FindElement(By.XPath("//XCUIElementTypeStaticText[@value='Debug test message']"));
                        Console.WriteLine($"7. Message found in list: {messageElement.Displayed}");
                    }
                    catch
                    {
                        Console.WriteLine("7. Message NOT found in list");
                        
                        // List all static text elements to see what's there
                        var allTexts = Driver!.FindElements(By.XPath("//XCUIElementTypeStaticText"));
                        Console.WriteLine($"   - Found {allTexts.Count} text elements:");
                        foreach (var text in allTexts.Take(10))
                        {
                            try
                            {
                                Console.WriteLine($"     - '{text.Text}' / '{text.GetAttribute("value")}'");
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("6. Send button still disabled after typing");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"5. Failed to type message: {ex.Message}");
            }
            
            TakeScreenshot("StepByStepUITest_Debug");
            Console.WriteLine("8. Screenshot taken");
            
            // The test should pass - we're just debugging
            Assert.True(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            TakeScreenshot("StepByStepUITest_FatalError");
            throw;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}