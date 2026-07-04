using System.Collections.Concurrent;
using AutoMapper;
using FluentValidation;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobSafetyPro.Application.Services;

public interface IJsaDraftWorkflowService
{
    Task<IReadOnlyList<JobSafetyAssessmentSummaryDto>> GetMySummariesByStatusAsync(
        JsaStatus status,
        string? department = null,
        string? location = null,
        string? section = null,
        CancellationToken cancellationToken = default);

    Task<JobSafetyAssessmentDto> SaveDraftAsync(SaveAssessmentDraftDto dto, CancellationToken cancellationToken = default);

    Task<JobSafetyAssessmentDto> SubmitAsync(Guid id, CreateJobSafetyAssessmentDto dto, CancellationToken cancellationToken = default);

    Task DeleteDraftAsync(Guid id, CancellationToken cancellationToken = default);
}

public class JsaDraftWorkflowService : IJsaDraftWorkflowService
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> AssessmentLocks = new();

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmployeeProfileService _employeeProfileService;
    private readonly IWorkLookupService _workLookupService;
    private readonly IValidator<SaveAssessmentDraftDto> _draftValidator;
    private readonly IValidator<CreateJobSafetyAssessmentDto> _submitValidator;
    private readonly ILogger<JsaDraftWorkflowService> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IJsaWorkflowService _jsaWorkflow;

    public JsaDraftWorkflowService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IEmployeeProfileService employeeProfileService,
        IWorkLookupService workLookupService,
        IValidator<SaveAssessmentDraftDto> draftValidator,
        IValidator<CreateJobSafetyAssessmentDto> submitValidator,
        IDateTimeService dateTimeService,
        IJsaWorkflowService jsaWorkflow,
        ILogger<JsaDraftWorkflowService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _employeeProfileService = employeeProfileService;
        _workLookupService = workLookupService;
        _draftValidator = draftValidator;
        _submitValidator = submitValidator;
        _dateTimeService = dateTimeService;
        _jsaWorkflow = jsaWorkflow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JobSafetyAssessmentSummaryDto>> GetMySummariesByStatusAsync(
        JsaStatus status,
        string? department = null,
        string? location = null,
        string? section = null,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        var items = await _unitOfWork.JobSafetyAssessments.GetByStatusForUserAsync(
            status,
            userId,
            department,
            location,
            section,
            cancellationToken);
        return items.Select(j => new JobSafetyAssessmentSummaryDto(
            j.Id,
            j.Title,
            j.Status,
            j.CurrentStep,
            j.LastSavedAt ?? j.ModifiedDate ?? j.CreatedDate,
            j.CreatedDate,
            j.Department,
            j.Location,
            j.Section,
            j.WorkLocationId,
            j.WorkSectionId)).ToList();
    }

    public async Task<JobSafetyAssessmentDto> SaveDraftAsync(
        SaveAssessmentDraftDto dto,
        CancellationToken cancellationToken = default)
    {
        var lockId = dto.Id is { } id && id != Guid.Empty ? id : Guid.NewGuid();
        var semaphore = AssessmentLocks.GetOrAdd(lockId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            return await ExecuteWithConcurrencyRetryAsync(
                () => SaveDraftCoreAsync(dto, lockId, cancellationToken),
                lockId,
                "SaveDraft",
                cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<JobSafetyAssessmentDto> SubmitAsync(
        Guid id,
        CreateJobSafetyAssessmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var semaphore = AssessmentLocks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            return await ExecuteWithConcurrencyRetryAsync(
                () => SubmitCoreAsync(id, dto, cancellationToken),
                id,
                "Submit",
                cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<JobSafetyAssessmentDto> ExecuteWithConcurrencyRetryAsync(
        Func<Task<JobSafetyAssessmentDto>> action,
        Guid assessmentId,
        string operation,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (attempt < 2 && IsConcurrencyConflict(ex))
            {
                _unitOfWork.ClearChangeTracker();
                _logger.LogWarning(
                    ex,
                    "{Operation} concurrency conflict for {AssessmentId}, retrying",
                    operation,
                    assessmentId);
            }
        }

        throw new InvalidOperationException("Unreachable concurrency retry path.");
    }

    private static bool IsConcurrencyConflict(Exception ex) =>
        ex.GetType().Name.Contains("Concurrency", StringComparison.Ordinal)
        || (ex.InnerException != null && IsConcurrencyConflict(ex.InnerException));

    private async Task<JobSafetyAssessmentDto> SaveDraftCoreAsync(
        SaveAssessmentDraftDto dto,
        Guid lockId,
        CancellationToken cancellationToken)
    {
        await _draftValidator.ValidateAndThrowAsync(dto, cancellationToken);
        await ValidateOrganizationAsync(dto.CompanyId, dto.PlantId, dto.DepartmentId, cancellationToken);

        var userId = RequireUserId();
        var (department, location, section) = await ResolveJobSiteFieldsAsync(
            dto.WorkLocationId,
            dto.WorkSectionId,
            cancellationToken);
        var lastSavedAt = _dateTimeService.UtcNow;
        var modifiedBy = _currentUserService.Email ?? userId.ToString();
        var assessmentId = dto.Id is { } dtoId && dtoId != Guid.Empty ? dtoId : lockId;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            if (dto.Id is { } existingId && existingId != Guid.Empty)
            {
                var existing = await _unitOfWork.JobSafetyAssessments.GetByIdAsync(existingId, cancellationToken)
                    ?? throw new NotFoundException(nameof(JobSafetyAssessment), existingId);
                EnsureDraftOwnedByUser(existing, userId);

                await _unitOfWork.JobSafetyAssessments.ReplaceChildrenPhysicallyAsync(assessmentId, cancellationToken);
                _unitOfWork.ClearChangeTracker();

                var rows = await _unitOfWork.JobSafetyAssessments.UpdateDraftHeaderAsync(
                    assessmentId,
                    dto.Title.Trim(),
                    dto.JobDescription.Trim(),
                    department,
                    location,
                    section,
                    dto.WorkLocationId,
                    dto.WorkSectionId,
                    dto.CurrentStep,
                    dto.WorkflowDataJson,
                    dto.SignatureStoragePath?.Trim(),
                    dto.SignOffName?.Trim(),
                    dto.SignOffSurname?.Trim(),
                    dto.SignOffCompanyNumber?.Trim(),
                    dto.SignOffOccupation?.Trim(),
                    JsaStatus.Draft,
                    lastSavedAt,
                    userId,
                    modifiedBy,
                    cancellationToken);

                if (rows == 0)
                {
                    throw new NotFoundException(nameof(JobSafetyAssessment), assessmentId);
                }

                await InsertChildrenAsync(assessmentId, dto, requireRiskLevels: false, cancellationToken);
            }
            else
            {
                assessmentId = lockId;
                await _unitOfWork.JobSafetyAssessments.ReplaceChildrenPhysicallyAsync(assessmentId, cancellationToken);
                _unitOfWork.ClearChangeTracker();

                var entity = new JobSafetyAssessment
                {
                    Id = assessmentId,
                    CompanyId = dto.CompanyId,
                    PlantId = dto.PlantId,
                    DepartmentId = dto.DepartmentId,
                    CreatedByUserId = userId,
                    Title = dto.Title.Trim(),
                    JobDescription = dto.JobDescription.Trim(),
                    Department = department,
                    Location = location,
                    Section = section,
                    WorkLocationId = dto.WorkLocationId,
                    WorkSectionId = dto.WorkSectionId,
                    CurrentStep = dto.CurrentStep,
                    WorkflowDataJson = dto.WorkflowDataJson,
                    SignatureStoragePath = dto.SignatureStoragePath?.Trim(),
                    SignOffName = dto.SignOffName?.Trim(),
                    SignOffSurname = dto.SignOffSurname?.Trim(),
                    SignOffCompanyNumber = dto.SignOffCompanyNumber?.Trim(),
                    SignOffOccupation = dto.SignOffOccupation?.Trim(),
                    Status = JsaStatus.Draft,
                    LastSavedAt = lastSavedAt,
                };
                await _unitOfWork.JobSafetyAssessments.AddAsync(entity, cancellationToken);
                await InsertChildrenAsync(assessmentId, dto, requireRiskLevels: false, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
        await _auditService.LogAsync("SaveDraft", nameof(JobSafetyAssessment), assessmentId, cancellationToken: cancellationToken);

        return await MapDetailsAsync(assessmentId, cancellationToken);
    }

    private async Task<JobSafetyAssessmentDto> SubmitCoreAsync(
        Guid id,
        CreateJobSafetyAssessmentDto dto,
        CancellationToken cancellationToken)
    {
        await _submitValidator.ValidateAndThrowAsync(dto, cancellationToken);
        await ValidateOrganizationAsync(dto.CompanyId, dto.PlantId, dto.DepartmentId, cancellationToken);

        var userId = RequireUserId();

        dto = await EnrichSignOffFromProfileAsync(dto, cancellationToken);

        var (department, location, section) = await ResolveJobSiteFieldsAsync(
            dto.WorkLocationId,
            dto.WorkSectionId,
            cancellationToken);

        var saveDto = new SaveAssessmentDraftDto(
            id,
            dto.CompanyId,
            dto.PlantId,
            dto.DepartmentId,
            dto.Title,
            dto.JobDescription,
            dto.WorkLocationId,
            dto.WorkSectionId,
            CurrentStep: 4,
            SignatureStoragePath: dto.SignatureStoragePath,
            SignOffName: dto.SignOffName,
            SignOffSurname: dto.SignOffSurname,
            SignOffCompanyNumber: dto.SignOffCompanyNumber,
            SignOffOccupation: dto.SignOffOccupation,
            Hazards: dto.Hazards,
            Controls: dto.Controls);

        var existing = await _unitOfWork.JobSafetyAssessments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);
        EnsureDraftOwnedByUser(existing, userId);

        var lastSavedAt = _dateTimeService.UtcNow;
        var modifiedBy = _currentUserService.Email ?? userId.ToString();
        var jobDescription = BuildJobDescription(dto);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _unitOfWork.JobSafetyAssessments.ReplaceChildrenPhysicallyAsync(id, cancellationToken);
            _unitOfWork.ClearChangeTracker();

            var rows = await _unitOfWork.JobSafetyAssessments.UpdateDraftHeaderAsync(
                id,
                dto.Title.Trim(),
                jobDescription,
                department,
                location,
                section,
                dto.WorkLocationId,
                dto.WorkSectionId,
                currentStep: 4,
                workflowDataJson: null,
                signatureStoragePath: dto.SignatureStoragePath?.Trim(),
                signOffName: dto.SignOffName?.Trim(),
                signOffSurname: dto.SignOffSurname?.Trim(),
                signOffCompanyNumber: dto.SignOffCompanyNumber?.Trim(),
                signOffOccupation: dto.SignOffOccupation?.Trim(),
                JsaStatus.Submitted,
                lastSavedAt,
                userId,
                modifiedBy,
                cancellationToken);

            if (rows == 0)
            {
                throw new NotFoundException(nameof(JobSafetyAssessment), id);
            }

            await InsertChildrenAsync(id, saveDto, requireRiskLevels: true, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
        await _auditService.LogAsync("Submit", nameof(JobSafetyAssessment), id, cancellationToken: cancellationToken);

        await _jsaWorkflow.NotifySubmittedAsync(id, cancellationToken);

        return await MapDetailsAsync(id, cancellationToken);
    }

    public async Task DeleteDraftAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);

        EnsureDraftOwnedByUser(entity, userId);

        if (entity.Status != JsaStatus.Draft)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = new[] { "Only draft assessments can be deleted." }
            });
        }

        _unitOfWork.JobSafetyAssessments.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Delete", nameof(JobSafetyAssessment), entity.Id, cancellationToken: cancellationToken);
    }

    private async Task<CreateJobSafetyAssessmentDto> EnrichSignOffFromProfileAsync(
        CreateJobSafetyAssessmentDto dto,
        CancellationToken cancellationToken)
    {
        var profile = await _employeeProfileService.GetMyProfileAsync(cancellationToken);
        if (profile == null) return dto;

        return dto with
        {
            SignOffName = string.IsNullOrWhiteSpace(dto.SignOffName) ? profile.Name : dto.SignOffName,
            SignOffSurname = string.IsNullOrWhiteSpace(dto.SignOffSurname) ? profile.Surname : dto.SignOffSurname,
            SignOffCompanyNumber = string.IsNullOrWhiteSpace(dto.SignOffCompanyNumber)
                ? profile.CompanyNumber
                : dto.SignOffCompanyNumber,
            SignOffOccupation = string.IsNullOrWhiteSpace(dto.SignOffOccupation)
                ? profile.Occupation
                : dto.SignOffOccupation,
        };
    }

    private async Task InsertChildrenAsync(
        Guid assessmentId,
        SaveAssessmentDraftDto dto,
        bool requireRiskLevels,
        CancellationToken cancellationToken)
    {
        var hazards = dto.Hazards ?? Array.Empty<CreateJobSafetyAssessmentHazardDto>();
        var controls = dto.Controls ?? Array.Empty<CreateJobSafetyAssessmentControlDto>();
        var hazardIdMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        if (requireRiskLevels)
        {
            foreach (var hazardDto in hazards)
            {
                if (!hazardDto.RiskLevelId.HasValue || hazardDto.RiskLevelId == Guid.Empty)
                {
                    throw new ValidationAppException(new Dictionary<string, string[]>
                    {
                        ["Hazards"] = new[] { "Risk level is required for all hazards before submit." }
                    });
                }
            }
        }

        foreach (var hazardDto in hazards.OrderBy(h => h.SortOrder))
        {
            if (!hazardDto.RiskLevelId.HasValue || hazardDto.RiskLevelId == Guid.Empty)
                continue;

            var riskLevelId = hazardDto.RiskLevelId.Value;
            if (await _unitOfWork.RiskLevels.GetByIdAsync(riskLevelId, cancellationToken) == null)
            {
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    ["Hazards"] = new[] { $"Risk level '{riskLevelId}' does not exist." }
                });
            }

            var hazard = new Hazard
            {
                JobSafetyAssessmentId = assessmentId,
                Description = hazardDto.Description.Trim(),
                RiskLevelId = riskLevelId,
                ResidualRiskLevelId = hazardDto.ResidualRiskLevelId ?? riskLevelId,
                SortOrder = hazardDto.SortOrder,
            };

            await _unitOfWork.Hazards.AddAsync(hazard, cancellationToken);
            if (!string.IsNullOrWhiteSpace(hazardDto.ClientHazardId))
                hazardIdMap[hazardDto.ClientHazardId.Trim()] = hazard.Id;
        }

        foreach (var controlDto in controls)
        {
            Guid? linkedHazardId = null;
            if (!string.IsNullOrWhiteSpace(controlDto.ClientHazardId) &&
                hazardIdMap.TryGetValue(controlDto.ClientHazardId.Trim(), out var mappedHazardId))
            {
                linkedHazardId = mappedHazardId;
            }

            await _unitOfWork.ControlMeasures.AddAsync(
                new ControlMeasure
                {
                    JobSafetyAssessmentId = assessmentId,
                    HazardId = linkedHazardId,
                    Description = controlDto.Description.Trim(),
                    HierarchyOfControl = controlDto.HierarchyOfControl.Trim(),
                    IsImplemented = controlDto.IsImplemented,
                },
                cancellationToken);
        }
    }

    private static string BuildJobDescription(CreateJobSafetyAssessmentDto dto)
    {
        var sections = new List<string> { dto.JobDescription.Trim() };
        if (!string.IsNullOrWhiteSpace(dto.SignOffName))
        {
            sections.Add(
                $"Sign-off: {dto.SignOffName} {dto.SignOffSurname} | {dto.SignOffCompanyNumber} | {dto.SignOffOccupation}");
        }
        if (!string.IsNullOrWhiteSpace(dto.SignatureStoragePath))
            sections.Add($"Signature: {dto.SignatureStoragePath.Trim()}");
        return string.Join("\n\n", sections.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private async Task ValidateOrganizationAsync(
        Guid companyId,
        Guid plantId,
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Companies.GetByIdAsync(companyId, cancellationToken) == null)
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(companyId)] = new[] { "Company does not exist." }
            });

        var plant = await _unitOfWork.Plants.GetByIdAsync(plantId, cancellationToken);
        if (plant == null || plant.CompanyId != companyId)
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(plantId)] = new[] { "Plant does not exist for the specified company." }
            });

        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken);
        if (department == null || department.PlantId != plantId)
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(departmentId)] = new[] { "Department does not exist for the specified plant." }
            });
    }

    private async Task<(string Department, string Location, string Section)> ResolveJobSiteFieldsAsync(
        Guid workLocationId,
        Guid workSectionId,
        CancellationToken cancellationToken)
    {
        var profile = await _employeeProfileService.GetMyProfileAsync(cancellationToken)
            ?? throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Profile"] = new[] { "Complete your employee profile before continuing." }
            });

        if (profile.WorkDepartmentId == Guid.Empty || string.IsNullOrWhiteSpace(profile.WorkDepartmentName))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["WorkDepartmentId"] = new[] { "Select your department in your employee profile." }
            });
        }

        var location = await _workLookupService.GetLocationNameAsync(workLocationId, cancellationToken);
        var section = await _workLookupService.GetSectionNameAsync(workSectionId, cancellationToken);
        return (profile.WorkDepartmentName.Trim(), location.Trim(), section.Trim());
    }

    private Guid RequireUserId() =>
        _currentUserService.UserId ?? throw new UnauthorizedAppException("User is not authenticated.");

    private static void EnsureDraftOwnedByUser(JobSafetyAssessment entity, Guid userId)
    {
        if (entity.CreatedByUserId == null)
        {
            entity.CreatedByUserId = userId;
            return;
        }

        if (entity.CreatedByUserId != userId)
            throw new UnauthorizedAppException("You do not have access to this assessment.");
    }

    private async Task<JobSafetyAssessmentDto> MapDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);
        return _mapper.Map<JobSafetyAssessmentDto>(entity);
    }
}
