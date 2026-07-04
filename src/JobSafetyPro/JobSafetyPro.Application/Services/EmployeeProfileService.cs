using FluentValidation;
using JobSafetyPro.Application.DTOs.Profile;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public interface IEmployeeProfileService
{
    Task<EmployeeProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken = default);

    Task<EmployeeProfileDto> SaveMyProfileAsync(SaveEmployeeProfileDto dto, CancellationToken cancellationToken = default);
}

public class EmployeeProfileService : IEmployeeProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IValidator<SaveEmployeeProfileDto> _validator;

    public EmployeeProfileService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IValidator<SaveEmployeeProfileDto> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _validator = validator;
    }

    public async Task<EmployeeProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        var profile = await _unitOfWork.EmployeeProfiles.GetByUserIdAsync(userId, cancellationToken);
        return profile == null ? null : MapToDto(profile);
    }

    public async Task<EmployeeProfileDto> SaveMyProfileAsync(
        SaveEmployeeProfileDto dto,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        await EnsureWorkDepartmentExistsAsync(dto.WorkDepartmentId, cancellationToken);

        var userId = RequireUserId();
        var existing = await _unitOfWork.EmployeeProfiles.GetByUserIdAsync(userId, cancellationToken);

        if (existing == null)
        {
            var profile = new EmployeeProfile
            {
                UserId = userId,
                WorkDepartmentId = dto.WorkDepartmentId,
                Name = dto.Name.Trim(),
                Surname = dto.Surname.Trim(),
                CompanyNumber = dto.CompanyNumber.Trim(),
                Occupation = dto.Occupation.Trim(),
            };
            await _unitOfWork.EmployeeProfiles.AddAsync(profile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var created = await _unitOfWork.EmployeeProfiles.GetByUserIdAsync(userId, cancellationToken)
                ?? profile;
            var createdDto = MapToDto(created);
            await _auditService.LogAsync(
                "Create",
                nameof(EmployeeProfile),
                createdDto.Id,
                newValues: createdDto,
                cancellationToken: cancellationToken);
            return createdDto;
        }

        existing.WorkDepartmentId = dto.WorkDepartmentId;
        existing.Name = dto.Name.Trim();
        existing.Surname = dto.Surname.Trim();
        existing.CompanyNumber = dto.CompanyNumber.Trim();
        existing.Occupation = dto.Occupation.Trim();
        _unitOfWork.EmployeeProfiles.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var updated = await _unitOfWork.EmployeeProfiles.GetByUserIdAsync(userId, cancellationToken)
            ?? existing;
        var updatedDto = MapToDto(updated);
        await _auditService.LogAsync(
            "Update",
            nameof(EmployeeProfile),
            updatedDto.Id,
            newValues: updatedDto,
            cancellationToken: cancellationToken);
        return updatedDto;
    }

    private async Task EnsureWorkDepartmentExistsAsync(Guid workDepartmentId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.WorkDepartments.GetByIdAsync(workDepartmentId, cancellationToken) == null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(workDepartmentId)] = new[] { "Department does not exist." }
            });
        }
    }

    private Guid RequireUserId() =>
        _currentUserService.UserId ?? throw new UnauthorizedAppException("User is not authenticated.");

    private static EmployeeProfileDto MapToDto(EmployeeProfile profile) =>
        new(
            profile.Id,
            profile.UserId,
            profile.WorkDepartmentId,
            profile.WorkDepartment?.Name ?? string.Empty,
            profile.Name,
            profile.Surname,
            profile.CompanyNumber,
            profile.Occupation);
}
