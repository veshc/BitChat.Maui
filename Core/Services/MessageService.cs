using BitChat.Maui.Core.Interfaces;
using BitChat.Maui.Core.Models;
using System.Collections.ObjectModel;

namespace BitChat.Maui.Core.Services;

/// <summary>
/// Service implementation for managing chat messages in memory
/// </summary>
public class MessageService : IMessageService
{
    private readonly ObservableCollection<BitchatMessage> _messages;
    private readonly int _maxMessages;
    private readonly object _messageLock = new();

    /// <summary>
    /// Observable collection of all messages
    /// </summary>
    public ObservableCollection<BitchatMessage> Messages => _messages;

    /// <summary>
    /// Event raised when a new message is received
    /// </summary>
    public event EventHandler<BitchatMessage>? MessageReceived;

    /// <summary>
    /// Event raised when a message status changes
    /// </summary>
    public event EventHandler<BitchatMessage>? MessageStatusChanged;

    /// <summary>
    /// Initializes a new instance of the MessageService
    /// </summary>
    /// <param name="maxMessages">Maximum number of messages to keep in memory (default: 1337)</param>
    public MessageService(int maxMessages = 1337)
    {
        _maxMessages = maxMessages;
        _messages = new ObservableCollection<BitchatMessage>();
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
    /// Sends a public message to all connected peers
    /// </summary>
    public async Task<BitchatMessage> SendPublicMessageAsync(string content, string senderNickname)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty", nameof(content));

        if (string.IsNullOrWhiteSpace(senderNickname))
            throw new ArgumentException("Sender nickname cannot be empty", nameof(senderNickname));

        var message = new BitchatMessage
        {
            Id = Guid.NewGuid().ToString(),
            Sender = senderNickname,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsPrivate = false,
            IsRelay = false,
            Status = MessageStatus.Sending,
            IsOwnMessage = true
        };

        await AddMessageAsync(message);

        // Simulate async sending
        await Task.Delay(100);

        // Update status to sent
        await UpdateMessageStatusAsync(message.Id, MessageStatus.Sent);

        // Return the updated message with correct status
        return message with { Status = MessageStatus.Sent };
    }

    /// <summary>
    /// Sends a private message to a specific recipient
    /// </summary>
    public async Task<BitchatMessage> SendPrivateMessageAsync(string content, string senderNickname, string recipientNickname)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty", nameof(content));

        if (string.IsNullOrWhiteSpace(senderNickname))
            throw new ArgumentException("Sender nickname cannot be empty", nameof(senderNickname));

        if (string.IsNullOrWhiteSpace(recipientNickname))
            throw new ArgumentException("Recipient nickname cannot be empty", nameof(recipientNickname));

        var message = new BitchatMessage
        {
            Id = Guid.NewGuid().ToString(),
            Sender = senderNickname,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsPrivate = true,
            IsRelay = false,
            RecipientNickname = recipientNickname,
            Status = MessageStatus.Sending,
            IsOwnMessage = true
        };

        await AddMessageAsync(message);

        // Simulate async sending
        await Task.Delay(100);

        // Update status to sent
        await UpdateMessageStatusAsync(message.Id, MessageStatus.Sent);

        // Return the updated message with correct status
        return message with { Status = MessageStatus.Sent };
    }

    /// <summary>
    /// Receives a message from another peer
    /// </summary>
    public async Task ReceiveMessageAsync(BitchatMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        var receivedMessage = message with
        {
            IsOwnMessage = false,
            Status = message.Status == MessageStatus.Read ? MessageStatus.Read : MessageStatus.Delivered
        };

        await AddMessageAsync(receivedMessage);
        MessageReceived?.Invoke(this, receivedMessage);
    }

    /// <summary>
    /// Updates the status of a message
    /// </summary>
    public async Task UpdateMessageStatusAsync(string messageId, MessageStatus status)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be empty", nameof(messageId));

        await Task.Run(() =>
        {
            lock (_messageLock)
            {
                var message = _messages.FirstOrDefault(m => m.Id == messageId);
                if (message != null)
                {
                    var index = _messages.IndexOf(message);
                    var updatedMessage = message with { Status = status };
                    
                    // Update on UI thread
                    SafeInvokeOnMainThread(() =>
                    {
                        _messages[index] = updatedMessage;
                        MessageStatusChanged?.Invoke(this, updatedMessage);
                    });
                }
            }
        });
    }

    /// <summary>
    /// Clears all messages from memory
    /// </summary>
    public void ClearMessages()
    {
        lock (_messageLock)
        {
            SafeInvokeOnMainThread(() => _messages.Clear());
        }
    }

    /// <summary>
    /// Gets messages for a specific conversation
    /// </summary>
    public IEnumerable<BitchatMessage> GetMessages(bool isPrivate = false, string? peerNickname = null)
    {
        lock (_messageLock)
        {
            if (!isPrivate)
            {
                // Return public messages
                return _messages.Where(m => !m.IsPrivate).ToList();
            }

            if (string.IsNullOrWhiteSpace(peerNickname))
            {
                // Return all private messages
                return _messages.Where(m => m.IsPrivate).ToList();
            }

            // Return private messages with specific peer
            return _messages.Where(m => m.IsPrivate &&
                (m.RecipientNickname == peerNickname || m.Sender == peerNickname))
                .ToList();
        }
    }

    /// <summary>
    /// Gets the count of unread messages
    /// </summary>
    public int GetUnreadCount(bool isPrivate = false, string? peerNickname = null)
    {
        lock (_messageLock)
        {
            var query = _messages.Where(m => !m.IsOwnMessage && m.Status != MessageStatus.Read);

            if (!isPrivate)
            {
                query = query.Where(m => !m.IsPrivate);
            }
            else
            {
                query = query.Where(m => m.IsPrivate);
                
                if (!string.IsNullOrWhiteSpace(peerNickname))
                {
                    query = query.Where(m => m.Sender == peerNickname);
                }
            }

            return query.Count();
        }
    }

    /// <summary>
    /// Adds a message to the collection with size management
    /// </summary>
    private async Task AddMessageAsync(BitchatMessage message)
    {
        await Task.Run(() =>
        {
            lock (_messageLock)
            {
                SafeInvokeOnMainThread(() =>
                {
                    _messages.Add(message);

                    // Enforce message limit
                    while (_messages.Count > _maxMessages)
                    {
                        _messages.RemoveAt(0);
                    }
                });
            }
        });
    }
}