using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IConversationRepository
{
  Task<Conversation> CreateAsync(Conversation conversation);
  Task<Conversation?> FindByIdAsync(Guid id);
  Task<Conversation?> FindByBookingAsync(Guid bookingId);
  Task<List<Conversation>> FindByUserAsync(Guid userId);
}
