using BitChat.Maui.Core.Models;
using BitChat.Maui.Core.Services;
using FluentAssertions;
using Xunit;

namespace BitChat.Maui.Tests.Unit.Services;

public class MessageServiceTests
{
    private readonly MessageService _sut;

    public MessageServiceTests()
    {
        _sut = new MessageService(maxMessages: 10);
    }

    #region SendPublicMessageAsync Tests

    [Fact]
    public async Task SendPublicMessageAsync_ValidInput_ReturnsMessage()
    {
        // Arrange
        var content = "Test message";
        var sender = "TestUser";

        // Act
        var result = await _sut.SendPublicMessageAsync(content, sender);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(content);
        result.Sender.Should().Be(sender);
        result.IsPrivate.Should().BeFalse();
        result.IsOwnMessage.Should().BeTrue();
        result.Status.Should().Be(MessageStatus.Sent);
    }

    [Fact]
    public async Task SendPublicMessageAsync_AddsMessageToCollection()
    {
        // Arrange
        var content = "Test message";
        var sender = "TestUser";

        // Act
        await _sut.SendPublicMessageAsync(content, sender);

        // Assert
        _sut.Messages.Should().HaveCount(1);
        _sut.Messages.First().Content.Should().Be(content);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendPublicMessageAsync_EmptyContent_ThrowsArgumentException(string? content)
    {
        // Arrange
        var sender = "TestUser";

        // Act
        var act = async () => await _sut.SendPublicMessageAsync(content!, sender);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("content");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendPublicMessageAsync_EmptySender_ThrowsArgumentException(string? sender)
    {
        // Arrange
        var content = "Test message";

        // Act
        var act = async () => await _sut.SendPublicMessageAsync(content, sender!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("senderNickname");
    }

    #endregion

    #region SendPrivateMessageAsync Tests

    [Fact]
    public async Task SendPrivateMessageAsync_ValidInput_ReturnsPrivateMessage()
    {
        // Arrange
        var content = "Private message";
        var sender = "TestUser";
        var recipient = "RecipientUser";

        // Act
        var result = await _sut.SendPrivateMessageAsync(content, sender, recipient);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(content);
        result.Sender.Should().Be(sender);
        result.RecipientNickname.Should().Be(recipient);
        result.IsPrivate.Should().BeTrue();
        result.IsOwnMessage.Should().BeTrue();
        result.Status.Should().Be(MessageStatus.Sent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendPrivateMessageAsync_EmptyRecipient_ThrowsArgumentException(string? recipient)
    {
        // Arrange
        var content = "Test message";
        var sender = "TestUser";

        // Act
        var act = async () => await _sut.SendPrivateMessageAsync(content, sender, recipient!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("recipientNickname");
    }

    #endregion

    #region ReceiveMessageAsync Tests

    [Fact]
    public async Task ReceiveMessageAsync_ValidMessage_AddsToCollection()
    {
        // Arrange
        var message = new BitchatMessage
        {
            Id = "123",
            Sender = "OtherUser",
            Content = "Received message",
            IsPrivate = false
        };

        // Act
        await _sut.ReceiveMessageAsync(message);

        // Assert
        _sut.Messages.Should().HaveCount(1);
        _sut.Messages.First().IsOwnMessage.Should().BeFalse();
        _sut.Messages.First().Status.Should().Be(MessageStatus.Delivered);
    }

    [Fact]
    public async Task ReceiveMessageAsync_NullMessage_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await _sut.ReceiveMessageAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task ReceiveMessageAsync_RaisesMessageReceivedEvent()
    {
        // Arrange
        var message = new BitchatMessage
        {
            Sender = "OtherUser",
            Content = "Test"
        };
        BitchatMessage? receivedMessage = null;
        _sut.MessageReceived += (s, e) => receivedMessage = e;

        // Act
        await _sut.ReceiveMessageAsync(message);

        // Assert
        receivedMessage.Should().NotBeNull();
        receivedMessage!.Content.Should().Be("Test");
    }

    #endregion

    #region UpdateMessageStatusAsync Tests

    [Fact]
    public async Task UpdateMessageStatusAsync_ValidId_UpdatesStatus()
    {
        // Arrange
        var message = await _sut.SendPublicMessageAsync("Test", "User");
        var messageId = message.Id;

        // Act
        await _sut.UpdateMessageStatusAsync(messageId, MessageStatus.Delivered);

        // Assert
        var updatedMessage = _sut.Messages.FirstOrDefault(m => m.Id == messageId);
        updatedMessage.Should().NotBeNull();
        updatedMessage!.Status.Should().Be(MessageStatus.Delivered);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateMessageStatusAsync_EmptyId_ThrowsArgumentException(string? messageId)
    {
        // Act
        var act = async () => await _sut.UpdateMessageStatusAsync(messageId!, MessageStatus.Delivered);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("messageId");
    }

    [Fact]
    public async Task UpdateMessageStatusAsync_NonExistentId_DoesNotThrow()
    {
        // Act
        var act = async () => await _sut.UpdateMessageStatusAsync("non-existent", MessageStatus.Delivered);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateMessageStatusAsync_RaisesStatusChangedEvent()
    {
        // Arrange
        var message = await _sut.SendPublicMessageAsync("Test", "User");
        BitchatMessage? changedMessage = null;
        _sut.MessageStatusChanged += (s, e) => changedMessage = e;

        // Act
        await _sut.UpdateMessageStatusAsync(message.Id, MessageStatus.Read);

        // Assert
        changedMessage.Should().NotBeNull();
        changedMessage!.Status.Should().Be(MessageStatus.Read);
    }

    #endregion

    #region ClearMessages Tests

    [Fact]
    public async Task ClearMessages_RemovesAllMessages()
    {
        // Arrange
        await _sut.SendPublicMessageAsync("Message 1", "User");
        await _sut.SendPublicMessageAsync("Message 2", "User");
        await _sut.SendPublicMessageAsync("Message 3", "User");

        // Act
        _sut.ClearMessages();

        // Assert
        _sut.Messages.Should().BeEmpty();
    }

    #endregion

    #region GetMessages Tests

    [Fact]
    public async Task GetMessages_PublicMessages_ReturnsOnlyPublicMessages()
    {
        // Arrange
        await _sut.SendPublicMessageAsync("Public 1", "User");
        await _sut.SendPrivateMessageAsync("Private 1", "User", "Other");
        await _sut.SendPublicMessageAsync("Public 2", "User");

        // Act
        var publicMessages = _sut.GetMessages(isPrivate: false).ToList();

        // Assert
        publicMessages.Should().HaveCount(2);
        publicMessages.Should().AllSatisfy(m => m.IsPrivate.Should().BeFalse());
    }

    [Fact]
    public async Task GetMessages_PrivateMessages_ReturnsOnlyPrivateMessages()
    {
        // Arrange
        await _sut.SendPublicMessageAsync("Public 1", "User");
        await _sut.SendPrivateMessageAsync("Private 1", "User", "Other");
        await _sut.SendPrivateMessageAsync("Private 2", "User", "Another");

        // Act
        var privateMessages = _sut.GetMessages(isPrivate: true).ToList();

        // Assert
        privateMessages.Should().HaveCount(2);
        privateMessages.Should().AllSatisfy(m => m.IsPrivate.Should().BeTrue());
    }

    [Fact]
    public async Task GetMessages_SpecificPeer_ReturnsConversationWithPeer()
    {
        // Arrange
        await _sut.SendPrivateMessageAsync("To Alice", "User", "Alice");
        await _sut.SendPrivateMessageAsync("To Bob", "User", "Bob");
        await _sut.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Alice",
            Content = "From Alice",
            IsPrivate = true,
            RecipientNickname = "User"
        });

        // Act
        var aliceMessages = _sut.GetMessages(isPrivate: true, peerNickname: "Alice").ToList();

        // Assert
        aliceMessages.Should().HaveCount(2);
        aliceMessages.Should().Contain(m => m.Content == "To Alice");
        aliceMessages.Should().Contain(m => m.Content == "From Alice");
    }

    #endregion

    #region GetUnreadCount Tests

    [Fact]
    public async Task GetUnreadCount_PublicMessages_ReturnsCorrectCount()
    {
        // Arrange
        await _sut.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Other1",
            Content = "Message 1",
            IsPrivate = false,
            Status = MessageStatus.Delivered
        });
        await _sut.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Other2",
            Content = "Message 2",
            IsPrivate = false,
            Status = MessageStatus.Read
        });

        // Act
        var unreadCount = _sut.GetUnreadCount(isPrivate: false);

        // Assert
        unreadCount.Should().Be(1);
    }

    [Fact]
    public async Task GetUnreadCount_PrivateMessagesFromSpecificPeer_ReturnsCorrectCount()
    {
        // Arrange
        await _sut.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Alice",
            Content = "Message 1",
            IsPrivate = true,
            Status = MessageStatus.Delivered
        });
        await _sut.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Bob",
            Content = "Message 2",
            IsPrivate = true,
            Status = MessageStatus.Delivered
        });

        // Act
        var aliceUnreadCount = _sut.GetUnreadCount(isPrivate: true, peerNickname: "Alice");

        // Assert
        aliceUnreadCount.Should().Be(1);
    }

    [Fact]
    public async Task GetUnreadCount_ExcludesOwnMessages()
    {
        // Arrange
        await _sut.SendPublicMessageAsync("My message", "User");
        await _sut.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Other",
            Content = "Their message",
            IsPrivate = false
        });

        // Act
        var unreadCount = _sut.GetUnreadCount(isPrivate: false);

        // Assert
        unreadCount.Should().Be(1);
    }

    #endregion

    #region Message Limit Tests

    [Fact]
    public async Task Messages_ExceedingLimit_RemovesOldestMessages()
    {
        // Arrange - Create service with limit of 5 messages
        var limitedService = new MessageService(maxMessages: 5);

        // Act - Add 7 messages
        for (int i = 1; i <= 7; i++)
        {
            await limitedService.SendPublicMessageAsync($"Message {i}", "User");
        }

        // Assert
        limitedService.Messages.Should().HaveCount(5);
        limitedService.Messages.First().Content.Should().Be("Message 3");
        limitedService.Messages.Last().Content.Should().Be("Message 7");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SendMessage_WithUnicodeContent_HandlesCorrectly()
    {
        // Arrange
        var content = "Test 🚀 emoji 日本語 characters";
        var sender = "User";

        // Act
        var result = await _sut.SendPublicMessageAsync(content, sender);

        // Assert
        result.Content.Should().Be(content);
        _sut.Messages.First().Content.Should().Be(content);
    }

    [Fact]
    public async Task SendMessage_VeryLongContent_HandlesCorrectly()
    {
        // Arrange
        var content = new string('a', 10000);
        var sender = "User";

        // Act
        var result = await _sut.SendPublicMessageAsync(content, sender);

        // Assert
        result.Content.Should().Be(content);
        result.Content.Length.Should().Be(10000);
    }

    [Fact]
    public async Task ConcurrentMessageAddition_HandlesCorrectly()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                await _sut.SendPublicMessageAsync($"Message {index}", $"User{index}");
            }));
        }
        await Task.WhenAll(tasks);

        // Assert
        _sut.Messages.Should().HaveCount(10);
    }

    #endregion
}