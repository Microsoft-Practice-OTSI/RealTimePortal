using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimePortal.Application;
using RealTimePortal.Domain;

namespace RealTimePortal.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    //[HttpGet("summary")]
    //public async Task<IActionResult> GetSummary(
    //    CancellationToken cancellationToken)
    //{
    //    var result = await _dashboardService.GetSummaryAsync(
    //        cancellationToken);

    //    return Ok(result);
    //}

    [HttpGet("process-types")]
    public async Task<IActionResult> GetProcessTypes(
    [FromQuery] DashboardPeriod period = DashboardPeriod.AllTime,
    CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetProcessTypeSummaryAsync(
            period,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("recent-processes")]
    public async Task<IActionResult> GetRecentProcesses(
    [FromQuery] DashboardPeriod period = DashboardPeriod.AllTime,
    [FromQuery] int count = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetRecentProcessesAsync(
            period,
            count,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("running-processes")]
    public async Task<IActionResult> GetRunningProcesses(
     [FromQuery] DashboardPeriod period = DashboardPeriod.AllTime,
     CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetRunningProcessesAsync(
            period,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("failed-processes")]
    public async Task<IActionResult> GetFailedProcesses(
    [FromQuery] DashboardPeriod period = DashboardPeriod.AllTime,
    [FromQuery] int count = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetFailedProcessesAsync(
            period,
            count,
            cancellationToken);

        return Ok(result);
    }


    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DashboardPeriod period = DashboardPeriod.AllTime,
        CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetSummaryAsync(
            period,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend(
    [FromQuery] DashboardPeriod period = DashboardPeriod.Last7Days,
    CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetTrendAsync(
            period,
            cancellationToken);

        return Ok(result);
    }
}