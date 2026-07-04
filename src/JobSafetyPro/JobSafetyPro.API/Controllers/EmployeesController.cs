using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.Profile;
using JobSafetyPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSafetyPro.API.Controllers;

[ApiController]
[Authorize(Policy = "RequireSafetyLead")]
[Route("api/v1/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService) => _employeeService = employeeService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<EmployeeListItemDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] string? occupation,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.GetAllAsync(
            new EmployeeSearchQuery(search, department, occupation, page, pageSize),
            cancellationToken);
        return Ok(ApiResponse<PaginatedList<EmployeeListItemDto>>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PaginatedList<EmployeeListItemDto>>>> Search(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] string? occupation,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.SearchAsync(
            new EmployeeSearchQuery(search, department, occupation, page, pageSize),
            cancellationToken);
        return Ok(ApiResponse<PaginatedList<EmployeeListItemDto>>.Ok(result));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<EmployeeStatsDto>>> GetStats(
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<EmployeeStatsDto>.Ok(await _employeeService.GetStatsAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeDetailDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<EmployeeDetailDto>.Ok(await _employeeService.GetByIdAsync(id, cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<ActionResult<ApiResponse<EmployeeListItemDto>>> Create(
        [FromBody] CreateEmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.CreateEmployeeAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<EmployeeListItemDto>.Ok(result, "Employee created."));
    }
}
