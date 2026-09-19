using RealTimePortal.Domain;

namespace RealTimePortal.Application;

public interface IChatRepository
{
    Task<Conversation> CreateConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    Task<List<Conversation>> GetConversationsAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<bool> IsParticipantAsync(
        long conversationId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<List<ChatMessage>> GetMessagesAsync(
        long conversationId,
        CancellationToken cancellationToken = default);

    Task<ChatMessage> AddMessageAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default);

    Task MarkMessagesAsReadAsync(
        long conversationId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<Conversation?> GetConversationByIdAsync(
    long conversationId,
    CancellationToken cancellationToken = default);
}