using RealTimePortal.Application.Requests;
using RealTimePortal.Application.Responses;
using RealTimePortal.Domain;

namespace RealTimePortal.Application;

public class ChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IChatGateway _chatGateway;

    public ChatService(
        IChatRepository chatRepository,
        IChatGateway chatGateway)
    {
        _chatRepository = chatRepository;
        _chatGateway = chatGateway;
    }

    public async Task<ConversationResponse> CreateConversationAsync(
        long userId,
        CreateConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ParticipantUserIds == null ||
            request.ParticipantUserIds.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one participant is required.");
        }

        var participantIds = request.ParticipantUserIds
            .Append(userId)
            .Distinct()
            .ToList();

        var conversation = new Conversation
        {
            ConversationType = request.ConversationType,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var participantId in participantIds)
        {
            conversation.Participants.Add(
                new ConversationParticipant
                {
                    UserId = participantId,
                    JoinedAt = DateTime.UtcNow
                });
        }

        await _chatRepository.CreateConversationAsync(
            conversation,
            cancellationToken);

        return MapConversation(conversation);
    }

    public async Task<List<ConversationResponse>> GetConversationsAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var conversations =
            await _chatRepository.GetConversationsAsync(
                userId,
                cancellationToken);

        return conversations
            .Select(MapConversation)
            .ToList();
    }

    public async Task<List<ChatMessageResponse>> GetMessagesAsync(
        long conversationId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureParticipantAsync(
            conversationId,
            userId,
            cancellationToken);

        var messages =
            await _chatRepository.GetMessagesAsync(
                conversationId,
                cancellationToken);

        return messages
            .Select(MapMessage)
            .ToList();
    }

    public async Task<ChatMessageResponse> SendMessageAsync(
        long conversationId,
        long userId,
        SendChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureParticipantAsync(
            conversationId,
            userId,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException(
                "Message cannot be empty.");
        }

        if (request.Message.Length > 4000)
        {
            throw new InvalidOperationException(
                "Message cannot exceed 4000 characters.");
        }

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderUserId = userId,
            Message = request.Message.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        await _chatRepository.AddMessageAsync(
            message,
            cancellationToken);

        var response = MapMessage(message);

        await _chatGateway.MessageReceivedAsync(
            conversationId,
            response,
            cancellationToken);

        return response;
    }

    public async Task MarkAsReadAsync(
        long conversationId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureParticipantAsync(
            conversationId,
            userId,
            cancellationToken);

        await _chatRepository.MarkMessagesAsReadAsync(
            conversationId,
            userId,
            cancellationToken);
    }

    public async Task EnsureParticipantAsync(
        long conversationId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var isParticipant =
            await _chatRepository.IsParticipantAsync(
                conversationId,
                userId,
                cancellationToken);

        if (!isParticipant)
        {
            throw new UnauthorizedAccessException(
                "You are not a participant in this conversation.");
        }
    }

    private static ConversationResponse MapConversation(
        Conversation conversation)
    {
        return new ConversationResponse
        {
            Id = conversation.Id,
            ConversationType = conversation.ConversationType,
            CreatedAt = conversation.CreatedAt,
            ParticipantUserIds = conversation.Participants
                .Select(x => x.UserId)
                .ToList()
        };
    }

    private static ChatMessageResponse MapMessage(
        ChatMessage message)
    {
        return new ChatMessageResponse
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            Message = message.Message,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt
        };
    }
}