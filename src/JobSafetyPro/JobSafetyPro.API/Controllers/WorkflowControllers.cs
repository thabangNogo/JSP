using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.Workflow;
using JobSafetyPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSafetyPro.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/workflows")]
public class WorkflowControllers : ControllerBase
{
    private readonly IJsaWorkflowService _jsaWorkflow;
    private readonly IInjuryWorkflowService _injuryWorkflow;
    private readonly IManagerDashboardService _dashboard;
    private readonly IAuditQueryService _auditQuery;

    public WorkflowControllers(
        IJsaWorkflowService jsaWorkflow,
        IInjuryWorkflowService injuryWorkflow,
        IManagerDashboardService dashboard,
        IAuditQueryService auditQuery)
    {
        _jsaWorkflow = jsaWorkflow;
        _injuryWorkflow = injuryWorkflow;
        _dashboard = dashboard;
        _auditQuery = auditQuery;
    }

    [HttpGet("dashboard/pending-actions")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<ManagerPendingActionsDto>>> GetPendingActions(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<ManagerPendingActionsDto>.Ok(
            await _dashboard.GetPendingActionsAsync(cancellationToken)));

    [HttpGet("jsas/pending-approval")]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<object>>> GetPendingJsas(CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await _jsaWorkflow.GetPendingApprovalsAsync(cancellationToken)));

    [HttpGet("jsas/{id:guid}/review")]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<JsaReviewDto>>> GetJsaReview(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<JsaReviewDto>.Ok(await _jsaWorkflow.GetForReviewAsync(id, cancellationToken)));

    [HttpPost("jsas/{id:guid}/approve")]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<object>>> ApproveJsa(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await _jsaWorkflow.ApproveAsync(id, cancellationToken), "Assessment approved."));

    [HttpPost("jsas/{id:guid}/reject")]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<object>>> RejectJsa(
        Guid id,
        [FromBody] RejectJsaDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await _jsaWorkflow.RejectAsync(id, dto, cancellationToken), "Assessment rejected."));

    [HttpPost("injuries/{id:guid}/investigate/start")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<object>>> StartInjuryInvestigation(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await _injuryWorkflow.StartInvestigationAsync(id, cancellationToken)));

    [HttpPut("injuries/{id:guid}/investigate")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<object>>> RecordInjuryInvestigation(
        Guid id,
        [FromBody] InjuryInvestigationDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await _injuryWorkflow.RecordInvestigationAsync(id, dto, cancellationToken)));

    [HttpPost("injuries/{id:guid}/medical-treatment")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<object>>> SetMedicalTreatment(
        Guid id,
        [FromBody] InjuryMedicalOutcomeDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await _injuryWorkflow.SetMedicalTreatmentAsync(id, dto, cancellationToken)));

    [HttpPost("injuries/{id:guid}/return-to-work")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<object>>> SetReturnToWork(
        Guid id,
        [FromBody] InjuryReturnToWorkDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await _injuryWorkflow.SetReturnToWorkAsync(id, dto, cancellationToken)));

    [HttpPost("injuries/{id:guid}/close")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<object>>> CloseInjury(
        Guid id,
        [FromBody] CloseInjuryDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await _injuryWorkflow.CloseAsync(id, dto, cancellationToken)));

    [HttpGet("audit/{entityType}/{entityId:guid}")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AuditLogDto>>>> GetEntityAudit(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<AuditLogDto>>.Ok(
            await _auditQuery.GetByEntityAsync(entityType, entityId, cancellationToken)));

    [HttpGet("audit/recent")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AuditLogDto>>>> GetRecentAudit(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<AuditLogDto>>.Ok(
            await _auditQuery.GetRecentAsync(limit, cancellationToken)));
}
