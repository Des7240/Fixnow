using Fixnow.DTOs.Chat;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Hubs;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Fixnow.Data;

namespace Fixnow.Services;

public class ChatService : IChatService
{
  private readonly IConversationRepository _conversationRepo;
  private readonly IMessageRepository _messageRepo;
  private readonly IBookingRepository _bookingRepo;
  private readonly IHubContext<ChatHub> _hubContext;
  private readonly INotificationService _notificationService;
  private readonly AppDbContext _db;

  public ChatService(
    IConversationRepository conversationRepo,
    IMessageRepository messageRepo,
    IBookingRepository bookingRepo,
    IHubContext<ChatHub> hubContext,
    INotificationService notificationService,
    AppDbContext db)
  {
    _conversationRepo = conversationRepo;
    _messageRepo = messageRepo;
    _bookingRepo = bookingRepo;
    _hubContext = hubContext;
    _notificationService = notificationService;
    _db = db;
  }

  public async Task<ConversationDto> GetOrCreateConversationByBookingAsync(Guid bookingId, Guid userId)
  {
    var conversation = await _conversationRepo.FindByBookingAsync(bookingId);
    if (conversation == null)
    {
      var booking = await _bookingRepo.FindByIdAsync(bookingId)
        ?? throw new KeyNotFoundException("Booking not found.");

      if (booking.CustomerId != userId && booking.WorkerId != userId)
        throw new UnauthorizedAccessException("You are not part of this booking.");

      if (!booking.WorkerId.HasValue)
        throw new InvalidOperationException("Cannot start chat without an assigned worker.");

      conversation = new Conversation
      {
        BookingId = bookingId,
        CustomerId = booking.CustomerId,
        WorkerId = booking.WorkerId.Value
      };

      await _conversationRepo.CreateAsync(conversation);
      
      // Reload to get navigation properties (Customer, Worker names)
      conversation = await _db.Conversations
        .Include(c => c.Customer)
        .Include(c => c.Worker)
        .FirstOrDefaultAsync(c => c.Id == conversation.Id);
    }

    return new ConversationDto
    {
      Id = conversation!.Id,
      BookingId = conversation.BookingId,
      CustomerId = conversation.CustomerId,
      WorkerId = conversation.WorkerId,
      CustomerName = conversation.Customer?.FullName ?? "Customer",
      WorkerName = conversation.Worker?.FullName ?? "Worker",
      CreatedAt = conversation.CreatedAt,
      UnreadCount = 0
    };
  }

  public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
  {
    var conversations = await _conversationRepo.FindByUserAsync(userId);
    var dtos = new List<ConversationDto>();

    foreach (var conv in conversations)
    {
      // Find last message and unread count
      // This can be optimized using EF Core raw queries or properly mapped repositories if the dataset grows
      var lastMessage = await _db.Messages
        .Include(m => m.Attachments).ThenInclude(a => a.File)
        .Where(m => m.ConversationId == conv.Id)
        .OrderByDescending(m => m.CreatedAt)
        .FirstOrDefaultAsync();

      var unreadCount = await _db.Messages
        .CountAsync(m => m.ConversationId == conv.Id && m.SenderId != userId && !m.IsRead);

      var dto = new ConversationDto
      {
        Id = conv.Id,
        BookingId = conv.BookingId,
        CustomerId = conv.CustomerId,
        WorkerId = conv.WorkerId,
        CustomerName = conv.Customer.FullName,
        WorkerName = conv.Worker.FullName,
        CreatedAt = conv.CreatedAt,
        UnreadCount = unreadCount,
        LastMessage = lastMessage != null ? MapMessage(lastMessage) : null
      };

      dtos.Add(dto);
    }

    return dtos.OrderByDescending(d => d.LastMessage?.CreatedAt ?? d.CreatedAt).ToList();
  }

  public async Task<(List<MessageDto> items, int totalCount)> GetMessagesAsync(Guid conversationId, Guid userId, int page, int pageSize)
  {
    var conversation = await _conversationRepo.FindByIdAsync(conversationId)
      ?? throw new KeyNotFoundException("Conversation not found.");

    if (conversation.CustomerId != userId && conversation.WorkerId != userId)
      throw new UnauthorizedAccessException("You are not part of this conversation.");

    var (messages, totalCount) = await _messageRepo.GetMessagesByConversationAsync(conversationId, page, pageSize);

    var dtos = messages.Select(MapMessage).ToList();
    return (dtos, totalCount);
  }

  public async Task<MessageDto> SendMessageAsync(SendMessageRequestDto request, Guid senderId)
  {
    var conversation = await _conversationRepo.FindByIdAsync(request.ConversationId)
      ?? throw new KeyNotFoundException("Conversation not found.");

    if (conversation.CustomerId != senderId && conversation.WorkerId != senderId)
      throw new UnauthorizedAccessException("You are not part of this conversation.");

    // Validate booking status (Optional: prevent chat if cancelled)
    // var booking = await _bookingRepo.FindByIdAsync(conversation.BookingId);
    // if (booking?.Status == BookingStatus.CANCELLED) throw ...

    var message = new Message
    {
      ConversationId = request.ConversationId,
      SenderId = senderId,
      MessageType = request.MessageType,
      Content = request.Content,
      IsRead = false
    };

    await _messageRepo.CreateAsync(message);

    if (request.MessageType == MessageType.IMAGE && request.FileIds != null && request.FileIds.Any())
    {
      foreach (var fileId in request.FileIds)
      {
        var attachment = new MessageAttachment
        {
          MessageId = message.Id,
          FileId = fileId
        };
        await _messageRepo.AddAttachmentAsync(attachment);
      }
    }

    // Reload message with details
    var createdMsg = await _messageRepo.FindByIdAsync(message.Id);
    var msgDto = MapMessage(createdMsg!);

    // Broadcast to SignalR group
    await _hubContext.Clients.Group(conversation.Id.ToString())
      .SendAsync("ReceiveMessage", msgDto);

    // Send push notification to offline user
    var receiverId = conversation.CustomerId == senderId ? conversation.WorkerId : conversation.CustomerId;
    
    // In a production app, we'd check if the user is connected to SignalR first.
    // For MVP, we'll send an in-app notification via INotificationService
    var senderName = senderId == conversation.CustomerId ? conversation.Customer.FullName : conversation.Worker.FullName;
    var preview = request.MessageType == MessageType.IMAGE ? "[Image]" : request.Content;
    await _notificationService.NotifyNewChatMessageAsync(receiverId, conversation.BookingId, senderName, preview);

    return msgDto;
  }

  public async Task MarkAsReadAsync(Guid conversationId, Guid receiverId)
  {
    var conversation = await _conversationRepo.FindByIdAsync(conversationId)
      ?? throw new KeyNotFoundException("Conversation not found.");

    if (conversation.CustomerId != receiverId && conversation.WorkerId != receiverId)
      throw new UnauthorizedAccessException("You are not part of this conversation.");

    await _messageRepo.MarkAllAsReadAsync(conversationId, receiverId);

    // Broadcast read receipt event
    await _hubContext.Clients.Group(conversationId.ToString())
      .SendAsync("ReadReceipt", new { conversationId, readBy = receiverId });
  }

  private static MessageDto MapMessage(Message m)
  {
    return new MessageDto
    {
      Id = m.Id,
      ConversationId = m.ConversationId,
      SenderId = m.SenderId,
      MessageType = m.MessageType.ToString(),
      Content = m.Content,
      IsRead = m.IsRead,
      CreatedAt = m.CreatedAt,
      AttachmentUrls = m.Attachments?.Select(a => $"/api/v1/files/{a.FileId}").ToList() ?? new List<string>()
    };
  }
}
