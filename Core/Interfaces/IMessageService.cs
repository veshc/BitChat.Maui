using BitChat.Maui.Core.Models;
using System.Collections.ObjectModel;

namespace BitChat.Maui.Core.Interfaces;

/// <summary>
/// Service for managing chat messages
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Observable collection of all messages
    /// </summary>
    ObservableCollection<BitchatMessage> Messages { get; }

    /// <summary>
    /// Event raised when a new message is received
    /// </summary>
    event EventHandler<BitchatMessage>? MessageReceived;

    /// <summary>
    /// Event raised when a message status changes
    /// </summary>
    event EventHandler<BitchatMessage>? MessageStatusChanged;

    /// <summary>
    /// Sends a public message to all connected peers
    /// </summary>
    /// <param name="content">The message content</param>
    /// <param name="senderNickname">The sender's nickname</param>
    /// <returns>The sent message</returns>
    Task<BitchatMessage> SendPublicMessageAsync(string content, string senderNickname);

    /// <summary>
    /// Sends a private message to a specific recipient
    /// </summary>
    /// <param name="content">The message content</param>
    /// <param name="senderNickname">The sender's nickname</param>
    /// <param name="recipientNickname">The recipient's nickname</param>
    /// <returns>The sent message</returns>
    Task<BitchatMessage> SendPrivateMessageAsync(string content, string senderNickname, string recipientNickname);

    /// <summary>
    /// Receives a message from another peer
    /// </summary>
    /// <param name="message">The received message</param>
    Task ReceiveMessageAsync(BitchatMessage message);

    /// <summary>
    /// Updates the status of a message
    /// </summary>
    /// <param name="messageId">The message ID</param>
    /// <param name="status">The new status</param>
    Task UpdateMessageStatusAsync(string messageId, MessageStatus status);

    /// <summary>
    /// Clears all messages from memory
    /// </summary>
    void ClearMessages();

    /// <summary>
    /// Gets messages for a specific conversation (public or private)
    /// </summary>
    /// <param name="isPrivate">Whether to get private messages</param>
    /// <param name="peerNickname">The peer nickname for private messages</param>
    /// <returns>Collection of messages</returns>
    IEnumerable<BitchatMessage> GetMessages(bool isPrivate = false, string? peerNickname = null);

    /// <summary>
    /// Gets the count of unread messages
    /// </summary>
    /// <param name="isPrivate">Whether to count private messages</param>
    /// <param name="peerNickname">The peer nickname for private messages</param>
    /// <returns>Count of unread messages</returns>
    int GetUnreadCount(bool isPrivate = false, string? peerNickname = null);
}