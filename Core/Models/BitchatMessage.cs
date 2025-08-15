namespace BitChat.Maui.Core.Models;

/// <summary>
/// Represents a message in the BitChat system
/// </summary>
public record BitchatMessage
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Nickname of the message sender
    /// </summary>
    public string Sender { get; init; } = string.Empty;

    /// <summary>
    /// Content of the message
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if this is a private message
    /// </summary>
    public bool IsPrivate { get; init; }

    /// <summary>
    /// Indicates if this message is being relayed through the mesh
    /// </summary>
    public bool IsRelay { get; init; }

    /// <summary>
    /// Recipient nickname for private messages
    /// </summary>
    public string? RecipientNickname { get; init; }

    /// <summary>
    /// Original sender for relayed messages
    /// </summary>
    public string? OriginalSender { get; init; }

    /// <summary>
    /// Current status of the message
    /// </summary>
    public MessageStatus Status { get; init; } = MessageStatus.Sent;

    /// <summary>
    /// Indicates if this message was sent by the current user
    /// </summary>
    public bool IsOwnMessage { get; init; }
}

/// <summary>
/// Represents the delivery status of a message
/// </summary>
public enum MessageStatus
{
    /// <summary>
    /// Message is being sent
    /// </summary>
    Sending,

    /// <summary>
    /// Message was successfully sent
    /// </summary>
    Sent,

    /// <summary>
    /// Message delivery failed
    /// </summary>
    Failed,

    /// <summary>
    /// Message was delivered to recipient
    /// </summary>
    Delivered,

    /// <summary>
    /// Message was read by recipient
    /// </summary>
    Read
}