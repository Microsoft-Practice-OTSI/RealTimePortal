namespace RealTimePortal.Application.Responses;

public class ChatMessageResponse
{
    public long Id { get; set; }

    public long ConversationId { get; set; }

    public long SenderUserId { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }
}