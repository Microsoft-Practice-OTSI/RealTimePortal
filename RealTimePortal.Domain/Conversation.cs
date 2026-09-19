namespace RealTimePortal.Domain;

public class Conversation
{
    public long Id { get; set; }

    public string ConversationType { get; set; } = "Direct";

    public DateTime CreatedAt { get; set; }

    public long CreatedByUserId { get; set; }

    public ICollection<ConversationParticipant> Participants { get; set; }
        = new List<ConversationParticipant>();

    public ICollection<ChatMessage> Messages { get; set; }
        = new List<ChatMessage>();
}