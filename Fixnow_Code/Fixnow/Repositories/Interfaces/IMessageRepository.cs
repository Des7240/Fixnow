using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IMessageRepository
{
  Task<Message> CreateAsync(Message message);
  Task<Message?> FindByIdAsync(Guid id);
  Task<(List<Message> items, int totalCount)> GetMessagesByConversationAsync(Guid conversationId, int page, int pageSize);
  Task UpdateAsync(Message message);
  Task AddAttachmentAsync(MessageAttachment attachment);
  Task MarkAllAsReadAsync(Guid conversationId, Guid receiverId);
}
