using BitChat.Maui.Tests.UI.Shared;
using FluentAssertions;
using Xunit;

namespace BitChat.Maui.Tests.UI;

/// <summary>
/// UI tests for the main chat interface - US-7.1.1 Public Chat View
/// Tests the complete user workflows for the chat functionality
/// </summary>
[Collection("UI Tests")]
public class ChatUITests : AppiumTestBase
{
    private ChatPageObject _chatPage = null!;
    
    public override Shared.Platform CurrentPlatform => Shared.Platform.iOS;

    public ChatUITests()
    {
        // Initialize for iOS by default - can be parameterized later
        InitializeIOSDriver();
        _chatPage = new ChatPageObject(Driver!, this);
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void ChatInterface_ShouldLoad_WithAllElements()
    {
        try
        {
            // Arrange & Act
            _chatPage.WaitForChatToLoad();

            // Assert - Verify all essential UI elements are present
            var messageInputExists = ElementExists(_chatPage.GetMessageInputLocator());
            var sendButtonExists = ElementExists(_chatPage.GetSendButtonLocator());
            var messagesListExists = ElementExists(_chatPage.GetMessagesListLocator());

            messageInputExists.Should().BeTrue("Message input field should be visible");
            sendButtonExists.Should().BeTrue("Send button should be visible");
            messagesListExists.Should().BeTrue("Messages list should be visible");

            // Verify initial state - app loads with demo messages, so it's not empty
            _chatPage.GetMessageCount().Should().BeGreaterThan(0, "App should load with demo messages");
            _chatPage.VerifyMessageInputIsEmpty().Should().BeTrue("Message input should be empty initially");
        }
        catch (Exception ex)
        {
            TakeScreenshot("ChatInterface_Load_Failed");
            throw new Exception($"Chat interface failed to load: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendMessage_ShouldDisplayInChat()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            var testMessage = "Hello from UI test!";

            // Act
            _chatPage.SendMessage(testMessage);

            // Assert
            _chatPage.VerifyMessageExists(testMessage).Should().BeTrue($"Message '{testMessage}' should appear in chat");
            _chatPage.GetMessageCount().Should().BeGreaterThan(0, "Message count should increase");
            _chatPage.VerifyMessageInputIsEmpty().Should().BeTrue("Message input should be cleared after sending");
        }
        catch (Exception ex)
        {
            TakeScreenshot("SendMessage_Failed");
            throw new Exception($"Send message test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendMultipleMessages_ShouldDisplayInOrder()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            var messages = new[] { "First message", "Second message", "Third message" };

            // Act
            foreach (var message in messages)
            {
                _chatPage.SendMessage(message);
                Thread.Sleep(500); // Small delay between messages
            }

            // Assert
            foreach (var message in messages)
            {
                _chatPage.VerifyMessageExists(message).Should().BeTrue($"Message '{message}' should be visible");
            }

            _chatPage.GetMessageCount().Should().BeGreaterOrEqualTo(messages.Length, "All messages should be displayed");
        }
        catch (Exception ex)
        {
            TakeScreenshot("SendMultipleMessages_Failed");
            throw new Exception($"Send multiple messages test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendCommand_Nick_ShouldChangeNickname()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            var newNickname = "UITestNick";
            var nickCommand = $"/nick {newNickname}";

            // Act
            _chatPage.SendCommand(nickCommand);

            // Assert
            _chatPage.VerifySystemMessage($"Nickname changed to {newNickname}")
                .Should().BeTrue("System message should confirm nickname change");
            
            // Send a test message to verify nickname appears
            _chatPage.SendMessage("Test message with new nick");
            _chatPage.VerifyMessageFromSender(newNickname, "Test message with new nick")
                .Should().BeTrue("Message should show new nickname");
        }
        catch (Exception ex)
        {
            TakeScreenshot("NickCommand_Failed");
            throw new Exception($"Nick command test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendCommand_Clear_ShouldClearMessages()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            
            // Send some messages first
            _chatPage.SendMessage("Message to be cleared 1");
            _chatPage.SendMessage("Message to be cleared 2");
            var initialCount = _chatPage.GetMessageCount();
            initialCount.Should().BeGreaterThan(0, "Should have messages before clearing");

            // Act
            _chatPage.SendCommand("/clear");

            // Assert
            _chatPage.VerifyEmptyState().Should().BeTrue("Chat should return to empty state after clear");
            _chatPage.GetMessageCount().Should().Be(0, "Message count should be zero after clear");
        }
        catch (Exception ex)
        {
            TakeScreenshot("ClearCommand_Failed");
            throw new Exception($"Clear command test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendCommand_Help_ShouldShowHelpMessage()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();

            // Act
            _chatPage.SendCommand("/help");

            // Assert
            _chatPage.VerifySystemMessage("Available commands:")
                .Should().BeTrue("Help message should be displayed");
        }
        catch (Exception ex)
        {
            TakeScreenshot("HelpCommand_Failed");
            throw new Exception($"Help command test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendCommand_Who_ShouldShowConnectedPeers()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();

            // Act
            _chatPage.SendCommand("/who");

            // Assert
            // Since no peers are connected in UI test, should show appropriate message
            _chatPage.VerifySystemMessage("Connected peers:")
                .Should().BeTrue("Who command should show peer information");
        }
        catch (Exception ex)
        {
            TakeScreenshot("WhoCommand_Failed");
            throw new Exception($"Who command test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendEmptyMessage_ShouldNotSend()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            var initialCount = _chatPage.GetMessageCount();

            // Act - Try to send empty message
            _chatPage.EnterText(_chatPage.GetMessageInputLocator(), "");
            _chatPage.TapElement(_chatPage.GetSendButtonLocator());

            // Assert
            _chatPage.GetMessageCount().Should().Be(initialCount, "Empty message should not be sent");
            _chatPage.VerifyMessageInputIsEmpty().Should().BeTrue("Input should remain empty");
        }
        catch (Exception ex)
        {
            TakeScreenshot("EmptyMessage_Failed");
            throw new Exception($"Empty message test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendLongMessage_ShouldHandleCorrectly()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            var longMessage = new string('A', 500); // 500 character message

            // Act
            _chatPage.SendMessage(longMessage);

            // Assert
            _chatPage.VerifyMessageExists(longMessage).Should().BeTrue("Long message should be sent");
            _chatPage.GetMessageCount().Should().BeGreaterThan(0, "Message should be added to list");
        }
        catch (Exception ex)
        {
            TakeScreenshot("LongMessage_Failed");
            throw new Exception($"Long message test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void MessageList_ShouldScrollCorrectly()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            
            // Send many messages to trigger scrolling
            for (int i = 1; i <= 20; i++)
            {
                _chatPage.SendMessage($"Scroll test message {i}");
                Thread.Sleep(100); // Small delay between messages
            }

            // Act & Assert
            _chatPage.ScrollToTop();
            Thread.Sleep(1000);
            
            _chatPage.ScrollToBottom();
            Thread.Sleep(1000);
            
            // Verify the last message is visible
            _chatPage.VerifyMessageExists("Scroll test message 20")
                .Should().BeTrue("Should be able to scroll and see last message");
        }
        catch (Exception ex)
        {
            TakeScreenshot("ScrollTest_Failed");
            throw new Exception($"Scroll test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void ClearButton_ShouldClearMessages()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();
            _chatPage.SendMessage("Message to clear");
            _chatPage.GetMessageCount().Should().BeGreaterThan(0, "Should have messages to clear");

            // Act
            _chatPage.ClearMessages();

            // Assert
            _chatPage.VerifyEmptyState().Should().BeTrue("Chat should show empty state after clear");
            _chatPage.GetMessageCount().Should().Be(0, "All messages should be cleared");
        }
        catch (Exception ex)
        {
            TakeScreenshot("ClearButton_Failed");
            throw new Exception($"Clear button test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void ConnectedPeersDisplay_ShouldShowCorrectCount()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();

            // Act & Assert
            // In UI test environment, no peers should be connected
            _chatPage.VerifyConnectedPeersCount(0).Should().BeTrue("Should show 0 connected peers in test environment");
        }
        catch (Exception ex)
        {
            TakeScreenshot("ConnectedPeers_Failed");
            throw new Exception($"Connected peers test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Platform", "iOS")]
    public void SendButton_ShouldBeEnabledWithText()
    {
        try
        {
            // Arrange
            _chatPage.WaitForChatToLoad();

            // Act & Assert
            // Initially should be disabled (no text)
            _chatPage.VerifySendButtonState(false).Should().BeTrue("Send button should be disabled when input is empty");

            // Enter text - button should become enabled
            _chatPage.EnterText(_chatPage.GetMessageInputLocator(), "Test");
            _chatPage.VerifySendButtonState(true).Should().BeTrue("Send button should be enabled when input has text");

            // Clear text - button should become disabled again
            _chatPage.EnterText(_chatPage.GetMessageInputLocator(), "");
            _chatPage.VerifySendButtonState(false).Should().BeTrue("Send button should be disabled when input is cleared");
        }
        catch (Exception ex)
        {
            TakeScreenshot("SendButtonState_Failed");
            throw new Exception($"Send button state test failed: {ex.Message}", ex);
        }
    }

    public override void Dispose()
    {
        _chatPage = null!;
        base.Dispose();
    }
}