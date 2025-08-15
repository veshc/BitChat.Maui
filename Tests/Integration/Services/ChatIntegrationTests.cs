using BitChat.Maui.Core.Interfaces;
using BitChat.Maui.Core.Models;
using BitChat.Maui.Core.Services;
using BitChat.Maui.Presentation.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BitChat.Maui.Tests.Integration.Services;

public class ChatIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageService _messageService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly ChatViewModel _chatViewModel;

    public ChatIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Register logging
        services.AddLogging();
        
        // Mock settings service since it depends on platform-specific Preferences
        _mockSettingsService = new Mock<ISettingsService>();
        _mockSettingsService.Setup(x => x.GetUserIdentityAsync())
            .ReturnsAsync(UserIdentity.WithNickname("TestUser"));
        
        // Register services
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<ISettingsService>(_mockSettingsService.Object);
        services.AddTransient<ChatViewModel>();

        _serviceProvider = services.BuildServiceProvider();
        _messageService = _serviceProvider.GetRequiredService<IMessageService>();
        _chatViewModel = _serviceProvider.GetRequiredService<ChatViewModel>();
    }

    [Fact]
    public async Task SendMessage_ThroughViewModel_AppearsInMessageService()
    {
        // Arrange
        _chatViewModel.MessageText = "Integration test message";

        // Act
        await _chatViewModel.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _chatViewModel.Messages.Should().ContainSingle(m => m.Content == "Integration test message");
        var sentMessage = _chatViewModel.Messages.Single(m => m.Content == "Integration test message");
        sentMessage.Sender.Should().Be("TestUser");
        sentMessage.IsOwnMessage.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeNickname_UpdatesInAllServices()
    {
        // Arrange
        _mockSettingsService.Setup(x => x.SetUserIdentityAsync(It.IsAny<UserIdentity>())).Returns(Task.CompletedTask);
        _mockSettingsService.Setup(x => x.GetUserIdentityAsync()).ReturnsAsync(UserIdentity.WithNickname("NewNick"));
        var viewModel = new ChatViewModel(_messageService, _mockSettingsService.Object);

        // Act
        viewModel.MessageText = "/nick NewNick";
        await viewModel.SendMessageCommand.ExecuteAsync(null);

        // Assert
        viewModel.CurrentNickname.Should().Be("NewNick");
        _mockSettingsService.Verify(x => x.SetUserIdentityAsync(It.Is<UserIdentity>(u => u.Nickname == "NewNick")), Times.Once);
    }

    [Fact]
    public async Task MessageFlow_FromReceiveToViewModel()
    {
        // Arrange
        var initialCount = _chatViewModel.Messages.Count;
        var incomingMessage = new BitchatMessage
        {
            Sender = "ExternalUser",
            Content = "Incoming message",
            IsPrivate = false
        };

        // Act
        await _messageService.ReceiveMessageAsync(incomingMessage);

        // Assert
        _chatViewModel.Messages.Count.Should().Be(initialCount + 1);
        var receivedMessage = _chatViewModel.Messages.Last();
        receivedMessage.Sender.Should().Be("ExternalUser");
        receivedMessage.Content.Should().Be("Incoming message");
        receivedMessage.IsOwnMessage.Should().BeFalse();
        receivedMessage.Status.Should().Be(MessageStatus.Delivered);
    }

    [Fact]
    public async Task PrivateMessage_ThroughCommand_CreatesCorrectMessage()
    {
        // Arrange
        _mockSettingsService.Setup(x => x.GetUserIdentityAsync()).ReturnsAsync(UserIdentity.WithNickname("Sender"));
        _chatViewModel.MessageText = "/msg Recipient Hello there!";

        // Act
        await _chatViewModel.SendMessageCommand.ExecuteAsync(null);

        // Assert
        var privateMessages = _messageService.GetMessages(isPrivate: true).ToList();
        privateMessages.Should().HaveCountGreaterThan(0);
        var sentMessage = privateMessages.Last();
        sentMessage.IsPrivate.Should().BeTrue();
        sentMessage.RecipientNickname.Should().Be("Recipient");
        sentMessage.Content.Should().Be("Hello there!");
        sentMessage.Sender.Should().Be("TestUser");
    }

    [Fact]
    public async Task ClearMessages_RemovesAllFromBothLayers()
    {
        // Arrange
        await _messageService.SendPublicMessageAsync("Message 1", "User");
        await _messageService.SendPublicMessageAsync("Message 2", "User");
        await _messageService.SendPublicMessageAsync("Message 3", "User");
        var viewModel = new ChatViewModel(_messageService, _mockSettingsService.Object);

        // Act
        _messageService.ClearMessages();

        // Assert
        _messageService.Messages.Should().BeEmpty();
        viewModel.Messages.Should().BeEmpty();
        viewModel.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task MessageStatusUpdate_PropagatesCorrectly()
    {
        // Arrange
        var message = await _messageService.SendPublicMessageAsync("Test", "User");
        var messageId = message.Id;

        // Act
        await _messageService.UpdateMessageStatusAsync(messageId, MessageStatus.Delivered);
        await Task.Delay(100); // Allow for async propagation

        // Assert
        var updatedMessage = _chatViewModel.Messages.FirstOrDefault(m => m.Id == messageId);
        updatedMessage.Should().NotBeNull();
        updatedMessage!.Status.Should().Be(MessageStatus.Delivered);
    }

    [Fact]
    public async Task UnreadCount_UpdatesCorrectlyAcrossServices()
    {
        // Arrange
        await _messageService.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Other1",
            Content = "Message 1",
            IsPrivate = false
        });
        await _messageService.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Other2",
            Content = "Message 2",
            IsPrivate = false
        });

        // Act
        var unreadCount = _messageService.GetUnreadCount(isPrivate: false);

        // Assert
        unreadCount.Should().Be(2);
        
        // Mark one as read
        var firstUnread = _messageService.Messages.First(m => !m.IsOwnMessage && m.Status != MessageStatus.Read);
        await _messageService.UpdateMessageStatusAsync(firstUnread.Id, MessageStatus.Read);
        
        var updatedUnreadCount = _messageService.GetUnreadCount(isPrivate: false);
        updatedUnreadCount.Should().Be(1);
    }

    [Fact]
    public async Task ConcurrentMessaging_HandlesCorrectly()
    {
        // Arrange
        var tasks = new List<Task>();
        var messageCount = 20;

        // Act
        for (int i = 0; i < messageCount; i++)
        {
            var index = i;
            if (i % 2 == 0)
            {
                tasks.Add(_messageService.SendPublicMessageAsync($"Message {index}", $"User{index}"));
            }
            else
            {
                tasks.Add(_messageService.ReceiveMessageAsync(new BitchatMessage
                {
                    Sender = $"External{index}",
                    Content = $"Received {index}"
                }));
            }
        }
        
        await Task.WhenAll(tasks);

        // Assert
        _messageService.Messages.Should().HaveCountGreaterOrEqualTo(messageCount);
        _chatViewModel.Messages.Should().HaveCountGreaterOrEqualTo(messageCount);
    }

    [Fact]
    public async Task MessageLimitEnforcement_WorksAcrossLayers()
    {
        // Arrange
        var limitedMessageService = new MessageService(maxMessages: 5);
        var viewModel = new ChatViewModel(limitedMessageService, _mockSettingsService.Object);

        // Act
        for (int i = 0; i < 10; i++)
        {
            await limitedMessageService.SendPublicMessageAsync($"Message {i}", "User");
        }

        // Assert
        limitedMessageService.Messages.Should().HaveCount(5);
        viewModel.Messages.Should().HaveCount(5);
        
        // Find the messages we sent (excluding any demo messages)
        var sentMessages = viewModel.Messages.Where(m => m.Content.StartsWith("Message ")).ToList();
        sentMessages.Should().HaveCountGreaterOrEqualTo(3); // At least some of our messages should be there
        
        // The last message should be Message 9 (the last one we sent)
        var lastSentMessage = sentMessages.Last();
        lastSentMessage.Content.Should().Be("Message 9");
    }

    [Fact]
    public async Task PrivateConversationFiltering_WorksCorrectly()
    {
        // Arrange
        await _messageService.SendPublicMessageAsync("Public 1", "User");
        await _messageService.SendPrivateMessageAsync("To Alice", "User", "Alice");
        await _messageService.SendPrivateMessageAsync("To Bob", "User", "Bob");
        await _messageService.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "Alice",
            Content = "From Alice",
            IsPrivate = true,
            RecipientNickname = "User"
        });

        // Act
        var aliceConversation = _messageService.GetMessages(isPrivate: true, peerNickname: "Alice");
        var bobConversation = _messageService.GetMessages(isPrivate: true, peerNickname: "Bob");
        var publicMessages = _messageService.GetMessages(isPrivate: false);

        // Assert
        aliceConversation.Should().HaveCount(2);
        aliceConversation.Should().Contain(m => m.Content == "To Alice");
        aliceConversation.Should().Contain(m => m.Content == "From Alice");
        
        bobConversation.Should().HaveCount(1);
        bobConversation.Should().Contain(m => m.Content == "To Bob");
        
        publicMessages.Should().HaveCountGreaterOrEqualTo(1);
        publicMessages.Should().Contain(m => m.Content == "Public 1");
    }

    [Fact]
    public async Task SettingsIntegration_NicknamePersistedAcrossViewModels()
    {
        // Arrange
        _mockSettingsService.Setup(x => x.GetUserIdentityAsync()).ReturnsAsync(UserIdentity.WithNickname("PersistentNick"));

        // Act
        var viewModel1 = new ChatViewModel(_messageService, _mockSettingsService.Object);
        await Task.Delay(100); // Allow async initialization
        
        var viewModel2 = new ChatViewModel(_messageService, _mockSettingsService.Object);
        await Task.Delay(100); // Allow async initialization

        // Assert
        viewModel1.CurrentNickname.Should().Be("PersistentNick");
        viewModel2.CurrentNickname.Should().Be("PersistentNick");
    }

    [Fact]
    public async Task MessageEventPropagation_WorksAcrossMultipleViewModels()
    {
        // Arrange
        var viewModel1 = new ChatViewModel(_messageService, _mockSettingsService.Object);
        var viewModel2 = new ChatViewModel(_messageService, _mockSettingsService.Object);
        
        var vm1ScrollRequested = false;
        var vm2ScrollRequested = false;
        
        viewModel1.ScrollToLatestRequested += (s, e) => vm1ScrollRequested = true;
        viewModel2.ScrollToLatestRequested += (s, e) => vm2ScrollRequested = true;

        // Act
        await _messageService.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "External",
            Content = "Broadcast message"
        });

        // Assert
        vm1ScrollRequested.Should().BeTrue();
        vm2ScrollRequested.Should().BeTrue();
        viewModel1.Messages.Last().Content.Should().Be("Broadcast message");
        viewModel2.Messages.Last().Content.Should().Be("Broadcast message");
    }

    [Fact]
    public async Task ErrorHandling_ServiceExceptionDoesNotCrashViewModel()
    {
        // Arrange
        var mockMessageService = new Mock<IMessageService>();
        mockMessageService.Setup(x => x.Messages).Returns(new System.Collections.ObjectModel.ObservableCollection<BitchatMessage>());
        mockMessageService.Setup(x => x.SendPublicMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service error"));
        
        var viewModel = new ChatViewModel(mockMessageService.Object, _mockSettingsService.Object);
        viewModel.MessageText = "Test message";
        // Note: Shell.Current is read-only in MAUI tests, so we skip shell interaction

        // Act
        var act = async () => await viewModel.SendMessageCommand.ExecuteAsync(null);

        // Assert
        await act.Should().NotThrowAsync();
        viewModel.IsSending.Should().BeFalse();
    }
}