using RealTimePortal.Application.Requests;
using RealTimePortal.Domain;

namespace RealTimePortal.Application;

public class ProcessService
{
    private readonly IProcessRepository _processRepository;
    private readonly IProgressGateway _progressGateway;
    private readonly IDashboardGateway _dashboardGateway;

    public ProcessService(
        IProcessRepository processRepository,
        IProgressGateway progressGateway,
        IDashboardGateway dashboardGateway)
    {
        _processRepository = processRepository;
        _progressGateway = progressGateway;
        _dashboardGateway = dashboardGateway;
    }

    public async Task<Process> CreateProcessAsync(
        CreateProcessRequest request,
        long? userId,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var process = new Process
        {
            ExecutionId = Guid.NewGuid().ToString(),
            ClientId = request.ClientId,
            ApplicationId = request.ApplicationId,
            ProcessType = request.ProcessType,
            ReferenceNumber = request.ReferenceNumber,
            Status = ProcessStatus.Pending,
            StartedByUserId = userId,
            RequestData = request.RequestData,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var step in request.Steps.OrderBy(x => x.StepNumber))
        {
            process.Steps.Add(new ProcessStep
            {
                StepNumber = step.StepNumber,
                StepName = step.StepName,
                Status = ProcessStepStatus.Pending,
                ProgressPercentage = 0,
                InputData = step.InputData
            });
        }

        await _processRepository.AddAsync(
            process,
            cancellationToken);

        await _dashboardGateway.ProcessStatusChangedAsync(
            process.Id,
            process.ProcessType,
            process.Status.ToString(),
            cancellationToken);

        return process;
    }

    public async Task<Process?> GetByIdAsync(
        long processId,
        CancellationToken cancellationToken = default)
    {
        return await _processRepository.GetByIdAsync(
            processId,
            cancellationToken);
    }

    public async Task<List<Process>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _processRepository.GetAllAsync(
            cancellationToken);
    }

    public async Task StartProcessAsync(
        long processId,
        CancellationToken cancellationToken = default)
    {
        var process = await GetRequiredProcessAsync(
            processId,
            cancellationToken);

        if (process.Status != ProcessStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending processes can be started.");
        }

        process.Status = ProcessStatus.Running;
        process.StartedAt = DateTime.UtcNow;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressUpdateAsync(
            new ProgressUpdateRequest
            {
                ExecutionId = process.ExecutionId,
                Step = "Process",
                Status = "InProgress",
                Percentage = 0,
                Message = "Process started."
            },
            cancellationToken);

        await _dashboardGateway.ProcessStatusChangedAsync(
            processId,
            process.ProcessType,
            process.Status.ToString(),
            cancellationToken);
    }

    public async Task StartStepAsync(
        long processId,
        int stepNumber,
        CancellationToken cancellationToken = default)
    {
        var process = await GetRequiredProcessAsync(
            processId,
            cancellationToken);

        EnsureProcessRunning(process);

        var step = GetRequiredStep(
            process,
            stepNumber);

        if (step.Status != ProcessStepStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending steps can be started.");
        }

        step.Status = ProcessStepStatus.Running;
        step.ProgressPercentage = 0;
        step.StartedAt = DateTime.UtcNow;
        step.Message = null;
        step.ErrorMessage = null;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressUpdateAsync(
            new ProgressUpdateRequest
            {
                ExecutionId = process.ExecutionId,
                Step = step.StepName,
                Status = "InProgress",
                Percentage = 0,
                Message = null
            },
            cancellationToken);
    }

    // ============================================================
    // GENERIC PROGRESS API
    // ============================================================

    public async Task UpdateProgressAsync(
        ProgressUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateProgressRequest(request);

        var process = await _processRepository.GetByExecutionIdAsync(
            request.ExecutionId,
            cancellationToken);

        if (process == null)
        {
            throw new KeyNotFoundException(
                $"Process with execution ID '{request.ExecutionId}' was not found.");
        }

        var status = request.Status.Trim();

        // "Process" means the overall process.
        if (request.Step.Equals(
                "Process",
                StringComparison.OrdinalIgnoreCase))
        {
            UpdateProcessProgress(
                process,
                status,
                request.Message);

            await _processRepository.UpdateAsync(
                process,
                cancellationToken);
        }
        else
        {
            var step = process.Steps.FirstOrDefault(
                x => x.StepName.Equals(
                    request.Step,
                    StringComparison.OrdinalIgnoreCase));

            if (step == null)
            {
                throw new KeyNotFoundException(
                    $"Step '{request.Step}' was not found for execution '{request.ExecutionId}'.");
            }

            UpdateStepProgress(
                process,
                step,
                status,
                request.Percentage,
                request.Message);

            await _processRepository.UpdateAsync(
                process,
                cancellationToken);
        }

        // Database is updated first.
        // SignalR then notifies the connected UI.
        await _progressGateway.SendProgressUpdateAsync(
            request,
            cancellationToken);

        // Refresh dashboard when overall process status changes.
        if (request.Step.Equals(
                "Process",
                StringComparison.OrdinalIgnoreCase))
        {
            await _dashboardGateway.ProcessStatusChangedAsync(
                process.Id,
                process.ProcessType,
                process.Status.ToString(),
                cancellationToken);
        }
    }

    public async Task CompleteProgressAsync(
        ProgressCompletedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ExecutionId))
        {
            throw new ArgumentException(
                "ExecutionId is required.");
        }

        var process = await _processRepository.GetByExecutionIdAsync(
            request.ExecutionId,
            cancellationToken);

        if (process == null)
        {
            throw new KeyNotFoundException(
                $"Process with execution ID '{request.ExecutionId}' was not found.");
        }

        if (process.Status != ProcessStatus.Running)
        {
            throw new InvalidOperationException(
                "Only running processes can be completed.");
        }

        foreach (var step in process.Steps)
        {
            if (step.Status != ProcessStepStatus.Failed &&
                step.Status != ProcessStepStatus.Skipped)
            {
                step.Status = ProcessStepStatus.Completed;
                step.ProgressPercentage = 100;
                step.CompletedAt ??= DateTime.UtcNow;
            }
        }

        process.Status = ProcessStatus.Completed;
        process.CompletedAt = DateTime.UtcNow;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressCompletedAsync(
            request,
            cancellationToken);

        await _dashboardGateway.ProcessStatusChangedAsync(
            process.Id,
            process.ProcessType,
            process.Status.ToString(),
            cancellationToken);
    }

    // ============================================================
    // NORMAL PROCESS STEP API
    // ============================================================

    public async Task UpdateStepProgressAsync(
        long processId,
        int stepNumber,
        UpdateStepProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProgressPercentage < 0 ||
            request.ProgressPercentage > 100)
        {
            throw new ArgumentException(
                "Progress percentage must be between 0 and 100.");
        }

        var process = await GetRequiredProcessAsync(
            processId,
            cancellationToken);

        EnsureProcessRunning(process);

        var step = GetRequiredStep(
            process,
            stepNumber);

        if (step.Status != ProcessStepStatus.Running)
        {
            throw new InvalidOperationException(
                "Only running steps can be updated.");
        }

        step.ProgressPercentage = request.ProgressPercentage;
        step.Message = request.Message;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressUpdateAsync(
            new ProgressUpdateRequest
            {
                ExecutionId = process.ExecutionId,
                Step = step.StepName,
                Status = "InProgress",
                Percentage = step.ProgressPercentage,
                Message = step.Message
            },
            cancellationToken);
    }

    public async Task CompleteStepAsync(
        long processId,
        int stepNumber,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        var process = await GetRequiredProcessAsync(
            processId,
            cancellationToken);

        EnsureProcessRunning(process);

        var step = GetRequiredStep(
            process,
            stepNumber);

        if (step.Status != ProcessStepStatus.Running)
        {
            throw new InvalidOperationException(
                "Only running steps can be completed.");
        }

        step.Status = ProcessStepStatus.Completed;
        step.ProgressPercentage = 100;
        step.CompletedAt = DateTime.UtcNow;
        step.Message = message;
        step.ErrorMessage = null;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressUpdateAsync(
            new ProgressUpdateRequest
            {
                ExecutionId = process.ExecutionId,
                Step = step.StepName,
                Status = "Completed",
                Percentage = 100,
                Message = message
            },
            cancellationToken);
    }

    public async Task FailStepAsync(
        long processId,
        int stepNumber,
        FailStepRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ErrorMessage))
        {
            throw new ArgumentException(
                "Error message is required.");
        }

        var process = await GetRequiredProcessAsync(
            processId,
            cancellationToken);

        EnsureProcessRunning(process);

        var step = GetRequiredStep(
            process,
            stepNumber);

        if (step.Status != ProcessStepStatus.Running)
        {
            throw new InvalidOperationException(
                "Only running steps can fail.");
        }

        step.Status = ProcessStepStatus.Failed;
        step.ErrorMessage = request.ErrorMessage;
        step.Message = request.ErrorMessage;
        step.CompletedAt = DateTime.UtcNow;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressUpdateAsync(
            new ProgressUpdateRequest
            {
                ExecutionId = process.ExecutionId,
                Step = step.StepName,
                Status = "Failed",
                Percentage = step.ProgressPercentage,
                Message = request.ErrorMessage
            },
            cancellationToken);
    }

    public async Task CompleteProcessAsync(
        long processId,
        CancellationToken cancellationToken = default)
    {
        var process = await GetRequiredProcessAsync(
            processId,
            cancellationToken);

        EnsureProcessRunning(process);

        if (process.Steps.Any(x =>
                x.Status != ProcessStepStatus.Completed &&
                x.Status != ProcessStepStatus.Skipped))
        {
            throw new InvalidOperationException(
                "Process cannot be completed until all steps are completed.");
        }

        process.Status = ProcessStatus.Completed;
        process.CompletedAt = DateTime.UtcNow;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressCompletedAsync(
            new ProgressCompletedRequest
            {
                ExecutionId = process.ExecutionId,
                Message = "Process completed."
            },
            cancellationToken);

        await _dashboardGateway.ProcessStatusChangedAsync(
            processId,
            process.ProcessType,
            process.Status.ToString(),
            cancellationToken);
    }

    public async Task FailProcessAsync(
        long processId,
        FailProcessRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ErrorMessage))
        {
            throw new ArgumentException(
                "Error message is required.");
        }

        var process = await GetRequiredProcessAsync(
            processId,
            cancellationToken);

        if (process.Status != ProcessStatus.Running)
        {
            throw new InvalidOperationException(
                "Only running processes can fail.");
        }

        process.Status = ProcessStatus.Failed;
        process.ErrorMessage = request.ErrorMessage;
        process.CompletedAt = DateTime.UtcNow;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressUpdateAsync(
            new ProgressUpdateRequest
            {
                ExecutionId = process.ExecutionId,
                Step = "Process",
                Status = "Failed",
                Percentage = 0,
                Message = request.ErrorMessage
            },
            cancellationToken);

        await _dashboardGateway.ProcessStatusChangedAsync(
            processId,
            process.ProcessType,
            process.Status.ToString(),
            cancellationToken);
    }

    public async Task CancelProcessAsync(
        long processId,
        CancellationToken cancellationToken = default)
    {
        var process = await GetRequiredProcessAsync(
            processId,
            cancellationToken);

        if (process.Status != ProcessStatus.Pending &&
            process.Status != ProcessStatus.Running)
        {
            throw new InvalidOperationException(
                "Only pending or running processes can be cancelled.");
        }

        process.Status = ProcessStatus.Cancelled;
        process.CompletedAt = DateTime.UtcNow;

        await _processRepository.UpdateAsync(
            process,
            cancellationToken);

        await _progressGateway.SendProgressUpdateAsync(
            new ProgressUpdateRequest
            {
                ExecutionId = process.ExecutionId,
                Step = "Process",
                Status = "Cancelled",
                Percentage = 0,
                Message = "Process cancelled."
            },
            cancellationToken);

        await _dashboardGateway.ProcessStatusChangedAsync(
            processId,
            process.ProcessType,
            process.Status.ToString(),
            cancellationToken);
    }

    // ============================================================
    // GENERIC PROGRESS HELPERS
    // ============================================================

    private static void UpdateProcessProgress(
        Process process,
        string status,
        string? message)
    {
        switch (status.ToLowerInvariant())
        {
            case "pending":
                process.Status = ProcessStatus.Pending;
                break;

            case "inprogress":
            case "in progress":
            case "running":
                process.Status = ProcessStatus.Running;
                process.StartedAt ??= DateTime.UtcNow;
                break;

            case "completed":
                process.Status = ProcessStatus.Completed;
                process.CompletedAt ??= DateTime.UtcNow;
                break;

            case "failed":
                process.Status = ProcessStatus.Failed;
                process.CompletedAt ??= DateTime.UtcNow;
                process.ErrorMessage = message;
                break;

            case "cancelled":
            case "canceled":
                process.Status = ProcessStatus.Cancelled;
                process.CompletedAt ??= DateTime.UtcNow;
                break;

            default:
                throw new ArgumentException(
                    $"Unsupported process status '{status}'.");
        }
    }

    private static void UpdateStepProgress(
        Process process,
        ProcessStep step,
        string status,
        int percentage,
        string? message)
    {
        switch (status.ToLowerInvariant())
        {
            case "pending":

                step.Status = ProcessStepStatus.Pending;
                step.ProgressPercentage = 0;
                step.Message = message;

                break;

            case "inprogress":
            case "in progress":
            case "running":

                if (process.Status != ProcessStatus.Running)
                {
                    throw new InvalidOperationException(
                        "Process must be running before step progress can be updated.");
                }

                step.Status = ProcessStepStatus.Running;
                step.ProgressPercentage = percentage;
                step.StartedAt ??= DateTime.UtcNow;
                step.Message = message;

                break;

            case "completed":

                step.Status = ProcessStepStatus.Completed;
                step.ProgressPercentage = 100;
                step.CompletedAt ??= DateTime.UtcNow;
                step.Message = message;
                step.ErrorMessage = null;

                break;

            case "failed":

                step.Status = ProcessStepStatus.Failed;
                step.ProgressPercentage = percentage;
                step.CompletedAt ??= DateTime.UtcNow;
                step.ErrorMessage = message;
                step.Message = message;

                break;

            case "skipped":

                step.Status = ProcessStepStatus.Skipped;
                step.ProgressPercentage = 100;
                step.CompletedAt ??= DateTime.UtcNow;
                step.Message = message;

                break;

            default:

                throw new ArgumentException(
                    $"Unsupported step status '{status}'.");
        }
    }

    private static void ValidateProgressRequest(
        ProgressUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExecutionId))
        {
            throw new ArgumentException(
                "ExecutionId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Step))
        {
            throw new ArgumentException(
                "Step is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new ArgumentException(
                "Status is required.");
        }

        if (request.Percentage < 0 ||
            request.Percentage > 100)
        {
            throw new ArgumentException(
                "Percentage must be between 0 and 100.");
        }
    }

    // ============================================================
    // COMMON HELPERS
    // ============================================================

    private async Task<Process> GetRequiredProcessAsync(
        long processId,
        CancellationToken cancellationToken)
    {
        var process = await _processRepository.GetByIdAsync(
            processId,
            cancellationToken);

        if (process == null)
        {
            throw new KeyNotFoundException(
                $"Process {processId} was not found.");
        }

        return process;
    }

    private static ProcessStep GetRequiredStep(
        Process process,
        int stepNumber)
    {
        var step = process.Steps.FirstOrDefault(
            x => x.StepNumber == stepNumber);

        if (step == null)
        {
            throw new KeyNotFoundException(
                $"Step number {stepNumber} was not found.");
        }

        return step;
    }

    private static void EnsureProcessRunning(
        Process process)
    {
        if (process.Status != ProcessStatus.Running)
        {
            throw new InvalidOperationException(
                "Process must be running.");
        }
    }

    private static void ValidateCreateRequest(
        CreateProcessRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            throw new ArgumentException(
                "ClientId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ApplicationId))
        {
            throw new ArgumentException(
                "ApplicationId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ProcessType))
        {
            throw new ArgumentException(
                "ProcessType is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ReferenceNumber))
        {
            throw new ArgumentException(
                "ReferenceNumber is required.");
        }

        if (request.Steps == null ||
            request.Steps.Count == 0)
        {
            throw new ArgumentException(
                "At least one process step is required.");
        }
    }
}