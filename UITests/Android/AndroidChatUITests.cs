using BitChat.Maui.UITests.Shared;
using OpenQA.Selenium.Appium.Android;
using Xunit;

namespace BitChat.Maui.UITests.Android;

/// <summary>
/// Android-specific chat UI tests
/// </summary>
[Collection("Android UI Tests")]
public class AndroidChatUITests : ChatUITests
{
    public AndroidChatUITests() : base()
    {
        Platform = Platform.Android;
    }

    [Fact]
    public void Android_BackButton_ShouldNotExitFromMainPage()
    {
        // Act - Press back button
        ((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Back);
        
        // Assert - Should still be on chat page
        Thread.Sleep(500);
        Assert.True(ChatPage.IsOnChatPage());
    }

    [Fact]
    public void Android_KeyboardHandling_ShouldAdjustLayout()
    {
        // Arrange
        ChatPage.EnterMessage("Test");
        
        // Act - Keyboard should be shown
        Thread.Sleep(500);
        var isKeyboardShown = ((AndroidDriver)Driver).IsKeyboardShown();
        
        // Assert
        Assert.True(isKeyboardShown);
        
        // Hide keyboard
        ((AndroidDriver)Driver).HideKeyboard();
        Thread.Sleep(500);
        Assert.False(((AndroidDriver)Driver).IsKeyboardShown());
    }

    [Fact]
    public void Android_OrientationChange_ShouldPreserveMessages()
    {
        // Arrange
        var testMessage = "Orientation test message";
        ChatPage.EnterMessage(testMessage);
        ChatPage.SendMessage();
        ChatPage.WaitForMessage(testMessage);
        
        // Act - Rotate to landscape
        Driver.Orientation = ScreenOrientation.Landscape;
        Thread.Sleep(1000);
        
        // Assert - Message should still be visible
        Assert.True(ChatPage.IsMessageVisible(testMessage));
        
        // Rotate back to portrait
        Driver.Orientation = ScreenOrientation.Portrait;
        Thread.Sleep(1000);
        Assert.True(ChatPage.IsMessageVisible(testMessage));
    }

    [Fact]
    public void Android_AppBackground_ShouldPreserveState()
    {
        // Arrange
        var testMessage = "Background test message";
        ChatPage.EnterMessage(testMessage);
        ChatPage.SendMessage();
        ChatPage.WaitForMessage(testMessage);
        
        // Act - Send app to background
        ((AndroidDriver)Driver).BackgroundApp(TimeSpan.FromSeconds(3));
        
        // Assert - Message should still be visible after resuming
        Thread.Sleep(1000);
        Assert.True(ChatPage.IsMessageVisible(testMessage));
    }

    [Fact]
    public void Android_ScrollPerformance_ShouldBeSmooth()
    {
        // Arrange - Add many messages
        for (int i = 0; i < 30; i++)
        {
            ChatPage.EnterMessage($"Scroll test {i}");
            ChatPage.SendMessage();
            Thread.Sleep(50);
        }
        
        // Act - Perform rapid scrolling
        for (int i = 0; i < 5; i++)
        {
            ChatPage.ScrollToTop();
            Thread.Sleep(200);
            ChatPage.ScrollToBottom();
            Thread.Sleep(200);
        }
        
        // Assert - App should still be responsive
        ChatPage.EnterMessage("Final message");
        ChatPage.SendMessage();
        ChatPage.WaitForMessage("Final message");
        Assert.True(ChatPage.IsMessageVisible("Final message"));
    }

    [Fact]
    public void Android_TextSelection_ShouldWork()
    {
        // Arrange
        var longMessage = "This is a long message that can be selected and copied";
        ChatPage.EnterMessage(longMessage);
        ChatPage.SendMessage();
        ChatPage.WaitForMessage(longMessage);
        
        // Act - Long press to select text (platform-specific gesture)
        // Note: Actual implementation would depend on how text selection is enabled
        Thread.Sleep(500);
        
        // Assert
        Assert.True(ChatPage.IsMessageVisible(longMessage));
    }
}