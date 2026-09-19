namespace RealTimePortal.Domain;

public class Process
{
    public long Id { get; set; }
    public string ExecutionId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;

    public string ApplicationId { get; set; } = string.Empty;

    public string ProcessType { get; set; } = string.Empty;

    public string ReferenceNumber { get; set; } = string.Empty;

    public ProcessStatus Status { get; set; }

    public long? StartedByUserId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public string? RequestData { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public ICollection<ProcessStep> Steps { get; set; }
        = new List<ProcessStep>();
}