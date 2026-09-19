namespace RealTimePortal.Domain;

public class ConversationParticipant
{
    public long Id { get; set; }

    public long ConversationId { get; set; }

    public long UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}