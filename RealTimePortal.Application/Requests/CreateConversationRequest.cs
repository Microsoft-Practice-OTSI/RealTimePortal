namespace RealTimePortal.Application.Requests;

public class CreateConversationRequest
{
    public string ConversationType { get; set; } = "Direct";

    public List<long> ParticipantUserIds { get; set; } = new();
}