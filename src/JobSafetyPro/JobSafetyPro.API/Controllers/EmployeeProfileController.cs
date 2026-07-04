using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.Profile;
using JobSafetyPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSafetyPro.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/employee-profiles")]
public class EmployeeProfileController : ControllerBase
{
    private readonly IEmployeeProfileService _service;

    public EmployeeProfileController(IEmployeeProfileService service) => _service = service;

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<EmployeeProfileDto?>>> GetMine(CancellationToken cancellationToken)
        => Ok(ApiResponse<EmployeeProfileDto?>.Ok(await _service.GetMyProfileAsync(cancellationToken)));

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<EmployeeProfileDto>>> SaveMine(
        [FromBody] SaveEmployeeProfileDto dto,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<EmployeeProfileDto>.Ok(await _service.SaveMyProfileAsync(dto, cancellationToken)));
}
