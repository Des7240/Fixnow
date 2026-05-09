using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class ConversationRepository : IConversationRepository
{
  private readonly AppDbContext _db;

  public ConversationRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task<Conversation> CreateAsync(Conversation conversation)
  {
    _db.Conversations.Add(conversation);
    await _db.SaveChangesAsync();
    return conversation;
  }

  public async Task<Conversation?> FindByIdAsync(Guid id)
  {
    return await _db.Conversations
      .Include(c => c.Customer)
      .Include(c => c.Worker)
      .FirstOrDefaultAsync(c => c.Id == id);
  }

  public async Task<Conversation?> FindByBookingAsync(Guid bookingId)
  {
    return await _db.Conversations
      .Include(c => c.Customer)
      .Include(c => c.Worker)
      .FirstOrDefaultAsync(c => c.BookingId == bookingId);
  }

  public async Task<List<Conversation>> FindByUserAsync(Guid userId)
  {
    return await _db.Conversations
      .Include(c => c.Customer)
      .Include(c => c.Worker)
      .Where(c => c.CustomerId == userId || c.WorkerId == userId)
      .OrderByDescending(c => c.CreatedAt)
      .ToListAsync();
  }
}
