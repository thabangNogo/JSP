using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.MasterData;
using JobSafetyPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSafetyPro.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/work-lookups")]
public class WorkLookupsController : ControllerBase
{
    private readonly IWorkLookupService _service;

    public WorkLookupsController(IWorkLookupService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<WorkLookupsDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(ApiResponse<WorkLookupsDto>.Ok(await _service.GetAllAsync(cancellationToken)));
}
