using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimePortal.Application;
using RealTimePortal.Application.Requests;
using System.Security.Claims;

namespace RealTimePortal.API.Controllers;

[ApiController]
[Authorize]
[Route("api/processes")]
public class ProcessController : ControllerBase
{
    private readonly ProcessService _processService;

    public ProcessController(ProcessService processService)
    {
        _processService = processService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProcessRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var process = await _processService.CreateProcessAsync(
            request,
            userId,
            cancellationToken);

        return StatusCode(
             StatusCodes.Status201Created,
             new
             {
                 process.Id,
                 process.ExecutionId,
                 process.ClientId,
                 process.ApplicationId,
                 process.ProcessType,
                 process.ReferenceNumber,
                 process.Status
             });
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(
    CancellationToken cancellationToken)
    {
        var processes = await _processService.GetAllAsync(
            cancellationToken);

        return Ok(
            processes
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.ClientId,
                    x.ApplicationId,
                    x.ProcessType,
                    x.ReferenceNumber,
                    x.Status,
                    x.StartedAt,
                    x.CompletedAt,
                    x.ErrorMessage,
                    x.CreatedAt
                }));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var process = await _processService.GetByIdAsync(
            id,
            cancellationToken);

        if (process == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            process.Id,
            process.ExecutionId,
            process.ClientId,
            process.ApplicationId,
            process.ProcessType,
            process.ReferenceNumber,
            process.Status,
            process.StartedByUserId,
            process.StartedAt,
            process.CompletedAt,
            process.ErrorMessage,
            process.RequestData,
            process.CreatedAt,
            process.CreatedBy,
            process.ModifiedAt,
            process.ModifiedBy,
            Steps = process.Steps
                .OrderBy(x => x.StepNumber)
                .Select(x => new
                {
                    x.Id,
                    x.StepNumber,
                    x.StepName,
                    x.Status,
                    x.ProgressPercentage,
                    x.RetryCount,
                    x.Message,
                    x.InputData,
                    x.OutputData,
                    x.StartedAt,
                    x.CompletedAt,
                    x.ErrorMessage
                })
        });
    }

    [HttpPost("{id:long}/start")]
    public async Task<IActionResult> Start(
        long id,
        CancellationToken cancellationToken)
    {
        await _processService.StartProcessAsync(
            id,
            cancellationToken);

        return Ok(new
        {
            ProcessId = id,
            Status = "Running"
        });
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(
        long id,
        CancellationToken cancellationToken)
    {
        await _processService.CancelProcessAsync(
            id,
            cancellationToken);

        return Ok(new
        {
            ProcessId = id,
            Status = "Cancelled"
        });
    }

    [HttpPost("{id:long}/steps/{stepNumber:int}/start")]
    public async Task<IActionResult> StartStep(
        long id,
        int stepNumber,
        CancellationToken cancellationToken)
    {
        await _processService.StartStepAsync(
            id,
            stepNumber,
            cancellationToken);

        return Ok(new
        {
            ProcessId = id,
            StepNumber = stepNumber,
            Status = "Running"
        });
    }

    [HttpPost("{id:long}/steps/{stepNumber:int}/progress")]
    public async Task<IActionResult> UpdateProgress(
        long id,
        int stepNumber,
        [FromBody] UpdateStepProgressRequest request,
        CancellationToken cancellationToken)
    {
        await _processService.UpdateStepProgressAsync(
            id,
            stepNumber,
            request,
            cancellationToken);

        return Ok(new
        {
            ProcessId = id,
            StepNumber = stepNumber,
            ProgressPercentage = request.ProgressPercentage,
            Message = request.Message
        });
    }

    [HttpPost("{id:long}/steps/{stepNumber:int}/complete")]
    public async Task<IActionResult> CompleteStep(
        long id,
        int stepNumber,
        [FromBody] string? message,
        CancellationToken cancellationToken)
    {
        await _processService.CompleteStepAsync(
            id,
            stepNumber,
            message,
            cancellationToken);

        return Ok(new
        {
            ProcessId = id,
            StepNumber = stepNumber,
            Status = "Completed"
        });
    }

    [HttpPost("{id:long}/steps/{stepNumber:int}/fail")]
    public async Task<IActionResult> FailStep(
        long id,
        int stepNumber,
        [FromBody] FailStepRequest request,
        CancellationToken cancellationToken)
    {
        await _processService.FailStepAsync(
            id,
            stepNumber,
            request,
            cancellationToken);

        return Ok(new
        {
            ProcessId = id,
            StepNumber = stepNumber,
            Status = "Failed"
        });
    }

    [HttpPost("{id:long}/complete")]
    public async Task<IActionResult> Complete(
        long id,
        CancellationToken cancellationToken)
    {
        await _processService.CompleteProcessAsync(
            id,
            cancellationToken);

        return Ok(new
        {
            ProcessId = id,
            Status = "Completed"
        });
    }

    [HttpPost("{id:long}/fail")]
    public async Task<IActionResult> Fail(
        long id,
        [FromBody] FailProcessRequest request,
        CancellationToken cancellationToken)
    {
        await _processService.FailProcessAsync(
            id,
            request,
            cancellationToken);

        return Ok(new
        {
            ProcessId = id,
            Status = "Failed"
        });
    }

    private long? GetUserId()
    {
        var claim = User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (claim == null)
        {
            return null;
        }

        return long.TryParse(
            claim.Value,
            out var userId)
            ? userId
            : null;
    }
}