using System.Text.Json;
using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Incidents;
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
[Route("api/v1/jsas")]
public class JobSafetyAssessmentsController : ControllerBase
{
    private readonly IJobSafetyAssessmentService _service;
    private readonly IJsaDraftWorkflowService _draftWorkflow;
    private readonly ILogger<JobSafetyAssessmentsController> _logger;

    public JobSafetyAssessmentsController(
        IJobSafetyAssessmentService service,
        IJsaDraftWorkflowService draftWorkflow,
        ILogger<JobSafetyAssessmentsController> logger)
    {
        _service = service;
        _draftWorkflow = draftWorkflow;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobSafetyAssessmentDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<JobSafetyAssessmentDto>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [HttpGet("summaries")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobSafetyAssessmentSummaryDto>>>> GetSummaries(
        [FromQuery] JsaStatus status,
        [FromQuery] string? department = null,
        [FromQuery] string? location = null,
        [FromQuery] string? section = null,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<JobSafetyAssessmentSummaryDto>>.Ok(
            await _draftWorkflow.GetMySummariesByStatusAsync(
                status,
                department,
                location,
                section,
                cancellationToken)));

    [HttpGet("reports")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobSafetyAssessmentDto>>>> GetReports(
        [FromQuery] string? department = null,
        [FromQuery] string? location = null,
        [FromQuery] string? section = null,
        [FromQuery] JsaStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(ApiResponse<IReadOnlyList<JobSafetyAssessmentDto>>.Ok(
            await _service.GetReportsAsync(department, location, section, status, cancellationToken)));
    }

    [HttpPost("drafts")]
    public async Task<ActionResult<ApiResponse<JobSafetyAssessmentDto>>> SaveDraft(
        [FromBody] SaveAssessmentDraftDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _draftWorkflow.SaveDraftAsync(dto, cancellationToken);
        return Ok(ApiResponse<JobSafetyAssessmentDto>.Ok(result, "Draft saved."));
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<ApiResponse<JobSafetyAssessmentDto>>> Submit(
        Guid id,
        [FromBody] CreateJobSafetyAssessmentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _draftWorkflow.SubmitAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<JobSafetyAssessmentDto>.Ok(result, "Assessment submitted."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<JobSafetyAssessmentDto>>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<JobSafetyAssessmentDto>.Ok(await _service.GetByIdAsync(id, cancellationToken)));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<JobSafetyAssessmentDto>>> Create(
        [FromBody] CreateJobSafetyAssessmentDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "POST /api/v1/jsas received. CompanyId={CompanyId}, PlantId={PlantId}, DepartmentId={DepartmentId}, Title={Title}",
            dto.CompanyId,
            dto.PlantId,
            dto.DepartmentId,
            dto.Title);

        _logger.LogDebug("POST /api/v1/jsas raw DTO: {Payload}", JsonSerializer.Serialize(dto));

        var result = await _service.CreateAsync(dto, cancellationToken);

        _logger.LogInformation("POST /api/v1/jsas completed. CreatedId={Id}", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<JobSafetyAssessmentDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<JobSafetyAssessmentDto>>> Update(
        Guid id,
        [FromBody] UpdateJobSafetyAssessmentDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<JobSafetyAssessmentDto>.Ok(await _service.UpdateAsync(id, dto, cancellationToken)));

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _draftWorkflow.DeleteDraftAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Draft deleted."));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/risk-levels")]
public class RiskLevelsController : ControllerBase
{
    private readonly IRiskLevelService _service;

    public RiskLevelsController(IRiskLevelService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RiskLevelDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<RiskLevelDto>>.Ok(await _service.GetAllAsync(cancellationToken)));
}

[ApiController]
[Authorize]
[Route("api/v1/incidents")]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _service;

    public IncidentsController(IIncidentService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IncidentDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<IncidentDto>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<IncidentDto>>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<IncidentDto>.Ok(await _service.GetByIdAsync(id, cancellationToken)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IncidentDto>>> Create([FromBody] CreateIncidentDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<IncidentDto>.Ok(result));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/near-misses")]
public class NearMissesController : ControllerBase
{
    private readonly INearMissService _service;

    public NearMissesController(INearMissService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NearMissDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<NearMissDto>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NearMissDto>>> Create([FromBody] CreateNearMissDto dto, CancellationToken cancellationToken)
        => Ok(ApiResponse<NearMissDto>.Ok(await _service.CreateAsync(dto, cancellationToken)));

    [HttpPost("reports")]
    public async Task<ActionResult<ApiResponse<NearMissDetailDto>>> CreateReport(
        [FromBody] CreateNearMissReportDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<NearMissDetailDto>.Ok(await _service.CreateReportAsync(dto, cancellationToken), "Near miss reported."));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<NearMissDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<NearMissDetailDto>.Ok(await _service.GetByIdAsync(id, cancellationToken)));

    [HttpPost("{id:guid}/investigate/start")]
    [Authorize(Policy = "RequireHseManager")]
    public async Task<ActionResult<ApiResponse<NearMissDetailDto>>> StartInvestigation(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<NearMissDetailDto>.Ok(await _service.StartInvestigationAsync(id, cancellationToken)));

    [HttpPut("{id:guid}/investigate")]
    [Authorize(Policy = "RequireHseManager")]
    public async Task<ActionResult<ApiResponse<NearMissDetailDto>>> Investigate(
        Guid id,
        [FromBody] InvestigateNearMissDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<NearMissDetailDto>.Ok(await _service.InvestigateAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "RequireHseManager")]
    public async Task<ActionResult<ApiResponse<NearMissDetailDto>>> Close(
        Guid id,
        [FromBody] CloseNearMissDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<NearMissDetailDto>.Ok(await _service.CloseAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/submit-verification")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<NearMissDetailDto>>> SubmitForVerification(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<NearMissDetailDto>.Ok(
            await _service.SubmitForVerificationAsync(id, cancellationToken),
            "Submitted for verification."));
}

[ApiController]
[Authorize]
[Route("api/v1/corrective-actions")]
public class CorrectiveActionsController : ControllerBase
{
    private readonly ICorrectiveActionService _service;

    public CorrectiveActionsController(ICorrectiveActionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CorrectiveActionDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<CorrectiveActionDto>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "RequireSupervisor")]
    public async Task<ActionResult<ApiResponse<CorrectiveActionDto>>> Create([FromBody] CreateCorrectiveActionDto dto, CancellationToken cancellationToken)
        => Ok(ApiResponse<CorrectiveActionDto>.Ok(await _service.CreateAsync(dto, cancellationToken)));

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<ApiResponse<CorrectiveActionDto>>> StartProgress(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<CorrectiveActionDto>.Ok(await _service.StartProgressAsync(id, cancellationToken)));

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<CorrectiveActionDto>>> Complete(
        Guid id,
        [FromBody] CompleteCorrectiveActionDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<CorrectiveActionDto>.Ok(await _service.CompleteAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/verify")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<CorrectiveActionDto>>> Verify(
        Guid id,
        [FromBody] VerifyCorrectiveActionDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<CorrectiveActionDto>.Ok(await _service.VerifyAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<CorrectiveActionDto>>> Close(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<CorrectiveActionDto>.Ok(await _service.CloseAsync(id, cancellationToken)));
}

[ApiController]
[Authorize]
[Route("api/v1/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _service;

    public AttachmentsController(IAttachmentService service) => _service = service;

    [HttpGet("{entityType}/{entityId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttachmentDto>>>> GetByEntity(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<AttachmentDto>>.Ok(await _service.GetByEntityAsync(entityType, entityId, cancellationToken)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> Create([FromBody] CreateAttachmentDto dto, CancellationToken cancellationToken)
        => Ok(ApiResponse<AttachmentDto>.Ok(await _service.CreateAsync(dto, cancellationToken)));
}
