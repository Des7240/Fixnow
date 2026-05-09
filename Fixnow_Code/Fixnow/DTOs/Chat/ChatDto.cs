using Fixnow.Enums;

namespace Fixnow.DTOs.Chat;

public class ConversationDto
{
  public Guid Id { get; set; }
  public Guid BookingId { get; set; }
  public Guid CustomerId { get; set; }
  public Guid WorkerId { get; set; }
  public DateTime CreatedAt { get; set; }
  
  // Extra fields for UI convenience
  public string CustomerName { get; set; } = string.Empty;
  public string WorkerName { get; set; } = string.Empty;
  public MessageDto? LastMessage { get; set; }
  public int UnreadCount { get; set; }
}

public class MessageDto
{
  public Guid Id { get; set; }
  public Guid ConversationId { get; set; }
  public Guid? SenderId { get; set; }
  public string MessageType { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public bool IsRead { get; set; }
  public DateTime CreatedAt { get; set; }
  public List<string> AttachmentUrls { get; set; } = new();
}

public class SendMessageRequestDto
{
  public Guid ConversationId { get; set; }
  public MessageType MessageType { get; set; }
  public string Content { get; set; } = string.Empty;
  public List<Guid>? FileIds { get; set; } // For IMAGE type
}
