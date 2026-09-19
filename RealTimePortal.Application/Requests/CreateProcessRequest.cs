namespace RealTimePortal.Application.Requests;

public class CreateProcessRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string ProcessType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? RequestData { get; set; }
    public List<CreateProcessStepRequest> Steps { get; set; } = new();
}

public class CreateProcessStepRequest
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? InputData { get; set; }
}

public class CreateProcessResponse
{
    public long ProcessId { get; set; }
    public string ExecutionId { get; set; } = string.Empty;
}