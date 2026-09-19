using Microsoft.EntityFrameworkCore;
using RealTimePortal.Application;
using RealTimePortal.Domain;

namespace RealTimePortal.Infrastructure;

public class ChatRepository : IChatRepository
{
    private readonly RealTimePortalDbContext _dbContext;

    public ChatRepository(RealTimePortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conversation> CreateConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Conversations.AddAsync(
            conversation,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return conversation;
    }

    public async Task<List<Conversation>> GetConversationsAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations
            .AsNoTracking()
            .Include(x => x.Participants)
            .Where(x => x.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsParticipantAsync(
        long conversationId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConversationParticipants
            .AnyAsync(
                x => x.ConversationId == conversationId &&
                     x.UserId == userId,
                cancellationToken);
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(
        long conversationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ChatMessages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatMessage> AddMessageAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ChatMessages.AddAsync(
            message,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return message;
    }

    public async Task MarkMessagesAsReadAsync(
        long conversationId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.ChatMessages
            .Where(x =>
                x.ConversationId == conversationId &&
                x.SenderUserId != userId &&
                !x.IsRead)
            .ToListAsync(cancellationToken);

        var readAt = DateTime.UtcNow;

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = readAt;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}