using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSafetyPro.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IInjuryFreeDaysService _injuryFreeDaysService;

    public DashboardController(IInjuryFreeDaysService injuryFreeDaysService) =>
        _injuryFreeDaysService = injuryFreeDaysService;

    [HttpGet("injury-free-days")]
    public async Task<ActionResult<ApiResponse<InjuryFreeDaysDto>>> GetInjuryFreeDays(
        CancellationToken cancellationToken)
    {
        var days = await _injuryFreeDaysService.GetCurrentInjuryFreeDaysAsync(cancellationToken);
        return Ok(ApiResponse<InjuryFreeDaysDto>.Ok(new InjuryFreeDaysDto(days)));
    }
}

[ApiController]
[Authorize(Policy = "RequireSafetyLead")]
[Route("api/v1/injuries")]
public class InjuriesController : ControllerBase
{
    private readonly IInjuryService _injuryService;
    private readonly IInjuryDashboardService _dashboardService;

    public InjuriesController(IInjuryService injuryService, IInjuryDashboardService dashboardService)
    {
        _injuryService = injuryService;
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InjuryDto>>>> GetAll(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<InjuryDto>>.Ok(await _injuryService.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InjuryDetailDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<InjuryDetailDto>.Ok(await _injuryService.GetByIdAsync(id, cancellationToken)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InjuryDetailDto>>> Create(
        [FromBody] CreateInjuryDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _injuryService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<InjuryDetailDto>.Ok(result, "Injury submitted."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InjuryDetailDto>>> Update(
        Guid id,
        [FromBody] UpdateInjuryDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<InjuryDetailDto>.Ok(
            await _injuryService.UpdateAsync(id, dto, cancellationToken),
            "Injury updated."));
}

[ApiController]
[Authorize(Policy = "RequireSafetyLead")]
[Route("api/v1/injury-reports")]
public class InjuryReportsController : ControllerBase
{
    private readonly IInjuryDashboardService _dashboardService;

    public InjuryReportsController(IInjuryDashboardService dashboardService) =>
        _dashboardService = dashboardService;

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<InjuryDashboardKpiDto>>> GetDashboard(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<InjuryDashboardKpiDto>.Ok(
            await _dashboardService.GetDashboardKpisAsync(cancellationToken)));

    [HttpGet("register")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InjuryRegisterRowDto>>>> GetRegister(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<InjuryRegisterRowDto>>.Ok(
            await _dashboardService.GetInjuryRegisterAsync(cancellationToken)));

    [HttpGet("trends")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InjuryTrendPointDto>>>> GetTrends(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<InjuryTrendPointDto>>.Ok(
            await _dashboardService.GetInjuryTrendsAsync(cancellationToken)));

    [HttpGet("frequency-rate")]
    public async Task<ActionResult<ApiResponse<InjuryFrequencyRateDto>>> GetFrequencyRate(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<InjuryFrequencyRateDto>.Ok(
            await _dashboardService.GetInjuryFrequencyRateAsync(cancellationToken)));

    [HttpGet("by-department")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NamedCountDto>>>> GetByDepartment(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<NamedCountDto>>.Ok(
            await _dashboardService.GetInjuriesByDepartmentAsync(cancellationToken)));

    [HttpGet("by-location")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NamedCountDto>>>> GetByLocation(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<NamedCountDto>>.Ok(
            await _dashboardService.GetInjuriesByLocationAsync(cancellationToken)));

    [HttpGet("by-section")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NamedCountDto>>>> GetBySection(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<NamedCountDto>>.Ok(
            await _dashboardService.GetInjuriesBySectionAsync(cancellationToken)));

    [HttpGet("by-body-part")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NamedCountDto>>>> GetByBodyPart(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<NamedCountDto>>.Ok(
            await _dashboardService.GetInjuriesByBodyPartAsync(cancellationToken)));

    [HttpGet("injury-free-days-history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InjuryFreeDaysHistoryPointDto>>>> GetInjuryFreeDaysHistory(
        CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<InjuryFreeDaysHistoryPointDto>>.Ok(
            await _dashboardService.GetInjuryFreeDaysHistoryAsync(cancellationToken)));
}
