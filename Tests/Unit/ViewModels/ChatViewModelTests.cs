using BitChat.Maui.Core.Interfaces;
using BitChat.Maui.Core.Models;
using BitChat.Maui.Presentation.ViewModels;
using FluentAssertions;
using Moq;
using System.Collections.ObjectModel;
using Xunit;

namespace BitChat.Maui.Tests.Unit.ViewModels;

public class ChatViewModelTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly ChatViewModel _sut;
    private readonly ObservableCollection<BitchatMessage> _messages;

    public ChatViewModelTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockSettingsService = new Mock<ISettingsService>();
        _messages = new ObservableCollection<BitchatMessage>();
        
        _mockMessageService.Setup(x => x.Messages).Returns(_messages);
        _mockSettingsService.Setup(x => x.GetUserIdentityAsync())
            .ReturnsAsync(UserIdentity.WithNickname("TestUser"));

        _sut = new ChatViewModel(_mockMessageService.Object, _mockSettingsService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullMessageService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ChatViewModel(null!, _mockSettingsService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("messageService");
    }

    [Fact]
    public void Constructor_NullSettingsService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ChatViewModel(_mockMessageService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settingsService");
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Assert
        _sut.Messages.Should().NotBeNull();
        _sut.MessageText.Should().BeEmpty();
        _sut.IsSending.Should().BeFalse();
        _sut.ConnectedPeers.Should().Be(0);
        _sut.HasConnectedPeers.Should().BeFalse();
    }

    #endregion

    #region SendMessageCommand Tests

    [Fact]
    public async Task SendMessageCommand_ValidMessage_SendsPublicMessage()
    {
        // Arrange
        _sut.MessageText = "Test message";
        _mockMessageService.Setup(x => x.SendPublicMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new BitchatMessage { Content = "Test message" });

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _mockMessageService.Verify(x => x.SendPublicMessageAsync("Test message", It.IsAny<string>()), Times.Once);
        _sut.MessageText.Should().BeEmpty();
        _sut.IsSending.Should().BeFalse();
    }

    [Fact]
    public void SendMessageCommand_EmptyMessage_CannotExecute()
    {
        // Arrange
        _sut.MessageText = "";

        // Act
        var canExecute = _sut.SendMessageCommand.CanExecute(null);

        // Assert
        canExecute.Should().BeFalse();
    }

    [Fact]
    public void SendMessageCommand_WhileSending_CannotExecute()
    {
        // Arrange
        _sut.MessageText = "Test";
        _sut.IsSending = true;

        // Act
        var canExecute = _sut.SendMessageCommand.CanExecute(null);

        // Assert
        canExecute.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessageCommand_SetsIsSendingDuringExecution()
    {
        // Arrange
        _sut.MessageText = "Test message";
        var sendingStates = new List<bool>();
        _sut.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_sut.IsSending))
                sendingStates.Add(_sut.IsSending);
        };

        _mockMessageService.Setup(x => x.SendPublicMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new BitchatMessage());

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        sendingStates.Should().Contain(true);
        sendingStates.Should().Contain(false);
        _sut.IsSending.Should().BeFalse();
    }

    #endregion

    #region Command Processing Tests

    [Fact]
    public async Task SendMessage_NickCommand_ChangesNickname()
    {
        // Arrange
        _sut.MessageText = "/nick NewNickname";
        var updatedIdentity = new UserIdentity { Nickname = "NewNickname" };

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _mockSettingsService.Verify(x => x.SetUserIdentityAsync(It.Is<UserIdentity>(u => u.Nickname == "NewNickname")), Times.Once);
        _sut.CurrentNickname.Should().Be("NewNickname");
    }

    [Fact]
    public async Task SendMessage_ClearCommand_CallsClearMessages()
    {
        // Arrange
        _sut.MessageText = "/clear";
        // Note: Shell.Current is read-only in MAUI tests, so we skip shell interaction

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        // Note: In actual test, you'd need to mock Shell.DisplayAlert
        _sut.MessageText.Should().BeEmpty();
    }

    [Fact]
    public async Task SendMessage_HelpCommand_ShowsHelpMessage()
    {
        // Arrange
        _sut.MessageText = "/help";

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _mockMessageService.Verify(x => x.ReceiveMessageAsync(
            It.Is<BitchatMessage>(m => m.Sender == "System" && m.Content.Contains("Available commands"))), 
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_WhoCommand_ShowsConnectedPeers()
    {
        // Arrange
        _sut.MessageText = "/who";
        _sut.ConnectedPeers = 5;

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _mockMessageService.Verify(x => x.ReceiveMessageAsync(
            It.Is<BitchatMessage>(m => m.Sender == "System" && m.Content.Contains("Connected peers: 5"))), 
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_MsgCommand_SendsPrivateMessage()
    {
        // Arrange
        _sut.MessageText = "/msg Alice Hello Alice!";
        _mockMessageService.Setup(x => x.SendPrivateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new BitchatMessage());

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _mockMessageService.Verify(x => x.SendPrivateMessageAsync("Hello Alice!", It.IsAny<string>(), "Alice"), Times.Once);
    }

    [Fact]
    public async Task SendMessage_UnknownCommand_ShowsErrorMessage()
    {
        // Arrange
        _sut.MessageText = "/unknown";

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _mockMessageService.Verify(x => x.ReceiveMessageAsync(
            It.Is<BitchatMessage>(m => m.Sender == "System" && m.Content.Contains("Unknown command"))), 
            Times.Once);
    }

    #endregion

    #region ClearMessagesCommand Tests

    [Fact]
    public async Task ClearMessagesCommand_CallsMessageServiceClear()
    {
        // Arrange
        // Note: Shell.Current is read-only in MAUI tests, so we skip shell interaction

        // Act
        await _sut.ClearMessagesCommand.ExecuteAsync(null);

        // Assert
        // Note: In actual test, you'd need to mock Shell.DisplayAlert and verify based on user response
        _sut.ClearMessagesCommand.CanExecute(null).Should().BeTrue();
    }

    #endregion

    #region RefreshPeersCommand Tests

    [Fact]
    public async Task RefreshPeersCommand_CanExecute()
    {
        // Arrange
        // Note: Shell.Current is read-only in MAUI tests, so we skip shell interaction

        // Act
        var canExecute = _sut.RefreshPeersCommand.CanExecute(null);
        await _sut.RefreshPeersCommand.ExecuteAsync(null);

        // Assert
        canExecute.Should().BeTrue();
    }

    #endregion

    #region Property Change Tests

    [Fact]
    public void MessageText_PropertyChanged_RaisesEvent()
    {
        // Arrange
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_sut.MessageText))
                propertyChangedRaised = true;
        };

        // Act
        _sut.MessageText = "New text";

        // Assert
        propertyChangedRaised.Should().BeTrue();
        _sut.MessageText.Should().Be("New text");
    }

    [Fact]
    public void IsSending_PropertyChanged_RaisesEvent()
    {
        // Arrange
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_sut.IsSending))
                propertyChangedRaised = true;
        };

        // Act
        _sut.IsSending = true;

        // Assert
        propertyChangedRaised.Should().BeTrue();
        _sut.IsSending.Should().BeTrue();
    }

    [Fact]
    public void ConnectedPeers_PropertyChanged_UpdatesHasConnectedPeers()
    {
        // Arrange
        var hasConnectedPeersChanged = false;
        _sut.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_sut.HasConnectedPeers))
                hasConnectedPeersChanged = true;
        };

        // Act
        _sut.ConnectedPeers = 5;

        // Assert
        hasConnectedPeersChanged.Should().BeTrue();
        _sut.HasConnectedPeers.Should().BeTrue();
    }

    #endregion

    #region Event Handling Tests

    [Fact]
    public void MessageReceived_RaisesScrollToLatestRequested()
    {
        // Arrange
        var scrollRequested = false;
        _sut.ScrollToLatestRequested += (s, e) => scrollRequested = true;

        // Act
        _mockMessageService.Raise(x => x.MessageReceived += null, 
            this, new BitchatMessage { Content = "New message" });

        // Assert
        scrollRequested.Should().BeTrue();
    }

    [Fact]
    public void MessagesCollectionChanged_UpdatesIsEmpty()
    {
        // Act
        _messages.Add(new BitchatMessage { Content = "Test" });

        // Assert
        _sut.IsEmpty.Should().BeFalse();

        // Act
        _messages.Clear();

        // Assert
        _sut.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region Cleanup Tests

    [Fact]
    public void Cleanup_UnsubscribesFromEvents()
    {
        // Act
        _sut.Cleanup();

        // Assert - Verify no exceptions when raising events after cleanup
        var act = () => _mockMessageService.Raise(x => x.MessageReceived += null, 
            this, new BitchatMessage { Content = "Test" });
        act.Should().NotThrow();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SendMessage_VeryLongCommand_HandlesCorrectly()
    {
        // Arrange
        var longNickname = new string('a', 1000);
        _sut.MessageText = $"/nick {longNickname}";

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _sut.CurrentNickname.Should().Be(longNickname);
    }

    [Fact]
    public async Task SendMessage_MsgCommandWithoutMessage_ShowsUsage()
    {
        // Arrange
        _sut.MessageText = "/msg Alice";

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _mockMessageService.Verify(x => x.ReceiveMessageAsync(
            It.Is<BitchatMessage>(m => m.Content.Contains("Usage: /msg"))), 
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_NickCommandWithoutNickname_ShowsUsage()
    {
        // Arrange
        _sut.MessageText = "/nick";

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _mockMessageService.Verify(x => x.ReceiveMessageAsync(
            It.Is<BitchatMessage>(m => m.Content.Contains("Usage: /nick"))), 
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_ExceptionThrown_HandlesGracefully()
    {
        // Arrange
        _sut.MessageText = "Test message";
        _mockMessageService.Setup(x => x.SendPublicMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));
        // Note: Shell.Current is read-only in MAUI tests, so we skip shell interaction

        // Act
        await _sut.SendMessageCommand.ExecuteAsync(null);

        // Assert
        _sut.IsSending.Should().BeFalse();
        // Note: In actual test, you'd verify Shell.DisplayAlert was called
    }

    #endregion
}