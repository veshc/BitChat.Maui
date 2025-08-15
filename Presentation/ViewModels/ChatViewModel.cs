using BitChat.Maui.Core.Interfaces;
using BitChat.Maui.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BitChat.Maui.Presentation.ViewModels;

/// <summary>
/// ViewModel for the main chat interface
/// </summary>
public partial class ChatViewModel : ObservableObject
{
    private readonly IMessageService _messageService;
    private readonly ISettingsService _settingsService;

    /// <summary>
    /// Collection of messages to display
    /// </summary>
    public ObservableCollection<BitchatMessage> Messages => _messageService.Messages;

    /// <summary>
    /// The current message being typed
    /// </summary>
    [ObservableProperty]
    private string _messageText = string.Empty;

    /// <summary>
    /// Indicates if a message is being sent
    /// </summary>
    [ObservableProperty]
    private bool _isSending;

    /// <summary>
    /// Indicates if the chat is empty
    /// </summary>
    [ObservableProperty]
    private bool _isEmpty = true;

    /// <summary>
    /// The current user's nickname
    /// </summary>
    [ObservableProperty]
    private string _currentNickname = string.Empty;

    /// <summary>
    /// Number of connected peers
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConnectedPeers))]
    private int _connectedPeers = 0;

    /// <summary>
    /// Indicates if there are any connected peers
    /// </summary>
    public bool HasConnectedPeers => ConnectedPeers > 0;

    /// <summary>
    /// Event raised when the UI should scroll to the latest message
    /// </summary>
    public event EventHandler? ScrollToLatestRequested;

    /// <summary>
    /// Initializes a new instance of ChatViewModel
    /// </summary>
    public ChatViewModel(IMessageService messageService, ISettingsService settingsService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        // Subscribe to message events
        _messageService.MessageReceived += OnMessageReceived;
        _messageService.MessageStatusChanged += OnMessageStatusChanged;

        // Subscribe to collection changes
        Messages.CollectionChanged += (s, e) =>
        {
            IsEmpty = Messages.Count == 0;
            if (e.NewItems?.Count > 0)
            {
                // Request scroll to latest when new messages are added
                ScrollToLatestRequested?.Invoke(this, EventArgs.Empty);
            }
        };

        // Initialize
        LoadUserNickname();
        IsEmpty = Messages.Count == 0;

        // Add some demo messages for testing
        AddDemoMessages();
    }

    /// <summary>
    /// Safely invokes action on main thread, or synchronously if in test environment
    /// </summary>
    private void SafeInvokeOnMainThread(Action action)
    {
        try
        {
            if (MainThread.IsMainThread)
            {
                action();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(action);
            }
        }
        catch (Exception)
        {
            // In test environment, just execute synchronously
            action();
        }
    }

    /// <summary>
    /// Command to send a message
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText))
            return;

        try
        {
            IsSending = true;

            // Check for commands
            if (MessageText.StartsWith("/"))
            {
                await ProcessCommandAsync(MessageText);
            }
            else
            {
                // Send as public message
                await _messageService.SendPublicMessageAsync(MessageText, CurrentNickname);
            }

            // Clear the input
            MessageText = string.Empty;
        }
        catch (Exception ex)
        {
            // Handle error
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", $"Failed to send message: {ex.Message}", "OK");
        }
        finally
        {
            IsSending = false;
        }
    }

    /// <summary>
    /// Determines if a message can be sent
    /// </summary>
    private bool CanSendMessage()
    {
        return !IsSending && !string.IsNullOrWhiteSpace(MessageText);
    }

    /// <summary>
    /// Command to clear all messages
    /// </summary>
    [RelayCommand]
    private async Task ClearMessagesAsync()
    {
        bool result = true; // Default to true for testing
        if (Shell.Current != null)
        {
            result = await Shell.Current.DisplayAlert(
                "Clear Messages",
                "Are you sure you want to clear all messages?",
                "Yes",
                "No");
        }

        if (result)
        {
            _messageService.ClearMessages();
        }
    }

    /// <summary>
    /// Command to refresh the peer list
    /// </summary>
    [RelayCommand]
    private async Task RefreshPeersAsync()
    {
        // This will be implemented when we add Bluetooth discovery
        await Task.Delay(1000); // Simulate refresh
        
        // For now, just show a message
        if (Shell.Current != null)
            await Shell.Current.DisplayAlert("Info", "Peer discovery will be implemented in the next feature", "OK");
    }

    /// <summary>
    /// Processes chat commands
    /// </summary>
    private async Task ProcessCommandAsync(string command)
    {
        var parts = command.Split(' ', 2);
        var cmd = parts[0].ToLower();
        var args = parts.Length > 1 ? parts[1] : string.Empty;

        switch (cmd)
        {
            case "/nick":
                await ChangeNicknameAsync(args);
                break;

            case "/clear":
                await ClearMessagesAsync();
                break;

            case "/who":
                await ShowConnectedPeersAsync();
                break;

            case "/help":
                await ShowHelpAsync();
                break;

            case "/msg":
                await SendPrivateMessageAsync(args);
                break;

            default:
                await _messageService.ReceiveMessageAsync(new BitchatMessage
                {
                    Sender = "System",
                    Content = $"Unknown command: {cmd}. Type /help for available commands.",
                    Timestamp = DateTime.UtcNow,
                    IsPrivate = false
                });
                break;
        }
    }

    /// <summary>
    /// Changes the user's nickname
    /// </summary>
    private async Task ChangeNicknameAsync(string newNickname)
    {
        if (string.IsNullOrWhiteSpace(newNickname))
        {
            await _messageService.ReceiveMessageAsync(new BitchatMessage
            {
                Sender = "System",
                Content = "Usage: /nick <new_nickname>",
                Timestamp = DateTime.UtcNow,
                IsPrivate = false
            });
            return;
        }

        var oldNickname = CurrentNickname;
        CurrentNickname = newNickname;
        
        // Save to settings
        var newIdentity = UserIdentity.WithNickname(newNickname);
        await _settingsService.SetUserIdentityAsync(newIdentity);

        await _messageService.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "System",
            Content = $"Nickname changed from '{oldNickname}' to '{newNickname}'",
            Timestamp = DateTime.UtcNow,
            IsPrivate = false
        });
    }

    /// <summary>
    /// Shows connected peers
    /// </summary>
    private async Task ShowConnectedPeersAsync()
    {
        var message = ConnectedPeers == 0
            ? "No peers connected. Waiting for connections..."
            : $"Connected peers: {ConnectedPeers}";

        await _messageService.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "System",
            Content = message,
            Timestamp = DateTime.UtcNow,
            IsPrivate = false
        });
    }

    /// <summary>
    /// Shows help information
    /// </summary>
    private async Task ShowHelpAsync()
    {
        var helpText = @"Available commands:
/nick <name> - Change your nickname
/msg <user> <message> - Send private message
/who - List connected peers
/clear - Clear message history
/help - Show this help message";

        await _messageService.ReceiveMessageAsync(new BitchatMessage
        {
            Sender = "System",
            Content = helpText,
            Timestamp = DateTime.UtcNow,
            IsPrivate = false
        });
    }

    /// <summary>
    /// Sends a private message
    /// </summary>
    private async Task SendPrivateMessageAsync(string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2)
        {
            await _messageService.ReceiveMessageAsync(new BitchatMessage
            {
                Sender = "System",
                Content = "Usage: /msg <nickname> <message>",
                Timestamp = DateTime.UtcNow,
                IsPrivate = false
            });
            return;
        }

        var recipient = parts[0];
        var message = parts[1];

        await _messageService.SendPrivateMessageAsync(message, CurrentNickname, recipient);
    }

    /// <summary>
    /// Loads the user's nickname from settings
    /// </summary>
    private async void LoadUserNickname()
    {
        try
        {
            var identity = await _settingsService.GetUserIdentityAsync();
            CurrentNickname = identity.Nickname;
        }
        catch
        {
            CurrentNickname = "Anonymous";
        }
    }

    /// <summary>
    /// Handles message received events
    /// </summary>
    private void OnMessageReceived(object? sender, BitchatMessage e)
    {
        // Scroll to latest when new message received
        SafeInvokeOnMainThread(() =>
        {
            ScrollToLatestRequested?.Invoke(this, EventArgs.Empty);
        });
    }

    /// <summary>
    /// Handles message status changed events
    /// </summary>
    private void OnMessageStatusChanged(object? sender, BitchatMessage e)
    {
        // Update UI if needed
    }

    /// <summary>
    /// Adds demo messages for testing
    /// </summary>
    private async void AddDemoMessages()
    {
        await Task.Delay(500); // Small delay to let UI initialize

        var demoMessages = new[]
        {
            new BitchatMessage
            {
                Sender = "System",
                Content = "Welcome to BitChat! Type /help for available commands.",
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                IsPrivate = false,
                IsOwnMessage = false
            },
            new BitchatMessage
            {
                Sender = "Alice",
                Content = "Hey everyone! 👋",
                Timestamp = DateTime.UtcNow.AddMinutes(-4),
                IsPrivate = false,
                IsOwnMessage = false
            },
            new BitchatMessage
            {
                Sender = "Bob",
                Content = "Hello Alice! How's the mesh network working for you?",
                Timestamp = DateTime.UtcNow.AddMinutes(-3),
                IsPrivate = false,
                IsOwnMessage = false
            },
            new BitchatMessage
            {
                Sender = "Alice",
                Content = "Working great! Love the decentralized approach.",
                Timestamp = DateTime.UtcNow.AddMinutes(-2),
                IsPrivate = false,
                IsOwnMessage = false
            }
        };

        foreach (var msg in demoMessages)
        {
            await _messageService.ReceiveMessageAsync(msg);
            await Task.Delay(100); // Small delay between messages
        }
    }

    /// <summary>
    /// Cleans up resources
    /// </summary>
    public void Cleanup()
    {
        _messageService.MessageReceived -= OnMessageReceived;
        _messageService.MessageStatusChanged -= OnMessageStatusChanged;
    }
}