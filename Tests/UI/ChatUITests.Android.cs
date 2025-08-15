using BitChat.Maui.Tests.UI.Shared;
using FluentAssertions;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace BitChat.Maui.Tests.UI;

/// <summary>
/// Android-specific UI tests for the main chat interface - US-7.1.1 Public Chat View
/// Tests the complete user workflows for the chat functionality on Android
/// </summary>
[Collection("UI Tests")]
public class ChatUITestsAndroid : AppiumTestBase
{
    private ChatPageObject _chatPage = null!;

    public ChatUITestsAndroid()
    {
        // Initialize for Android
        InitializeDriver(Platform.Android);
        _chatPage = new ChatPageObject(Driver!, this);
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "Android")]
    public void ChatInterface_Android_ShouldLoad_WithAllElements()
    {
        try
        {
            // Arrange & Act
            _chatPage.WaitForChatToLoad();

            // Assert - Verify all essential UI elements are present
            var messageInputExists = ElementExists(_chatPage.GetMessageInputLocator());
            var sendButtonExists = ElementExists(_chatPage.GetSendButtonLocator());
            var messagesListExists = ElementExists(_chatPage.GetMessagesListLocator());

            messageInputExists.Should().BeTrue("Message input field should be visible on Android");
            sendButtonExists.Should().BeTrue("Send button should be visible on Android");
            messagesListExists.Should().BeTrue("Messages list should be visible on Android");

            // Verify initial state
            _chatPage.VerifyEmptyState().Should().BeTrue("Chat should start in empty state on Android");
            _chatPage.VerifyMessageInputIsEmpty().Should().BeTrue("Message input should be empty initially on Android");
        }
        catch (Exception ex)
        {
            TakeScreenshot("Android_ChatInterface_Load_Failed");
            throw new Exception($"Android chat interface failed to load: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "Android")]
    public void SendMessage_Android_ShouldDisplayInChat()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            var testMessage = "Hello from Android UI test!";

            // Act
            _chatPage.SendMessage(testMessage);

            // Assert
            _chatPage.VerifyMessageExists(testMessage).Should().BeTrue($"Message '{testMessage}' should appear in Android chat");
            _chatPage.GetMessageCount().Should().BeGreaterThan(0, "Message count should increase on Android");
            _chatPage.VerifyMessageInputIsEmpty().Should().BeTrue("Message input should be cleared after sending on Android");
        }
        catch (Exception ex)
        {
            TakeScreenshot("Android_SendMessage_Failed");
            throw new Exception($"Android send message test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "Android")]
    public void SendCommand_Nick_Android_ShouldChangeNickname()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            var newNickname = "AndroidUITestNick";
            var nickCommand = $"/nick {newNickname}";

            // Act
            _chatPage.SendCommand(nickCommand);

            // Assert
            _chatPage.VerifySystemMessage($"Nickname changed to {newNickname}")
                .Should().BeTrue("System message should confirm nickname change on Android");
            
            // Send a test message to verify nickname appears
            _chatPage.SendMessage("Test message with new nick on Android");
            _chatPage.VerifyMessageFromSender(newNickname, "Test message with new nick on Android")
                .Should().BeTrue("Message should show new nickname on Android");
        }
        catch (Exception ex)
        {
            TakeScreenshot("Android_NickCommand_Failed");
            throw new Exception($"Android nick command test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "Android")]
    public void AndroidBackButton_ShouldNotCrashApp()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            _chatPage.SendMessage("Test message before back button");

            // Act - Press Android back button
            Driver!.Navigate().Back();
            Thread.Sleep(1000);

            // Assert - App should still be responsive
            // Try to interact with the app again
            _chatPage.WaitForChatToLoad();
            _chatPage.SendMessage("Test message after back button");
            
            _chatPage.VerifyMessageExists("Test message after back button")
                .Should().BeTrue("App should remain functional after back button press");
        }
        catch (Exception ex)
        {
            TakeScreenshot("Android_BackButton_Failed");
            throw new Exception($"Android back button test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "Android")]
    public void AndroidKeyboard_ShouldShowAndHide()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();

            // Act - Tap message input to show keyboard
            _chatPage.TapElement(_chatPage.GetMessageInputLocator());
            Thread.Sleep(1000);

            // Assert - Should be able to type
            _chatPage.EnterText(_chatPage.GetMessageInputLocator(), "Keyboard test");
            var inputText = _chatPage.GetMessageInputText();
            inputText.Should().Contain("Keyboard test", "Should be able to type with Android keyboard");

            // Hide keyboard by tapping elsewhere or sending message
            _chatPage.TapElement(_chatPage.GetSendButtonLocator());
            Thread.Sleep(1000);

            // Should be able to continue interacting
            _chatPage.VerifyMessageExists("Keyboard test").Should().BeTrue("Message should be sent after keyboard interaction");
        }
        catch (Exception ex)
        {
            TakeScreenshot("Android_Keyboard_Failed");
            throw new Exception($"Android keyboard test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "Android")]
    public void AndroidRotation_ShouldMaintainState()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            _chatPage.SendMessage("Message before rotation");
            var initialCount = _chatPage.GetMessageCount();

            // Act - Rotate device (if supported in emulator)
            try
            {
                SetOrientation(ScreenOrientation.Landscape);
                Thread.Sleep(2000);
                
                SetOrientation(ScreenOrientation.Portrait);
                Thread.Sleep(2000);
            }
            catch
            {
                // Rotation may not be supported in all test environments
                // Continue with test anyway
            }

            // Assert - State should be maintained
            _chatPage.WaitForChatToLoad();
            _chatPage.VerifyMessageExists("Message before rotation")
                .Should().BeTrue("Messages should persist after rotation");
            
            var currentCount = _chatPage.GetMessageCount();
            currentCount.Should().BeGreaterOrEqualTo(initialCount, "Message count should be maintained after rotation");
        }
        catch (Exception ex)
        {
            TakeScreenshot("Android_Rotation_Failed");
            throw new Exception($"Android rotation test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "Android")]
    public void AndroidSwipeGestures_ShouldWork()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            
            // Send enough messages to enable scrolling
            for (int i = 1; i <= 15; i++)
            {
                _chatPage.SendMessage($"Android scroll test message {i}");
                Thread.Sleep(100);
            }

            // Act & Assert - Test scrolling
            _chatPage.ScrollToTop();
            Thread.Sleep(1000);
            
            _chatPage.ScrollToBottom();
            Thread.Sleep(1000);
            
            // Verify we can still see recent messages
            _chatPage.VerifyMessageExists("Android scroll test message 15")
                .Should().BeTrue("Should be able to scroll on Android and see latest message");
        }
        catch (Exception ex)
        {
            TakeScreenshot("Android_SwipeGestures_Failed");
            throw new Exception($"Android swipe gestures test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "Android")]
    public void AndroidLongPress_ShouldNotCrash()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            _chatPage.SendMessage("Message for long press test");

            // Act - Long press on message (Android-specific gesture)
            var messagesList = _chatPage.GetMessagesListLocator();
            if (ElementExists(messagesList))
            {
                var messagesElement = WaitForElement(messagesList);
                
                // Perform long press using Android-specific action
                var actions = new Actions(Driver!);
                actions.ClickAndHold(messagesElement)
                       .Pause(TimeSpan.FromMilliseconds(1500))
                       .Release()
                       .Perform();
                       
                Thread.Sleep(1000);
            }

            // Assert - App should remain stable
            _chatPage.WaitForChatToLoad();
            _chatPage.VerifyMessageExists("Message for long press test")
                .Should().BeTrue("App should remain stable after long press gesture");
        }
        catch (Exception ex)
        {
            TakeScreenshot("Android_LongPress_Failed");
            throw new Exception($"Android long press test failed: {ex.Message}", ex);
        }
    }

    public override void Dispose()
    {
        _chatPage = null!;
        base.Dispose();
    }
}