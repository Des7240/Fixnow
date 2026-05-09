using Fixnow.Enums;

namespace Fixnow.Entities;

public class Message
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid ConversationId { get; set; }
  public Guid? SenderId { get; set; } // Null for SYSTEM messages
  public MessageType MessageType { get; set; } = MessageType.TEXT;
  public string Content { get; set; } = string.Empty;
  public bool IsRead { get; set; } = false;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Conversation Conversation { get; set; } = null!;
  public User? Sender { get; set; }
  public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}
