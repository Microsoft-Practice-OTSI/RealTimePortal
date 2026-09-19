namespace RealTimePortal.API.Constants;

public static class SignalREventNames
{
    // Process Events
    public const string ProcessStarted = "ProcessStarted";

    public const string ProcessStepStarted = "ProcessStepStarted";

    public const string ProcessStepCompleted = "ProcessStepCompleted";

    public const string ProcessCompleted =
        "ProcessCompleted";

    public const string ProcessFailed =
        "ProcessFailed";

    // Notification Events
    public const string NotificationCreated =
        "NotificationCreated";

    public const string NotificationRead =
        "NotificationRead";

    // Approval Events
    public const string ApprovalSubmitted =
        "ApprovalSubmitted";

    public const string ApprovalCompleted =
        "ApprovalCompleted";

    // Chat Events
    public const string ChatMessageReceived =
        "ChatMessageReceived";

    public const string ChatMessageRead =
        "ChatMessageRead";

    public const string UserTyping =
        "UserTyping";

    // Presence Events
    public const string UserOnline =
        "UserOnline";

    public const string UserOffline =
        "UserOffline";
}