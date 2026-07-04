using AutoMapper;
using FluentValidation;
using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Organization;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Entities.Shared;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateCompanyDto> _validator;

    public CompanyService(IUnitOfWork unitOfWork, IMapper mapper, IAuditService auditService, IValidator<CreateCompanyDto> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
        _validator = validator;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Companies.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CompanyDto>>(items);
    }

    public async Task<CompanyDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Companies.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Company), id);
        return _mapper.Map<CompanyDto>(entity);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var entity = _mapper.Map<Company>(dto);
        await _unitOfWork.Companies.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Create", nameof(Company), entity.Id, newValues: entity, cancellationToken: cancellationToken);
        return _mapper.Map<CompanyDto>(entity);
    }
}

public class PlantService : IPlantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public PlantService(IUnitOfWork unitOfWork, IMapper mapper, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<IReadOnlyList<PlantDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Plants.FindAsync(p => p.CompanyId == companyId, cancellationToken);
        return _mapper.Map<IReadOnlyList<PlantDto>>(items);
    }

    public async Task<PlantDto> CreateAsync(CreatePlantDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Plant>(dto);
        await _unitOfWork.Plants.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Create", nameof(Plant), entity.Id, newValues: entity, cancellationToken: cancellationToken);
        return _mapper.Map<PlantDto>(entity);
    }
}

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetByPlantAsync(Guid plantId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Departments.FindAsync(d => d.PlantId == plantId, cancellationToken);
        return _mapper.Map<IReadOnlyList<DepartmentDto>>(items);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Department>(dto);
        await _unitOfWork.Departments.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Create", nameof(Department), entity.Id, newValues: entity, cancellationToken: cancellationToken);
        return _mapper.Map<DepartmentDto>(entity);
    }
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateUserDto> _validator;

    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IValidator<CreateUserDto> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _validator = validator;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.FindAsync(_ => true, cancellationToken);
        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var withRoles = await _unitOfWork.Users.GetByIdWithRolesAsync(user.Id, cancellationToken) ?? user;
            result.Add(_mapper.Map<UserDto>(withRoles));
        }
        return result;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = new User
        {
            CompanyId = dto.CompanyId,
            PlantId = dto.PlantId,
            DepartmentId = dto.DepartmentId,
            Email = dto.Email,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmployeeNumber = dto.EmployeeNumber
        };

        var roles = await _unitOfWork.Roles.FindAsync(r => dto.Roles.Contains(r.Name), cancellationToken);
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = user.Id });
        }

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Create", nameof(User), user.Id, newValues: new { user.Email }, cancellationToken: cancellationToken);

        var created = await _unitOfWork.Users.GetByIdWithRolesAsync(user.Id, cancellationToken) ?? user;
        return _mapper.Map<UserDto>(created);
    }
}
