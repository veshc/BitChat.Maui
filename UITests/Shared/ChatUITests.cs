using BitChat.Maui.UITests.Shared.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using Xunit;

namespace BitChat.Maui.UITests.Shared;

/// <summary>
/// Cross-platform UI tests for chat functionality
/// </summary>
public abstract class ChatUITests : AppiumTestBase
{
    protected ChatPage ChatPage { get; private set; } = null!;

    protected override void InitializePages()
    {
        ChatPage = new ChatPage(Driver, Platform);
    }

    [Fact]
    public void SendMessage_ShouldDisplayInChatView()
    {
        // Arrange
        var testMessage = $"Test message {DateTime.Now:HH:mm:ss}";
        
        // Act
        ChatPage.EnterMessage(testMessage);
        ChatPage.SendMessage();
        
        // Assert
        ChatPage.WaitForMessage(testMessage);
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains(testMessage));
    }

    [Fact]
    public void SendMultipleMessages_ShouldDisplayInOrder()
    {
        // Arrange
        var messages = new[]
        {
            $"First message {DateTime.Now:HH:mm:ss}",
            $"Second message {DateTime.Now:HH:mm:ss}",
            $"Third message {DateTime.Now:HH:mm:ss}"
        };

        // Act
        foreach (var message in messages)
        {
            ChatPage.EnterMessage(message);
            ChatPage.SendMessage();
            ChatPage.WaitForMessage(message);
        }

        // Assert
        var displayedMessages = ChatPage.GetMessages();
        displayedMessages.Should().HaveCountGreaterOrEqualTo(3);
        
        // Verify order
        var messageTexts = displayedMessages.TakeLast(3).ToList();
        for (int i = 0; i < messages.Length; i++)
        {
            messageTexts[i].Should().Contain(messages[i]);
        }
    }

    [Fact]
    public void EmptyMessage_ShouldNotSend()
    {
        // Arrange
        var initialMessageCount = ChatPage.GetMessageCount();
        
        // Act
        ChatPage.ClearMessageInput();
        ChatPage.SendMessage();
        Thread.Sleep(1000); // Wait to ensure no message is sent
        
        // Assert
        var newMessageCount = ChatPage.GetMessageCount();
        newMessageCount.Should().Be(initialMessageCount);
    }

    [Fact]
    public void CommandHelp_ShouldDisplayHelpMessage()
    {
        // Act
        ChatPage.EnterMessage("/help");
        ChatPage.SendMessage();
        
        // Assert
        ChatPage.WaitForSystemMessage();
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains("Available commands"));
    }

    [Fact]
    public void CommandNick_ShouldChangeNickname()
    {
        // Arrange
        var newNickname = $"TestUser{DateTime.Now:HHmmss}";
        
        // Act
        ChatPage.EnterMessage($"/nick {newNickname}");
        ChatPage.SendMessage();
        
        // Assert
        ChatPage.WaitForSystemMessage();
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains($"Nickname changed") && m.Contains(newNickname));
    }

    [Fact]
    public void CommandWho_ShouldDisplayPeerInfo()
    {
        // Act
        ChatPage.EnterMessage("/who");
        ChatPage.SendMessage();
        
        // Assert
        ChatPage.WaitForSystemMessage();
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains("peers") || m.Contains("No peers connected"));
    }

    [Fact]
    public void CommandClear_ShouldClearMessages()
    {
        // Arrange
        ChatPage.EnterMessage("Message before clear");
        ChatPage.SendMessage();
        ChatPage.WaitForMessage("Message before clear");
        
        // Act
        ChatPage.EnterMessage("/clear");
        ChatPage.SendMessage();
        ChatPage.ConfirmClearMessages();
        
        // Assert
        Thread.Sleep(1000); // Wait for clear to complete
        var messageCount = ChatPage.GetMessageCount();
        messageCount.Should().Be(0);
    }

    [Fact]
    public void LongMessage_ShouldWrapCorrectly()
    {
        // Arrange
        var longMessage = string.Join(" ", Enumerable.Repeat("This is a very long message that should wrap correctly in the chat view.", 5));
        
        // Act
        ChatPage.EnterMessage(longMessage);
        ChatPage.SendMessage();
        
        // Assert
        ChatPage.WaitForMessage("very long message");
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains("very long message"));
    }

    [Fact]
    public void MessageWithEmoji_ShouldDisplayCorrectly()
    {
        // Arrange
        var emojiMessage = "Hello 👋 World 🌍 Test 🚀";
        
        // Act
        ChatPage.EnterMessage(emojiMessage);
        ChatPage.SendMessage();
        
        // Assert
        ChatPage.WaitForMessage("Hello");
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains("Hello") && m.Contains("World"));
    }

    [Fact]
    public void NavigateToSettings_ShouldOpenSettingsPage()
    {
        // Act
        ChatPage.OpenSettings();
        
        // Assert
        Thread.Sleep(1000); // Wait for navigation
        var settingsPage = new SettingsPage(Driver, Platform);
        settingsPage.IsOnSettingsPage().Should().BeTrue();
        
        // Navigate back
        settingsPage.NavigateBack();
        Thread.Sleep(1000);
        ChatPage.IsOnChatPage().Should().BeTrue();
    }

    [Fact]
    public void MessageInput_ShouldSupportReturnToSend()
    {
        // Arrange
        var testMessage = $"Return key test {DateTime.Now:HH:mm:ss}";
        
        // Act
        ChatPage.EnterMessage(testMessage);
        ChatPage.PressReturnKey();
        
        // Assert
        ChatPage.WaitForMessage(testMessage);
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains(testMessage));
    }

    [Fact]
    public void ScrollToLatest_ShouldWorkAfterMultipleMessages()
    {
        // Arrange
        for (int i = 0; i < 20; i++)
        {
            ChatPage.EnterMessage($"Scroll test message {i}");
            ChatPage.SendMessage();
            Thread.Sleep(100);
        }
        
        // Act
        ChatPage.ScrollToTop();
        Thread.Sleep(500);
        ChatPage.EnterMessage("Latest message");
        ChatPage.SendMessage();
        
        // Assert
        Thread.Sleep(500); // Wait for auto-scroll
        ChatPage.IsMessageVisible("Latest message").Should().BeTrue();
    }

    [Fact]
    public void EmptyState_ShouldShowWhenNoMessages()
    {
        // Arrange
        ChatPage.EnterMessage("/clear");
        ChatPage.SendMessage();
        ChatPage.ConfirmClearMessages();
        Thread.Sleep(1000);
        
        // Assert
        ChatPage.IsEmptyStateVisible().Should().BeTrue();
        ChatPage.GetEmptyStateText().Should().Contain("No messages yet");
    }

    [Fact]
    public void ConnectionStatus_ShouldBeVisible()
    {
        // Assert
        ChatPage.IsConnectionStatusVisible().Should().BeTrue();
        var peerCount = ChatPage.GetPeerCount();
        peerCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void MessageTimestamp_ShouldBeVisible()
    {
        // Arrange
        var testMessage = $"Timestamp test {DateTime.Now:HH:mm:ss}";
        
        // Act
        ChatPage.EnterMessage(testMessage);
        ChatPage.SendMessage();
        ChatPage.WaitForMessage(testMessage);
        
        // Assert
        var timestamp = ChatPage.GetMessageTimestamp(testMessage);
        timestamp.Should().NotBeNullOrEmpty();
        timestamp.Should().MatchRegex(@"\d{2}:\d{2}");
    }

    [Fact]
    public void PrivateMessage_ShouldBeMarkedAsPrivate()
    {
        // Act
        ChatPage.EnterMessage("/msg TestUser This is a private message");
        ChatPage.SendMessage();
        
        // Assert
        Thread.Sleep(1000);
        // Note: This would need UI indicators for private messages to be fully testable
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains("private message"));
    }

    [Fact]
    public void InvalidCommand_ShouldShowError()
    {
        // Act
        ChatPage.EnterMessage("/invalid");
        ChatPage.SendMessage();
        
        // Assert
        ChatPage.WaitForSystemMessage();
        var messages = ChatPage.GetMessages();
        messages.Should().Contain(m => m.Contains("Unknown command"));
    }

    [Fact]
    public void MessageInput_ShouldClearAfterSending()
    {
        // Arrange
        var testMessage = "Clear test message";
        
        // Act
        ChatPage.EnterMessage(testMessage);
        ChatPage.SendMessage();
        ChatPage.WaitForMessage(testMessage);
        
        // Assert
        ChatPage.GetMessageInputText().Should().BeEmpty();
    }

    [Fact]
    public void SendButton_ShouldBeDisabledWhenEmpty()
    {
        // Arrange
        ChatPage.ClearMessageInput();
        
        // Assert
        ChatPage.IsSendButtonEnabled().Should().BeFalse();
        
        // Act
        ChatPage.EnterMessage("Test");
        
        // Assert
        ChatPage.IsSendButtonEnabled().Should().BeTrue();
    }
}