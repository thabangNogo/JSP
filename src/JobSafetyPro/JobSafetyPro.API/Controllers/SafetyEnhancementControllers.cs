using System.Text;
using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.DTOs.Workflow;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Application.Services;
using JobSafetyPro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSafetyPro.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/stop-unsafe-work")]
public class StopUnsafeWorkController : ControllerBase
{
    private readonly IStopUnsafeWorkService _service;

    public StopUnsafeWorkController(IStopUnsafeWorkService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StopUnsafeWorkDto>>>> GetAll(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<StopUnsafeWorkDto>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StopUnsafeWorkDto>>> Create(
        [FromBody] CreateStopUnsafeWorkDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<StopUnsafeWorkDto>.Ok(await _service.CreateAsync(dto, cancellationToken), "Report submitted."));

    [HttpPost("{id:guid}/acknowledge")]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<StopUnsafeWorkDto>>> Acknowledge(
        Guid id,
        [FromBody] AcknowledgeStopUnsafeWorkDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<StopUnsafeWorkDto>.Ok(await _service.AcknowledgeAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/work-stopped")]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<StopUnsafeWorkDto>>> MarkWorkStopped(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<StopUnsafeWorkDto>.Ok(await _service.MarkWorkStoppedAsync(id, cancellationToken)));

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<StopUnsafeWorkDto>>> Resolve(
        Guid id,
        [FromBody] ResolveStopUnsafeWorkDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<StopUnsafeWorkDto>.Ok(await _service.ResolveAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<StopUnsafeWorkDto>>> Close(
        Guid id,
        [FromBody] VerifyStopUnsafeWorkDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<StopUnsafeWorkDto>.Ok(await _service.VerifyAndCloseAsync(id, dto, cancellationToken)));
}

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public class SafetyNotificationsController : ControllerBase
{
    private readonly ISafetyNotificationService _service;

    public SafetyNotificationsController(ISafetyNotificationService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SafetyNotificationDto>>>> GetMine(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<SafetyNotificationDto>>.Ok(await _service.GetMyNotificationsAsync(cancellationToken)));

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<NotificationSummaryDto>>> GetUnreadCount(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<NotificationSummaryDto>.Ok(await _service.GetUnreadSummaryAsync(cancellationToken)));

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await _service.MarkReadAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Marked as read."));
    }

    [HttpPost("devices")]
    public async Task<ActionResult<ApiResponse<object>>> RegisterDevice(
        [FromBody] RegisterDeviceDto dto,
        CancellationToken cancellationToken)
    {
        await _service.RegisterDeviceAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Device registered."));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/safety-kpis")]
public class SafetyKpiController : ControllerBase
{
    private readonly ISafetyKpiService _service;

    public SafetyKpiController(ISafetyKpiService service) => _service = service;

    [HttpGet("employee")]
    public async Task<ActionResult<ApiResponse<EmployeeSafetyKpiDto>>> GetEmployee(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<EmployeeSafetyKpiDto>.Ok(await _service.GetEmployeeKpisAsync(cancellationToken)));

    [HttpGet("manager")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<ManagerSafetyKpiDto>>> GetManager(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<ManagerSafetyKpiDto>.Ok(await _service.GetManagerKpisAsync(cancellationToken)));
}

[ApiController]
[Authorize(Policy = "RequireHseManager")]
[Route("api/v1/safety-reports")]
public class SafetyReportsController : ControllerBase
{
    private readonly INearMissService _nearMisses;
    private readonly IStopUnsafeWorkService _stopUnsafeWork;
    private readonly ICorrectiveActionService _correctiveActions;

    public SafetyReportsController(
        INearMissService nearMisses,
        IStopUnsafeWorkService stopUnsafeWork,
        ICorrectiveActionService correctiveActions)
    {
        _nearMisses = nearMisses;
        _stopUnsafeWork = stopUnsafeWork;
        _correctiveActions = correctiveActions;
    }

    [HttpGet("near-misses/export")]
    public async Task<IActionResult> ExportNearMissRegister(CancellationToken cancellationToken)
    {
        var items = await _nearMisses.GetAllAsync(cancellationToken);
        var csv = BuildCsv(
            "Id,Location,Section,Description,OccurredAt,Status",
            items.Select(i => $"{i.Id},{Escape(i.Description)},{i.OccurredAt:O}"));
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "near-miss-register.csv");
    }

    [HttpGet("stop-unsafe-work/export")]
    public async Task<IActionResult> ExportStopUnsafeWorkRegister(CancellationToken cancellationToken)
    {
        var items = await _stopUnsafeWork.GetAllAsync(cancellationToken);
        var csv = BuildCsv(
            "Id,Department,Location,Section,Category,ImmediateRisk,Status,CreatedDate",
            items.Select(i =>
                $"{i.Id},{Escape(i.Department)},{Escape(i.Location)},{Escape(i.Section)},{i.Category},{i.ImmediateRisk},{i.Status},{i.CreatedDate:O}"));
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "stop-unsafe-work-register.csv");
    }

    [HttpGet("corrective-actions/export")]
    public async Task<IActionResult> ExportCorrectiveActionRegister(CancellationToken cancellationToken)
    {
        var items = await _correctiveActions.GetAllAsync(cancellationToken);
        var csv = BuildCsv(
            "Id,NearMissId,Description,AssignedTo,DueDate,Status",
            items.Select(i =>
                $"{i.Id},{i.NearMissId},{Escape(i.Description)},{i.AssignedToUserId},{i.DueDate:O},{i.Status}"));
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "corrective-action-register.csv");
    }

    private static string BuildCsv(string header, IEnumerable<string> rows) =>
        header + "\n" + string.Join("\n", rows);

    private static string Escape(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
