using Fixnow.DTOs.Chat;

namespace Fixnow.Services.Interfaces;

public interface IChatService
{
  Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
  Task<(List<MessageDto> items, int totalCount)> GetMessagesAsync(Guid conversationId, Guid userId, int page, int pageSize);
  Task<MessageDto> SendMessageAsync(SendMessageRequestDto request, Guid senderId);
  Task MarkAsReadAsync(Guid conversationId, Guid receiverId);
}
