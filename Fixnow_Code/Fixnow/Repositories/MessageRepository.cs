using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class MessageRepository : IMessageRepository
{
  private readonly AppDbContext _db;

  public MessageRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task<Message> CreateAsync(Message message)
  {
    _db.Messages.Add(message);
    await _db.SaveChangesAsync();
    return message;
  }

  public async Task<Message?> FindByIdAsync(Guid id)
  {
    return await _db.Messages
      .Include(m => m.Attachments)
        .ThenInclude(a => a.File)
      .FirstOrDefaultAsync(m => m.Id == id);
  }

  public async Task<(List<Message> items, int totalCount)> GetMessagesByConversationAsync(Guid conversationId, int page, int pageSize)
  {
    var query = _db.Messages
      .Include(m => m.Attachments)
        .ThenInclude(a => a.File)
      .Where(m => m.ConversationId == conversationId);

    var totalCount = await query.CountAsync();
    var items = await query
      .OrderBy(m => m.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    return (items, totalCount);
  }

  public async Task UpdateAsync(Message message)
  {
    _db.Messages.Update(message);
    await _db.SaveChangesAsync();
  }

  public async Task AddAttachmentAsync(MessageAttachment attachment)
  {
    _db.MessageAttachments.Add(attachment);
    await _db.SaveChangesAsync();
  }

  public async Task MarkAllAsReadAsync(Guid conversationId, Guid receiverId)
  {
    var unreadMessages = await _db.Messages
      .Where(m => m.ConversationId == conversationId && m.SenderId != receiverId && !m.IsRead)
      .ToListAsync();

    if (unreadMessages.Any())
    {
      foreach (var msg in unreadMessages)
      {
        msg.IsRead = true;
      }
      await _db.SaveChangesAsync();
    }
  }
}
