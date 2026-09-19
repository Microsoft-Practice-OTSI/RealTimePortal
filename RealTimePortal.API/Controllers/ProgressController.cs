using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimePortal.Application;
using RealTimePortal.Application.Requests;

namespace RealTimePortal.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly ProcessService _processService;

    public ProgressController(
        ProcessService processService)
    {
        _processService = processService;
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] ProgressUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                request.ExecutionId))
        {
            return BadRequest(new
            {
                success = false,
                message = "ExecutionId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
                request.Step))
        {
            return BadRequest(new
            {
                success = false,
                message = "Step is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
                request.Status))
        {
            return BadRequest(new
            {
                success = false,
                message = "Status is required."
            });
        }

        if (request.Percentage < 0 ||
            request.Percentage > 100)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Percentage must be between 0 and 100."
            });
        }

        await _processService.UpdateProgressAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Progress update processed successfully."
        });
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromBody] ProgressCompletedRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                request.ExecutionId))
        {
            return BadRequest(new
            {
                success = false,
                message = "ExecutionId is required."
            });
        }

        await _processService.CompleteProgressAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message =
                "Process completed successfully."
        });
    }
}