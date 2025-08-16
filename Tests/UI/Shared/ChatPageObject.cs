using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;

namespace BitChat.Maui.Tests.UI.Shared;

/// <summary>
/// Page Object Model for the BitChat main chat interface
/// </summary>
public class ChatPageObject
{
    private readonly AppiumDriver _driver;
    private readonly AppiumTestBase _testBase;
    
    public ChatPageObject(AppiumDriver driver, AppiumTestBase testBase)
    {
        _driver = driver;
        _testBase = testBase;
    }
    
    #region Element Locators
    
    // Message input and send
    private By MessageInputField => GetLocator("MessageInput", "messageInput");
    private By SendButton => GetLocator("SendButton", "sendButton");
    
    // Message list and items
    private By MessagesList => GetLocator("MessagesList", "messagesList");
    private By MessageItems => GetLocator("MessageItem", "messageItem");
    
    // Commands and status
    private By ConnectedPeersLabel => GetLocator("ConnectedPeersLabel", "connectedPeersLabel");
    private By EmptyStateLabel => GetLocator("EmptyStateLabel", "emptyStateLabel");
    
    // Toolbar buttons
    private By ClearMessagesButton => GetLocator("ClearMessagesButton", "clearMessagesButton");
    private By RefreshPeersButton => GetLocator("RefreshPeersButton", "refreshPeersButton");
    
    // Public accessor methods for locators (needed by test classes)
    public By GetMessageInputLocator() => MessageInputField;
    public By GetSendButtonLocator() => SendButton;
    public By GetMessagesListLocator() => MessagesList;
    public By GetMessageItemsLocator() => MessageItems;
    public By GetConnectedPeersLocator() => ConnectedPeersLabel;
    public By GetEmptyStateLocator() => EmptyStateLabel;
    public By GetClearButtonLocator() => ClearMessagesButton;
    public By GetRefreshButtonLocator() => RefreshPeersButton;
    
    #endregion
    
    #region Helper Methods
    
    private By GetLocator(string automationId, string fallbackId)
    {
        return _testBase.CurrentPlatform switch
        {
            // Use XPath selectors for iOS since MAUI doesn't set accessibility IDs properly
            Platform.iOS => automationId switch
            {
                "MessageInput" => By.XPath("//XCUIElementTypeTextField[@placeholderValue='Type a message...']"),
                "SendButton" => By.XPath("//XCUIElementTypeButton[@name='Send']"),
                "MessagesList" => By.XPath("//XCUIElementTypeCollectionView"),
                "MessageItem" => By.XPath("//XCUIElementTypeCell"),
                "ConnectedPeersLabel" => By.XPath("//XCUIElementTypeStaticText[contains(@value, 'peers')]"),
                "EmptyStateLabel" => By.XPath("//XCUIElementTypeStaticText[contains(@value, 'No messages')]"),
                "ClearMessagesButton" => By.XPath("//XCUIElementTypeButton[@name='Clear']"),
                "RefreshPeersButton" => By.XPath("//XCUIElementTypeButton[@name='Refresh']"),
                _ => MobileBy.AccessibilityId(automationId) // Fallback to accessibility ID
            },
            Platform.Android => MobileBy.Id($"com.bitchat.maui:id/{fallbackId}"),
            _ => throw new ArgumentException($"Unsupported platform: {_testBase.CurrentPlatform}")
        };
    }
    
    private By GetTextLocator(string text)
    {
        return _testBase.CurrentPlatform switch
        {
            Platform.iOS => MobileBy.XPath($"//XCUIElementTypeStaticText[contains(@value,'{text}')]"),
            Platform.Android => MobileBy.XPath($"//*[contains(@text,'{text}')]"),
            _ => throw new ArgumentException($"Unsupported platform: {_testBase.CurrentPlatform}")
        };
    }
    
    #endregion
    
    #region Chat Actions
    
    /// <summary>
    /// Sends a public message
    /// </summary>
    public void SendMessage(string message)
    {
        _testBase.EnterText(MessageInputField, message);
        _testBase.TapElement(SendButton);
        
        // Wait for message to appear in the list
        _testBase.WaitForTextInElement(MessagesList, message, TimeSpan.FromSeconds(10));
    }
    
    /// <summary>
    /// Sends a command (e.g., "/nick NewName")
    /// </summary>
    public void SendCommand(string command)
    {
        _testBase.EnterText(MessageInputField, command);
        _testBase.TapElement(SendButton);
        
        // Wait for the input to be cleared (indicates command was processed)
        Thread.Sleep(1000); // Allow time for command processing
    }
    
    /// <summary>
    /// Clears all messages
    /// </summary>
    public void ClearMessages()
    {
        _testBase.TapElement(ClearMessagesButton);
        
        // Handle platform-specific confirmation dialog
        if (_testBase.CurrentPlatform == Platform.iOS)
        {
            var confirmButton = MobileBy.AccessibilityId("Yes");
            if (_testBase.ElementExists(confirmButton))
            {
                _testBase.TapElement(confirmButton);
            }
        }
        else
        {
            var confirmButton = MobileBy.Id("android:id/button1");
            if (_testBase.ElementExists(confirmButton))
            {
                _testBase.TapElement(confirmButton);
            }
        }
    }
    
    /// <summary>
    /// Refreshes the peer list
    /// </summary>
    public void RefreshPeers()
    {
        _testBase.TapElement(RefreshPeersButton);
    }
    
    #endregion
    
    #region Verification Methods
    
    /// <summary>
    /// Verifies that a message appears in the chat
    /// </summary>
    public bool VerifyMessageExists(string messageContent)
    {
        return _testBase.WaitForTextInElement(MessagesList, messageContent, TimeSpan.FromSeconds(5));
    }
    
    /// <summary>
    /// Verifies that a message with specific sender appears
    /// </summary>
    public bool VerifyMessageFromSender(string sender, string messageContent)
    {
        var messageLocator = _testBase.CurrentPlatform switch
        {
            Platform.iOS => MobileBy.XPath($"//XCUIElementTypeCell[contains(@value,'{sender}') and contains(@value,'{messageContent}')]"),
            Platform.Android => MobileBy.XPath($"//*[contains(@text,'{sender}') and contains(@text,'{messageContent}')]"),
            _ => throw new ArgumentException($"Unsupported platform: {_testBase.CurrentPlatform}")
        };
        
        return _testBase.ElementExists(messageLocator);
    }
    
    /// <summary>
    /// Verifies the empty state is shown
    /// </summary>
    public bool VerifyEmptyState()
    {
        return _testBase.ElementExists(EmptyStateLabel);
    }
    
    /// <summary>
    /// Verifies the connected peers count
    /// </summary>
    public bool VerifyConnectedPeersCount(int expectedCount)
    {
        if (!_testBase.ElementExists(ConnectedPeersLabel))
            return expectedCount == 0;
            
        var text = _testBase.GetElementText(ConnectedPeersLabel);
        return text.Contains(expectedCount.ToString());
    }
    
    /// <summary>
    /// Gets the current message input text
    /// </summary>
    public string GetMessageInputText()
    {
        var element = _testBase.WaitForElement(MessageInputField);
        
        // For text fields, use the value attribute rather than Text property
        // value attribute contains actual input text, not placeholder
        var inputValue = element.GetAttribute("value");
        
        // If value is the placeholder text or null, consider it empty
        return (inputValue == "Type a message..." || string.IsNullOrEmpty(inputValue)) ? "" : inputValue;
    }
    
    /// <summary>
    /// Verifies that the message input is empty
    /// </summary>
    public bool VerifyMessageInputIsEmpty()
    {
        var inputText = GetMessageInputText();
        return string.IsNullOrEmpty(inputText);
    }
    
    /// <summary>
    /// Gets the number of messages in the chat
    /// </summary>
    public int GetMessageCount()
    {
        try
        {
            var messages = _driver.FindElements(MessageItems);
            return messages.Count;
        }
        catch (NoSuchElementException)
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Verifies that a system message appears (e.g., command responses)
    /// </summary>
    public bool VerifySystemMessage(string expectedContent)
    {
        var systemMessageLocator = GetTextLocator("System");
        if (!_testBase.ElementExists(systemMessageLocator))
            return false;
            
        return _testBase.WaitForTextInElement(MessagesList, expectedContent, TimeSpan.FromSeconds(5));
    }
    
    /// <summary>
    /// Verifies that the send button is enabled/disabled
    /// </summary>
    public bool VerifySendButtonState(bool shouldBeEnabled)
    {
        var element = _testBase.WaitForElement(SendButton);
        return element.Enabled == shouldBeEnabled;
    }
    
    /// <summary>
    /// Waits for the chat interface to load completely
    /// </summary>
    public void WaitForChatToLoad()
    {
        // Wait for key elements to be present
        _testBase.WaitForElement(MessageInputField, TimeSpan.FromSeconds(30));
        _testBase.WaitForElement(SendButton, TimeSpan.FromSeconds(30));
        _testBase.WaitForElement(MessagesList, TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Enters text into an element (wrapper for test base method)
    /// </summary>
    public void EnterText(By locator, string text)
    {
        _testBase.EnterText(locator, text);
    }

    /// <summary>
    /// Taps an element (wrapper for test base method)
    /// </summary>
    public void TapElement(By locator)
    {
        _testBase.TapElement(locator);
    }
    
    #endregion
    
    #region Advanced Actions
    
    /// <summary>
    /// Scrolls the message list to the top
    /// </summary>
    public void ScrollToTop()
    {
        var messagesList = _testBase.WaitForElement(MessagesList);
        
        if (_testBase.CurrentPlatform == Platform.iOS)
        {
            // iOS scroll gesture
            _driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
            {
                {"element", messagesList},
                {"direction", "up"}
            });
        }
        else
        {
            // Android scroll gesture
            var actions = new Actions(_driver);
            var messageListRect = messagesList.Location;
            var messageListSize = messagesList.Size;
            
            var startX = messageListRect.X + messageListSize.Width / 2;
            var startY = messageListRect.Y + messageListSize.Height / 4;
            var endY = messageListRect.Y + messageListSize.Height * 3 / 4;
            
            actions.MoveToLocation(startX, startY)
                   .ClickAndHold()
                   .MoveToLocation(startX, endY)
                   .Release()
                   .Perform();
        }
    }
    
    /// <summary>
    /// Scrolls the message list to the bottom
    /// </summary>
    public void ScrollToBottom()
    {
        var messagesList = _testBase.WaitForElement(MessagesList);
        
        if (_testBase.CurrentPlatform == Platform.iOS)
        {
            // iOS scroll gesture
            _driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
            {
                {"element", messagesList},
                {"direction", "down"}
            });
        }
        else
        {
            // Android scroll gesture
            var actions = new Actions(_driver);
            var messageListRect = messagesList.Location;
            var messageListSize = messagesList.Size;
            
            var startX = messageListRect.X + messageListSize.Width / 2;
            var startY = messageListRect.Y + messageListSize.Height * 3 / 4;
            var endY = messageListRect.Y + messageListSize.Height / 4;
            
            actions.MoveToLocation(startX, startY)
                   .ClickAndHold()
                   .MoveToLocation(startX, endY)
                   .Release()
                   .Perform();
        }
    }
    
    #endregion
}