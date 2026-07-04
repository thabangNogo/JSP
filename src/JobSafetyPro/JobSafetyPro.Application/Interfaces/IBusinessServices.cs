using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Organization;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.DTOs.Workflow;
using JobSafetyPro.Domain.Enums;

// Safety enhancement DTOs in Safety namespace

namespace JobSafetyPro.Application.Interfaces;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CompanyDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CompanyDto> CreateAsync(CreateCompanyDto dto, CancellationToken cancellationToken = default);
}

public interface IPlantService
{
    Task<IReadOnlyList<PlantDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<PlantDto> CreateAsync(CreatePlantDto dto, CancellationToken cancellationToken = default);
}

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetByPlantAsync(Guid plantId, CancellationToken cancellationToken = default);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);
}

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
}

public interface IJobSafetyAssessmentService
{
    Task<IReadOnlyList<JobSafetyAssessmentDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSafetyAssessmentDto>> GetReportsAsync(
        string? department = null,
        string? location = null,
        string? section = null,
        JsaStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<JobSafetyAssessmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobSafetyAssessmentDto> CreateAsync(CreateJobSafetyAssessmentDto dto, CancellationToken cancellationToken = default);
    Task<JobSafetyAssessmentDto> UpdateAsync(Guid id, UpdateJobSafetyAssessmentDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRiskLevelService
{
    Task<IReadOnlyList<RiskLevelDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IIncidentService
{
    Task<IReadOnlyList<IncidentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IncidentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IncidentDto> CreateAsync(CreateIncidentDto dto, CancellationToken cancellationToken = default);
}

public interface INearMissService
{
    Task<IReadOnlyList<NearMissDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NearMissDto> CreateAsync(CreateNearMissDto dto, CancellationToken cancellationToken = default);
    Task<NearMissDetailDto> CreateReportAsync(CreateNearMissReportDto dto, CancellationToken cancellationToken = default);
    Task<NearMissDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NearMissDetailDto> StartInvestigationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NearMissDetailDto> InvestigateAsync(Guid id, InvestigateNearMissDto dto, CancellationToken cancellationToken = default);
    Task<NearMissDetailDto> SubmitForVerificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NearMissDetailDto> CloseAsync(Guid id, CloseNearMissDto dto, CancellationToken cancellationToken = default);
}

public interface ICorrectiveActionService
{
    Task<IReadOnlyList<CorrectiveActionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CorrectiveActionDto> CreateAsync(CreateCorrectiveActionDto dto, CancellationToken cancellationToken = default);
    Task<CorrectiveActionDto> StartProgressAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CorrectiveActionDto> CompleteAsync(Guid id, CompleteCorrectiveActionDto dto, CancellationToken cancellationToken = default);
    Task<CorrectiveActionDto> VerifyAsync(Guid id, VerifyCorrectiveActionDto dto, CancellationToken cancellationToken = default);
    Task<CorrectiveActionDto> CloseAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IAttachmentService
{
    Task<AttachmentDto> CreateAsync(CreateAttachmentDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttachmentDto>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
}
