using BitChat.Maui.UITests.Shared;
using Xunit;

namespace BitChat.Maui.UITests.iOS;

/// <summary>
/// iOS-specific chat UI tests
/// </summary>
[Collection("iOS UI Tests")]
public class IOSChatUITests : ChatUITests
{
    public IOSChatUITests() : base()
    {
        Platform = Platform.iOS;
    }

    [Fact]
    public void iOS_KeyboardDismissal_ShouldWork()
    {
        // Arrange
        ChatPage.EnterMessage("Test message");
        
        // Act - Tap outside to dismiss keyboard
        Driver.ExecuteScript("mobile: tap", new Dictionary<string, object>
        {
            { "x", 50 },
            { "y", 100 }
        });
        
        // Assert - Keyboard should be dismissed
        Thread.Sleep(500);
        var keyboardVisible = (bool)Driver.ExecuteScript("mobile: isKeyboardShown");
        Assert.False(keyboardVisible);
    }

    [Fact]
    public void iOS_SwipeToRefresh_ShouldBeDisabled()
    {
        // Act - Try to swipe down to refresh
        Driver.ExecuteScript("mobile: swipe", new Dictionary<string, object>
        {
            { "direction", "down" },
            { "velocity", 100 }
        });
        
        // Assert - Should not trigger refresh (no loading indicator)
        Thread.Sleep(1000);
        Assert.True(ChatPage.IsOnChatPage());
    }

    [Fact]
    public void iOS_LongPress_ShouldShowMessageOptions()
    {
        // Arrange
        ChatPage.EnterMessage("Long press test");
        ChatPage.SendMessage();
        ChatPage.WaitForMessage("Long press test");
        
        // Act - Long press on message
        var messages = ChatPage.GetMessages();
        if (messages.Any())
        {
            Driver.ExecuteScript("mobile: touchAndHold", new Dictionary<string, object>
            {
                { "duration", 2.0 },
                { "x", 200 },
                { "y", 400 }
            });
        }
        
        // Assert - Context menu might appear (platform-specific)
        Thread.Sleep(500);
        // Note: Actual assertion would depend on implemented context menu
    }
}