namespace RealTimePortal.Domain;

public class ProcessStep
{
    public long Id { get; set; }

    public long ProcessId { get; set; }

    public int StepNumber { get; set; }

    public string StepName { get; set; } = string.Empty;

    public ProcessStepStatus Status { get; set; }

    public int ProgressPercentage { get; set; }

    public int RetryCount { get; set; }

    public string? Message { get; set; }

    public string? InputData { get; set; }

    public string? OutputData { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public Process Process { get; set; } = null!;
}