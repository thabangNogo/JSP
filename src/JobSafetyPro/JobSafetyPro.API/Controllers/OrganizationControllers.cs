using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.DTOs.Organization;
using JobSafetyPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSafetyPro.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;

    public CompaniesController(ICompanyService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CompanyDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<CompanyDto>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<CompanyDto>.Ok(await _service.GetByIdAsync(id, cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Create([FromBody] CreateCompanyDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CompanyDto>.Ok(result));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class PlantsController : ControllerBase
{
    private readonly IPlantService _service;

    public PlantsController(IPlantService service) => _service = service;

    [HttpGet("by-company/{companyId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PlantDto>>>> GetByCompany(Guid companyId, CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<PlantDto>>.Ok(await _service.GetByCompanyAsync(companyId, cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<ActionResult<ApiResponse<PlantDto>>> Create([FromBody] CreatePlantDto dto, CancellationToken cancellationToken)
        => Ok(ApiResponse<PlantDto>.Ok(await _service.CreateAsync(dto, cancellationToken)));
}

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service) => _service = service;

    [HttpGet("by-plant/{plantId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentDto>>>> GetByPlant(Guid plantId, CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<DepartmentDto>>.Ok(await _service.GetByPlantAsync(plantId, cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create([FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken)
        => Ok(ApiResponse<DepartmentDto>.Ok(await _service.CreateAsync(dto, cancellationToken)));
}

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<UserDto>.Ok(await _service.GetByIdAsync(id, cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "RequireAdministrator")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
        => Ok(ApiResponse<UserDto>.Ok(await _service.CreateAsync(dto, cancellationToken)));
}
