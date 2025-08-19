using BitChat.Maui.Tests.UI.Shared;
using OpenQA.Selenium;
using Xunit;

namespace BitChat.Maui.Tests.UI;

/// <summary>
/// Comprehensive documentation of actual app behavior
/// This test captures the truth about what the app really does
/// </summary>
public class AppBehaviorDocumentation : AppiumTestBase
{
    public override Shared.Platform CurrentPlatform => Shared.Platform.iOS;

    public AppBehaviorDocumentation()
    {
        InitializeIOSDriver();
    }

    [Fact]
    [Trait("Category", "Documentation")]
    public void Document_iOS_ActualBehavior()
    {
        try
        {
            Console.WriteLine("=== iOS APP BEHAVIOR DOCUMENTATION ===");
            
            // Give app time to fully load
            Thread.Sleep(5000);
            
            // 1. Document initial app state
            Console.WriteLine("\n1. INITIAL APP STATE:");
            var initialPageSource = Driver?.PageSource ?? "";
            Console.WriteLine($"Page source length: {initialPageSource.Length} characters");
            
            // Save page source to file for analysis
            File.WriteAllText("ios_initial_state.xml", initialPageSource);
            Console.WriteLine("✅ Saved initial page source to ios_initial_state.xml");
            
            TakeScreenshot("iOS_Initial_State");
            Console.WriteLine("✅ Saved initial screenshot");
            
            // 2. Document all text fields
            Console.WriteLine("\n2. TEXT FIELDS ANALYSIS:");
            var textFields = Driver?.FindElements(By.XPath("//XCUIElementTypeTextField"));
            Console.WriteLine($"Found {textFields?.Count ?? 0} text fields");
            
            if (textFields != null)
            {
                for (int i = 0; i < textFields.Count; i++)
                {
                    var field = textFields[i];
                    Console.WriteLine($"TextField {i + 1}:");
                    Console.WriteLine($"  - Enabled: {field.Enabled}");
                    Console.WriteLine($"  - Displayed: {field.Displayed}");
                    Console.WriteLine($"  - Text: '{field.Text}'");
                    Console.WriteLine($"  - Value: '{field.GetAttribute("value")}'");
                    Console.WriteLine($"  - PlaceholderValue: '{field.GetAttribute("placeholderValue")}'");
                    Console.WriteLine($"  - Location: {field.Location}");
                    Console.WriteLine($"  - Size: {field.Size}");
                }
            }
            
            // 3. Document all buttons
            Console.WriteLine("\n3. BUTTONS ANALYSIS:");
            var buttons = Driver?.FindElements(By.XPath("//XCUIElementTypeButton"));
            Console.WriteLine($"Found {buttons?.Count ?? 0} buttons");
            
            if (buttons != null)
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    var button = buttons[i];
                    Console.WriteLine($"Button {i + 1}:");
                    Console.WriteLine($"  - Name: '{button.GetAttribute("name")}'");
                    Console.WriteLine($"  - Label: '{button.GetAttribute("label")}'");
                    Console.WriteLine($"  - Enabled: {button.Enabled}");
                    Console.WriteLine($"  - Displayed: {button.Displayed}");
                    Console.WriteLine($"  - Location: {button.Location}");
                    Console.WriteLine($"  - Size: {button.Size}");
                }
            }
            
            // 4. Document CollectionView
            Console.WriteLine("\n4. COLLECTION VIEW ANALYSIS:");
            var collectionViews = Driver?.FindElements(By.XPath("//XCUIElementTypeCollectionView"));
            Console.WriteLine($"Found {collectionViews?.Count ?? 0} collection views");
            
            if (collectionViews != null && collectionViews.Count > 0)
            {
                var collectionView = collectionViews[0];
                Console.WriteLine($"CollectionView:");
                Console.WriteLine($"  - Enabled: {collectionView.Enabled}");
                Console.WriteLine($"  - Displayed: {collectionView.Displayed}");
                Console.WriteLine($"  - Location: {collectionView.Location}");
                Console.WriteLine($"  - Size: {collectionView.Size}");
                
                // Count cells
                var cells = Driver?.FindElements(By.XPath("//XCUIElementTypeCollectionView//XCUIElementTypeCell"));
                Console.WriteLine($"  - Cell count: {cells?.Count ?? 0}");
                
                if (cells != null)
                {
                    for (int i = 0; i < Math.Min(3, cells.Count); i++) // Show first 3 cells
                    {
                        var cell = cells[i];
                        Console.WriteLine($"  Cell {i + 1}:");
                        Console.WriteLine($"    - Text content: '{cell.Text}'");
                        Console.WriteLine($"    - Location: {cell.Location}");
                    }
                }
            }
            
            // 5. Try to interact with text field
            Console.WriteLine("\n5. TEXT FIELD INTERACTION TEST:");
            try
            {
                var messageField = Driver?.FindElement(By.XPath("//XCUIElementTypeTextField[@placeholderValue='Type a message...']"));
                if (messageField != null)
                {
                    Console.WriteLine("✅ Found message input field");
                    
                    Console.WriteLine("Attempting to click on text field...");
                    messageField.Click();
                    Thread.Sleep(1000);
                    
                    Console.WriteLine("Attempting to type text...");
                    messageField.SendKeys("Test message");
                    Thread.Sleep(1000);
                    
                    Console.WriteLine($"Text after typing: '{messageField.GetAttribute("value")}'");
                    
                    TakeScreenshot("iOS_After_Text_Input");
                    
                    // Check if Send button is now enabled
                    var sendButton = Driver?.FindElement(By.XPath("//XCUIElementTypeButton[@name='Send']"));
                    if (sendButton != null)
                    {
                        Console.WriteLine($"Send button enabled after text input: {sendButton.Enabled}");
                        
                        if (sendButton.Enabled)
                        {
                            Console.WriteLine("Attempting to click Send button...");
                            sendButton.Click();
                            Thread.Sleep(3000);
                            
                            TakeScreenshot("iOS_After_Send_Click");
                            
                            // Check if message appeared
                            var updatedPageSource = Driver?.PageSource ?? "";
                            File.WriteAllText("ios_after_send.xml", updatedPageSource);
                            Console.WriteLine("✅ Saved page source after send to ios_after_send.xml");
                            
                            // Count cells again
                            var newCells = Driver?.FindElements(By.XPath("//XCUIElementTypeCollectionView//XCUIElementTypeCell"));
                            Console.WriteLine($"Cell count after send: {newCells?.Count ?? 0}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Interaction test failed: {ex.Message}");
            }
            
            Console.WriteLine("\n=== DOCUMENTATION COMPLETE ===");
            Console.WriteLine("Check the following files for detailed analysis:");
            Console.WriteLine("- ios_initial_state.xml");
            Console.WriteLine("- ios_after_send.xml"); 
            Console.WriteLine("- Screenshots folder");
            
            Assert.True(true, "Documentation completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Documentation failed: {ex.Message}");
            throw;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}