using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.Ppe;
using JobSafetyPro.Application.Services;
using JobSafetyPro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSafetyPro.API.Controllers;

[ApiController]
[Authorize(Policy = "RequireSafetyLead")]
[Route("api/v1/ppe/catalogue")]
public class PpeCatalogueController : ControllerBase
{
    private readonly IPpeCatalogueService _service;

    public PpeCatalogueController(IPpeCatalogueService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeCatalogueItemDto>>>> GetAll(
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeCatalogueItemDto>>.Ok(await _service.GetAllAsync(activeOnly, cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PpeCatalogueItemDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeCatalogueItemDto>.Ok(await _service.GetByIdAsync(id, cancellationToken)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PpeCatalogueItemDto>>> Create(
        [FromBody] CreatePpeCatalogueItemDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PpeCatalogueItemDto>.Ok(result, "PPE catalogue item created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PpeCatalogueItemDto>>> Update(
        Guid id,
        [FromBody] UpdatePpeCatalogueItemDto dto,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeCatalogueItemDto>.Ok(await _service.UpdateAsync(id, dto, cancellationToken), "Updated."));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Deleted."));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/ppe/requests")]
public class PpeRequestsController : ControllerBase
{
    private readonly IPpeRequestService _service;

    public PpeRequestsController(IPpeRequestService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeRequestListDto>>>> GetAll(
        [FromQuery] PpeRequestFilterDto filter,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeRequestListDto>>.Ok(await _service.GetAllAsync(filter, cancellationToken)));

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<MyPpeSummaryDto>>> GetMy(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<MyPpeSummaryDto>.Ok(await _service.GetMyPpeAsync(cancellationToken)));

    [HttpGet("employee/{employeeUserId:guid}/history")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EmployeePpeHistoryDto>>>> GetEmployeeHistory(
        Guid employeeUserId,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<EmployeePpeHistoryDto>>.Ok(
            await _service.GetEmployeeHistoryAsync(employeeUserId, cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.GetByIdAsync(id, cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Create(
        [FromBody] CreatePpeRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PpeRequestDetailDto>.Ok(result, "PPE request created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Update(
        Guid id,
        [FromBody] UpdatePpeRequestDto dto,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.UpdateAsync(id, dto, cancellationToken), "Updated."));

    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Submit(Guid id, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.SubmitForApprovalAsync(id, cancellationToken)));

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Approve(Guid id, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.ApproveAsync(id, cancellationToken)));

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Reject(
        Guid id,
        [FromBody] RejectPpeRequestDto dto,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.RejectAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/preparing")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> StartPreparing(Guid id, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.StartPreparingAsync(id, cancellationToken)));

    [HttpPost("{id:guid}/dispatch")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Dispatch(
        Guid id,
        [FromBody] DispatchPpeRequestDto dto,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.DispatchAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/collect")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Collect(
        Guid id,
        [FromBody] CollectPpeRequestDto dto,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.CollectAsync(id, dto, cancellationToken)));

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Complete(Guid id, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.CompleteAsync(id, cancellationToken)));

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Archive(Guid id, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.ArchiveAsync(id, cancellationToken)));

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "RequireSafetyLead")]
    public async Task<ActionResult<ApiResponse<PpeRequestDetailDto>>> Cancel(Guid id, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeRequestDetailDto>.Ok(await _service.CancelAsync(id, cancellationToken)));
}

[ApiController]
[Authorize(Policy = "RequireSafetyLead")]
[Route("api/v1/ppe/dashboard")]
public class PpeDashboardController : ControllerBase
{
    private readonly IPpeDashboardService _service;

    public PpeDashboardController(IPpeDashboardService service) => _service = service;

    [HttpGet("kpis")]
    public async Task<ActionResult<ApiResponse<PpeDashboardKpiDto>>> GetKpis(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<PpeDashboardKpiDto>.Ok(await _service.GetDashboardKpisAsync(cancellationToken)));
}

[ApiController]
[Authorize(Policy = "RequireSafetyLead")]
[Route("api/v1/ppe/reports")]
public class PpeReportsController : ControllerBase
{
    private readonly IPpeReportService _service;

    public PpeReportsController(IPpeReportService service) => _service = service;

    [HttpGet("outstanding")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeRequestListDto>>>> Outstanding(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeRequestListDto>>.Ok(await _service.GetOutstandingRequestsAsync(cancellationToken)));

    [HttpGet("issued-register")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeRequestListDto>>>> IssuedRegister(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeRequestListDto>>.Ok(await _service.GetIssuedRegisterAsync(cancellationToken)));

    [HttpGet("by-department")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeReportRowDto>>>> ByDepartment(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeReportRowDto>>.Ok(await _service.GetByDepartmentAsync(cancellationToken)));

    [HttpGet("by-employee")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeReportRowDto>>>> ByEmployee(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeReportRowDto>>.Ok(await _service.GetByEmployeeAsync(cancellationToken)));

    [HttpGet("by-item")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeReportRowDto>>>> ByItem(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeReportRowDto>>.Ok(await _service.GetByItemAsync(cancellationToken)));

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeRequestListDto>>>> Overdue(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeRequestListDto>>.Ok(await _service.GetOverdueAsync(cancellationToken)));

    [HttpGet("usage-trends")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PpeReportRowDto>>>> UsageTrends(CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<PpeReportRowDto>>.Ok(await _service.GetUsageTrendsAsync(cancellationToken)));
}
