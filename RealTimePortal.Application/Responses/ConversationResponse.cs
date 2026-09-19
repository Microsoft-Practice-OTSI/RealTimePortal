namespace RealTimePortal.Application.Responses;

public class ConversationResponse
{
    public long Id { get; set; }

    public string ConversationType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<long> ParticipantUserIds { get; set; } = new();

    public int UnreadMessageCount { get; set; }
}