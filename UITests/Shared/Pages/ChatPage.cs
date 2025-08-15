using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace BitChat.Maui.UITests.Shared.Pages;

/// <summary>
/// Page object for the main chat interface
/// </summary>
public class ChatPage
{
    private readonly AppiumDriver _driver;
    private readonly Platform _platform;
    private readonly WebDriverWait _wait;

    public ChatPage(AppiumDriver driver, Platform platform)
    {
        _driver = driver;
        _platform = platform;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }

    // Element locators
    private By MessageInputLocator => _platform == Platform.iOS
        ? MobileBy.AccessibilityId("MessageEntry")
        : By.Id("MessageEntry");

    private By SendButtonLocator => _platform == Platform.iOS
        ? MobileBy.IosNSPredicate("label == 'Send'")
        : By.XPath("//android.widget.Button[@text='Send']");

    private By MessagesListLocator => _platform == Platform.iOS
        ? MobileBy.AccessibilityId("MessagesCollectionView")
        : By.Id("MessagesCollectionView");

    private By SettingsButtonLocator => _platform == Platform.iOS
        ? MobileBy.AccessibilityId("Open settings")
        : By.XPath("//android.widget.Button[@content-desc='Open settings']");

    private By EmptyStateLocator => _platform == Platform.iOS
        ? MobileBy.IosNSPredicate("label CONTAINS 'No messages yet'")
        : By.XPath("//*[contains(@text, 'No messages yet')]");

    private By ConnectionStatusLocator => _platform == Platform.iOS
        ? MobileBy.IosNSPredicate("label CONTAINS 'peers'")
        : By.XPath("//*[contains(@text, 'peers')]");

    // Actions
    public void EnterMessage(string message)
    {
        var input = _wait.Until(d => d.FindElement(MessageInputLocator));
        input.Clear();
        input.SendKeys(message);
    }

    public void SendMessage()
    {
        var sendButton = _wait.Until(d => d.FindElement(SendButtonLocator));
        sendButton.Click();
    }

    public void PressReturnKey()
    {
        var input = _driver.FindElement(MessageInputLocator);
        input.SendKeys(Keys.Return);
    }

    public void ClearMessageInput()
    {
        var input = _driver.FindElement(MessageInputLocator);
        input.Clear();
    }

    public string GetMessageInputText()
    {
        var input = _driver.FindElement(MessageInputLocator);
        return input.Text ?? string.Empty;
    }

    public bool IsSendButtonEnabled()
    {
        try
        {
            var sendButton = _driver.FindElement(SendButtonLocator);
            return sendButton.Enabled;
        }
        catch
        {
            return false;
        }
    }

    public void OpenSettings()
    {
        var settingsButton = _wait.Until(d => d.FindElement(SettingsButtonLocator));
        settingsButton.Click();
    }

    public List<string> GetMessages()
    {
        try
        {
            var messageElements = _platform == Platform.iOS
                ? _driver.FindElements(MobileBy.IosClassChain("**/XCUIElementTypeStaticText"))
                : _driver.FindElements(By.ClassName("android.widget.TextView"));

            return messageElements
                .Select(e => e.Text)
                .Where(text => !string.IsNullOrEmpty(text))
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public int GetMessageCount()
    {
        return GetMessages().Count;
    }

    public void WaitForMessage(string messageText, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d =>
        {
            var messages = GetMessages();
            return messages.Any(m => m.Contains(messageText));
        });
    }

    public void WaitForSystemMessage(int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d =>
        {
            var messages = GetMessages();
            return messages.Any(m => m.Contains("System"));
        });
    }

    public bool IsMessageVisible(string messageText)
    {
        var messages = GetMessages();
        return messages.Any(m => m.Contains(messageText));
    }

    public void ScrollToTop()
    {
        if (_platform == Platform.iOS)
        {
            _driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
            {
                { "direction", "up" },
                { "distance", 1000 }
            });
        }
        else
        {
            // Android scroll
            var element = _driver.FindElement(MessagesListLocator);
            _driver.ExecuteScript("arguments[0].scrollTop = 0", element);
        }
    }

    public void ScrollToBottom()
    {
        if (_platform == Platform.iOS)
        {
            _driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
            {
                { "direction", "down" },
                { "distance", 1000 }
            });
        }
        else
        {
            // Android scroll
            var element = _driver.FindElement(MessagesListLocator);
            _driver.ExecuteScript("arguments[0].scrollTop = arguments[0].scrollHeight", element);
        }
    }

    public bool IsEmptyStateVisible()
    {
        try
        {
            var emptyState = _driver.FindElement(EmptyStateLocator);
            return emptyState.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public string GetEmptyStateText()
    {
        try
        {
            var emptyState = _driver.FindElement(EmptyStateLocator);
            return emptyState.Text;
        }
        catch
        {
            return string.Empty;
        }
    }

    public bool IsConnectionStatusVisible()
    {
        try
        {
            var status = _driver.FindElement(ConnectionStatusLocator);
            return status.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public int GetPeerCount()
    {
        try
        {
            var statusText = _driver.FindElement(ConnectionStatusLocator).Text;
            var match = System.Text.RegularExpressions.Regex.Match(statusText, @"(\d+)\s+peers");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
        }
        catch
        {
            // Ignore
        }
        return 0;
    }

    public string GetMessageTimestamp(string messageText)
    {
        try
        {
            // Find the message and its associated timestamp
            var messages = GetMessages();
            var messageIndex = messages.FindIndex(m => m.Contains(messageText));
            
            if (messageIndex >= 0 && messageIndex < messages.Count - 1)
            {
                // Check next element for timestamp pattern
                var nextText = messages[messageIndex + 1];
                if (System.Text.RegularExpressions.Regex.IsMatch(nextText, @"\d{2}:\d{2}"))
                {
                    return nextText;
                }
            }
        }
        catch
        {
            // Ignore
        }
        return string.Empty;
    }

    public void ConfirmClearMessages()
    {
        try
        {
            // Look for confirmation dialog
            if (_platform == Platform.iOS)
            {
                var yesButton = _driver.FindElement(MobileBy.IosNSPredicate("label == 'Yes'"));
                yesButton.Click();
            }
            else
            {
                var yesButton = _driver.FindElement(By.XPath("//android.widget.Button[@text='Yes']"));
                yesButton.Click();
            }
        }
        catch
        {
            // Dialog might not appear in tests
        }
    }

    public bool IsOnChatPage()
    {
        try
        {
            return _driver.FindElement(MessageInputLocator).Displayed;
        }
        catch
        {
            return false;
        }
    }
}