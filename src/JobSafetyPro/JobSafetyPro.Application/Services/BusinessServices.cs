using AutoMapper;
using FluentValidation;
using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Workflow;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Entities.Shared;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobSafetyPro.Application.Services;

public class JobSafetyAssessmentService : IJobSafetyAssessmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateJobSafetyAssessmentDto> _validator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmployeeProfileService _employeeProfileService;
    private readonly IWorkLookupService _workLookupService;
    private readonly ILogger<JobSafetyAssessmentService> _logger;

    public JobSafetyAssessmentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditService auditService,
        IValidator<CreateJobSafetyAssessmentDto> validator,
        ICurrentUserService currentUserService,
        IEmployeeProfileService employeeProfileService,
        IWorkLookupService workLookupService,
        ILogger<JobSafetyAssessmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
        _validator = validator;
        _currentUserService = currentUserService;
        _employeeProfileService = employeeProfileService;
        _workLookupService = workLookupService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JobSafetyAssessmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<JobSafetyAssessmentDto>>(items);
    }

    public async Task<IReadOnlyList<JobSafetyAssessmentDto>> GetReportsAsync(
        string? department = null,
        string? location = null,
        string? section = null,
        JsaStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.JobSafetyAssessments.GetForReportsAsync(
            department,
            location,
            section,
            status,
            cancellationToken);
        return _mapper.Map<IReadOnlyList<JobSafetyAssessmentDto>>(items);
    }

    public async Task<JobSafetyAssessmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);
        return _mapper.Map<JobSafetyAssessmentDto>(entity);
    }

    public async Task<JobSafetyAssessmentDto> CreateAsync(CreateJobSafetyAssessmentDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Create JSA request received. CompanyId={CompanyId}, PlantId={PlantId}, DepartmentId={DepartmentId}, Title={Title}, HazardCount={HazardCount}, ControlCount={ControlCount}",
            dto.CompanyId,
            dto.PlantId,
            dto.DepartmentId,
            dto.Title,
            dto.Hazards?.Count ?? 0,
            dto.Controls?.Count ?? 0);

        _logger.LogDebug(
            "Create JSA DTO: {@Dto}",
            new
            {
                dto.CompanyId,
                dto.PlantId,
                dto.DepartmentId,
                dto.Title,
                dto.JobDescription,
                dto.ObserveStopNotes,
                dto.ConversationNotes,
                dto.SignatureStoragePath,
                Hazards = dto.Hazards?.Select(h => new
                {
                    h.ClientHazardId,
                    h.Description,
                    h.RiskLevelId,
                    h.ResidualRiskLevelId,
                    h.SortOrder
                }),
                Controls = dto.Controls?.Select(c => new
                {
                    c.ClientHazardId,
                    c.Description,
                    c.HierarchyOfControl,
                    c.IsImplemented
                })
            });

        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        _logger.LogInformation("Create JSA validation passed");

        await ValidateOrganizationReferencesAsync(dto, cancellationToken);

        var entity = _mapper.Map<JobSafetyAssessment>(dto);
        var profile = await _employeeProfileService.GetMyProfileAsync(cancellationToken)
            ?? throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Profile"] = new[] { "Complete your employee profile before submitting." }
            });
        entity.Department = profile.WorkDepartmentName;
        entity.WorkLocationId = dto.WorkLocationId;
        entity.WorkSectionId = dto.WorkSectionId;
        entity.Location = await _workLookupService.GetLocationNameAsync(dto.WorkLocationId, cancellationToken);
        entity.Section = await _workLookupService.GetSectionNameAsync(dto.WorkSectionId, cancellationToken);
        entity.Status = JsaStatus.Submitted;
        entity.CreatedByUserId = _currentUserService.UserId;
        entity.SignOffName = dto.SignOffName?.Trim();
        entity.SignOffSurname = dto.SignOffSurname?.Trim();
        entity.SignOffCompanyNumber = dto.SignOffCompanyNumber?.Trim();
        entity.SignOffOccupation = dto.SignOffOccupation?.Trim();
        entity.SignatureStoragePath = dto.SignatureStoragePath?.Trim();
        entity.JobDescription = BuildJobDescription(dto);

        _logger.LogInformation(
            "Mapped JSA entity before child collections. EntityId={EntityId}, Status={Status}",
            entity.Id,
            entity.Status);

        var hazardIdMap = await AttachHazardsAndControlsAsync(entity, dto, cancellationToken);

        _logger.LogInformation(
            "Entity ready for persistence. Hazards={HazardCount}, Controls={ControlCount}, HazardIdMap={MapCount}",
            entity.Hazards.Count,
            entity.ControlMeasures.Count,
            hazardIdMap.Count);

        try
        {
            await _unitOfWork.JobSafetyAssessments.AddAsync(entity, cancellationToken);
            _logger.LogInformation("Calling SaveChanges for JSA {EntityId}", entity.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("SaveChanges succeeded for JSA {EntityId}", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "SaveChanges failed for JSA {EntityId}. Message={Message} Inner={Inner}",
                entity.Id,
                ex.Message,
                ex.InnerException?.Message);

            throw;
        }

        var auditSnapshot = new
        {
            entity.Id,
            entity.CompanyId,
            entity.PlantId,
            entity.DepartmentId,
            entity.Title,
            entity.Status,
            HazardCount = entity.Hazards.Count,
            ControlCount = entity.ControlMeasures.Count
        };

        await _auditService.LogAsync(
            "Create",
            nameof(JobSafetyAssessment),
            entity.Id,
            newValues: auditSnapshot,
            cancellationToken: cancellationToken);

        var result = await GetByIdAsync(entity.Id, cancellationToken);
        _logger.LogInformation("Create JSA completed successfully. Id={Id}", result.Id);
        return result;
    }

    public async Task<JobSafetyAssessmentDto> UpdateAsync(Guid id, UpdateJobSafetyAssessmentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);

        var oldValues = new { entity.Title, entity.JobDescription, entity.Status };
        entity.Title = dto.Title;
        entity.JobDescription = dto.JobDescription;
        entity.Status = dto.Status;
        _unitOfWork.JobSafetyAssessments.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Update", nameof(JobSafetyAssessment), entity.Id, oldValues, entity, cancellationToken);
        return _mapper.Map<JobSafetyAssessmentDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);
        _unitOfWork.JobSafetyAssessments.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Delete", nameof(JobSafetyAssessment), entity.Id, cancellationToken: cancellationToken);
    }

    private async Task ValidateOrganizationReferencesAsync(
        CreateJobSafetyAssessmentDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating organization references for JSA create");

        if (await _unitOfWork.Companies.GetByIdAsync(dto.CompanyId, cancellationToken) == null)
        {
            _logger.LogWarning("Company not found: {CompanyId}", dto.CompanyId);
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(dto.CompanyId)] = new[] { "Company does not exist." }
            });
        }

        var plant = await _unitOfWork.Plants.GetByIdAsync(dto.PlantId, cancellationToken);
        if (plant == null || plant.CompanyId != dto.CompanyId)
        {
            _logger.LogWarning(
                "Plant not found or does not belong to company. PlantId={PlantId}, CompanyId={CompanyId}",
                dto.PlantId,
                dto.CompanyId);
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(dto.PlantId)] = new[] { "Plant does not exist for the specified company." }
            });
        }

        var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId, cancellationToken);
        if (department == null || department.PlantId != dto.PlantId)
        {
            _logger.LogWarning(
                "Department not found or does not belong to plant. DepartmentId={DepartmentId}, PlantId={PlantId}",
                dto.DepartmentId,
                dto.PlantId);
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(dto.DepartmentId)] = new[] { "Department does not exist for the specified plant." }
            });
        }

        _logger.LogInformation("Organization references validated successfully");
    }

    private static string BuildJobDescription(CreateJobSafetyAssessmentDto dto)
    {
        var sections = new List<string> { dto.JobDescription.Trim() };

        if (!string.IsNullOrWhiteSpace(dto.ObserveStopNotes))
            sections.Add($"Observe & Stop Notes:\n{dto.ObserveStopNotes.Trim()}");

        if (!string.IsNullOrWhiteSpace(dto.ConversationNotes))
            sections.Add($"Conversation Notes:\n{dto.ConversationNotes.Trim()}");

        if (!string.IsNullOrWhiteSpace(dto.SignatureStoragePath))
            sections.Add($"Signature (client path): {dto.SignatureStoragePath.Trim()}");

        return string.Join("\n\n", sections.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private async Task<Dictionary<string, Guid>> AttachHazardsAndControlsAsync(
        JobSafetyAssessment entity,
        CreateJobSafetyAssessmentDto dto,
        CancellationToken cancellationToken)
    {
        var hazardIdMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var hazards = dto.Hazards ?? Array.Empty<CreateJobSafetyAssessmentHazardDto>();
        var controls = dto.Controls ?? Array.Empty<CreateJobSafetyAssessmentControlDto>();

        if (hazards.Count == 0)
        {
            _logger.LogInformation("No hazards supplied on create request");
            return hazardIdMap;
        }

        var riskLevelIds = hazards
            .SelectMany(h => new Guid?[] { h.RiskLevelId, h.ResidualRiskLevelId })
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        foreach (var riskLevelId in riskLevelIds)
        {
            if (await _unitOfWork.RiskLevels.GetByIdAsync(riskLevelId, cancellationToken) == null)
            {
                _logger.LogWarning("Risk level not found: {RiskLevelId}", riskLevelId);
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    ["Hazards"] = new[] { $"Risk level '{riskLevelId}' does not exist." }
                });
            }
        }

        foreach (var hazardDto in hazards.OrderBy(h => h.SortOrder))
        {
            if (!hazardDto.RiskLevelId.HasValue || hazardDto.RiskLevelId == Guid.Empty)
                continue;

            var riskLevelId = hazardDto.RiskLevelId.Value;
            var hazard = new Hazard
            {
                JobSafetyAssessmentId = entity.Id,
                Description = hazardDto.Description.Trim(),
                RiskLevelId = riskLevelId,
                ResidualRiskLevelId = hazardDto.ResidualRiskLevelId ?? riskLevelId,
                SortOrder = hazardDto.SortOrder
            };

            entity.Hazards.Add(hazard);

            if (!string.IsNullOrWhiteSpace(hazardDto.ClientHazardId))
                hazardIdMap[hazardDto.ClientHazardId.Trim()] = hazard.Id;

            _logger.LogDebug(
                "Mapped hazard. ClientId={ClientId}, EntityId={EntityId}, RiskLevelId={RiskLevelId}",
                hazardDto.ClientHazardId,
                hazard.Id,
                hazard.RiskLevelId);
        }

        foreach (var controlDto in controls)
        {
            Guid? linkedHazardId = null;
            if (!string.IsNullOrWhiteSpace(controlDto.ClientHazardId) &&
                hazardIdMap.TryGetValue(controlDto.ClientHazardId.Trim(), out var mappedHazardId))
            {
                linkedHazardId = mappedHazardId;
            }

            var control = new ControlMeasure
            {
                JobSafetyAssessmentId = entity.Id,
                HazardId = linkedHazardId,
                Description = controlDto.Description.Trim(),
                HierarchyOfControl = controlDto.HierarchyOfControl.Trim(),
                IsImplemented = controlDto.IsImplemented
            };

            entity.ControlMeasures.Add(control);

            _logger.LogDebug(
                "Mapped control. Description={Description}, HazardId={HazardId}",
                control.Description,
                control.HazardId);
        }

        return hazardIdMap;
    }
}

public class RiskLevelService : IRiskLevelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RiskLevelService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RiskLevelDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.RiskLevels.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<RiskLevelDto>>(items);
    }
}

public class IncidentService : IIncidentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateIncidentDto> _validator;

    public IncidentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IValidator<CreateIncidentDto> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _validator = validator;
    }

    public async Task<IReadOnlyList<IncidentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Incidents.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<IncidentDto>>(items);
    }

    public async Task<IncidentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Incidents.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);
        return _mapper.Map<IncidentDto>(entity);
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var entity = _mapper.Map<Incident>(dto);
        entity.ReportedByUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        await _unitOfWork.Incidents.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Create", nameof(Incident), entity.Id, newValues: entity, cancellationToken: cancellationToken);
        return _mapper.Map<IncidentDto>(entity);
    }
}

public class NearMissService : INearMissService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IEmployeeProfileService _employeeProfileService;
    private readonly ISafetyNotificationDispatcher _notifications;
    private readonly IDateTimeService _dateTimeService;
    private readonly IValidator<CreateNearMissReportDto> _reportValidator;

    public NearMissService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IEmployeeProfileService employeeProfileService,
        ISafetyNotificationDispatcher notifications,
        IDateTimeService dateTimeService,
        IValidator<CreateNearMissReportDto> reportValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _employeeProfileService = employeeProfileService;
        _notifications = notifications;
        _dateTimeService = dateTimeService;
        _reportValidator = reportValidator;
    }

    public async Task<IReadOnlyList<NearMissDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.NearMisses.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<NearMissDto>>(items);
    }

    public async Task<NearMissDto> CreateAsync(CreateNearMissDto dto, CancellationToken cancellationToken = default)
    {
        var detail = await CreateReportAsync(
            new CreateNearMissReportDto(
                dto.CompanyId,
                dto.PlantId,
                dto.DepartmentId,
                string.Empty,
                string.Empty,
                NearMissCategory.Other,
                dto.Description,
                dto.OccurredAt),
            cancellationToken);
        return new NearMissDto(
            detail.Id,
            detail.CompanyId,
            detail.PlantId,
            detail.DepartmentId,
            detail.Department,
            detail.Location,
            detail.Section,
            detail.Category,
            detail.Description,
            detail.OccurredAt,
            detail.Status,
            null);
    }

    public async Task<NearMissDetailDto> CreateReportAsync(
        CreateNearMissReportDto dto,
        CancellationToken cancellationToken = default)
    {
        await _reportValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        if (!user.DepartmentId.HasValue || !user.PlantId.HasValue)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["departmentId"] = new[] { "Your account is missing plant or department assignment. Contact your administrator." },
            });
        }

        var profile = await _employeeProfileService.GetMyProfileAsync(cancellationToken);

        var entity = new NearMiss
        {
            CompanyId = user.CompanyId,
            PlantId = user.PlantId.Value,
            DepartmentId = user.DepartmentId.Value,
            Department = profile?.WorkDepartmentName ?? string.Empty,
            Location = dto.Location.Trim(),
            Section = dto.Section.Trim(),
            Category = dto.Category,
            Description = dto.Description.Trim(),
            OccurredAt = dto.OccurredAt,
            ReportedByUserId = userId,
            Status = NearMissStatus.Submitted,
            CreatedDate = _dateTimeService.UtcNow,
            CreatedBy = _currentUserService.Email ?? userId.ToString(),
        };

        await _unitOfWork.NearMisses.AddAsync(entity, cancellationToken);
        await SaveNearMissPhotosAsync(entity.Id, dto.PhotoStoragePaths, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var title = "New Near Miss Reported";
        var message = $"A Near Miss has been reported in {entity.Location} - {entity.Section}";

        await _notifications.NotifyRolesAsync(
            new[] { AppRoles.SafetyOfficer, AppRoles.Supervisor, AppRoles.SafetyManager },
            title,
            message,
            NotificationPriority.High,
            WorkflowNotificationType.NearMissSubmitted,
            relatedEntityType: RelatedEntityTypes.NearMiss,
            relatedEntityId: entity.Id,
            cancellationToken);

        return MapDetail(entity);
    }

    public async Task<NearMissDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.NearMisses.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(NearMiss), id);
        return MapDetail(entity);
    }

    public async Task<NearMissDetailDto> StartInvestigationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.NearMisses.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(NearMiss), id);
        entity.Status = NearMissStatus.UnderInvestigation;
        entity.InvestigatorUserId = _currentUserService.UserId;
        _unitOfWork.NearMisses.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapDetail(entity);
    }

    public async Task<NearMissDetailDto> InvestigateAsync(
        Guid id,
        InvestigateNearMissDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.NearMisses.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(NearMiss), id);
        entity.InvestigatorUserId = dto.InvestigatorUserId;
        entity.InvestigationNotes = dto.InvestigationNotes.Trim();
        entity.RootCause = dto.RootCause.Trim();
        entity.RootCauseCategory = dto.RootCauseCategory;
        entity.CorrectiveActionPlan = dto.CorrectiveActionPlan.Trim();
        entity.ResponsiblePersonUserId = dto.ResponsiblePersonUserId;
        entity.TargetDate = dto.TargetDate;
        entity.Status = NearMissStatus.CorrectiveActionAssigned;

        await _unitOfWork.CorrectiveActions.AddAsync(
            new CorrectiveAction
            {
                NearMissId = entity.Id,
                Description = dto.CorrectiveActionPlan.Trim(),
                AssignedToUserId = dto.ResponsiblePersonUserId,
                DueDate = dto.TargetDate,
                Status = CorrectiveActionStatus.Assigned,
                AssignedAt = _dateTimeService.UtcNow,
                CreatedDate = _dateTimeService.UtcNow,
                CreatedBy = _currentUserService.Email ?? "system",
            },
            cancellationToken);

        _unitOfWork.NearMisses.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersAsync(
            new[] { dto.ResponsiblePersonUserId },
            "Corrective Action Assigned",
            $"You have been assigned a corrective action for near miss at {entity.Location} - {entity.Section}.",
            NotificationPriority.High,
            WorkflowNotificationType.CorrectiveActionAssigned,
            relatedEntityType: RelatedEntityTypes.NearMiss,
            relatedEntityId: entity.Id,
            cancellationToken);

        await _auditService.LogAsync("Investigate", nameof(NearMiss), entity.Id, cancellationToken: cancellationToken);
        return MapDetail(entity);
    }

    public async Task<NearMissDetailDto> CloseAsync(
        Guid id,
        CloseNearMissDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.NearMisses.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(NearMiss), id);

        if (entity.Status != NearMissStatus.AwaitingVerification)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Near miss must be awaiting verification before closing." },
            });
        }

        entity.ClosureNotes = dto.ClosureNotes.Trim();
        entity.Status = NearMissStatus.Closed;
        _unitOfWork.NearMisses.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Close", nameof(NearMiss), entity.Id, cancellationToken: cancellationToken);
        return MapDetail(entity);
    }

    public async Task<NearMissDetailDto> SubmitForVerificationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.NearMisses.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(NearMiss), id);

        if (entity.Status != NearMissStatus.CorrectiveActionAssigned)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Corrective actions must be assigned before verification." },
            });
        }

        entity.Status = NearMissStatus.AwaitingVerification;
        _unitOfWork.NearMisses.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyRolesAsync(
            new[] { AppRoles.SafetyManager, AppRoles.HseManager },
            "Near Miss Awaiting Verification",
            $"Near miss at {entity.Location} is ready for verification.",
            NotificationPriority.High,
            WorkflowNotificationType.NearMissAssigned,
            relatedEntityType: RelatedEntityTypes.NearMiss,
            relatedEntityId: entity.Id,
            cancellationToken);

        await _auditService.LogAsync("AwaitingVerification", nameof(NearMiss), entity.Id, cancellationToken: cancellationToken);
        return MapDetail(entity);
    }

    private async Task SaveNearMissPhotosAsync(
        Guid entityId,
        IReadOnlyList<string>? paths,
        CancellationToken cancellationToken)
    {
        if (paths == null || paths.Count == 0) return;
        foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            await _unitOfWork.Attachments.AddAsync(
                new Attachment
                {
                    RelatedEntityType = RelatedEntityTypes.NearMiss,
                    RelatedEntityId = entityId,
                    FileName = System.IO.Path.GetFileName(path),
                    ContentType = "image/jpeg",
                    StoragePath = path,
                    FileSizeBytes = 0,
                    CreatedDate = _dateTimeService.UtcNow,
                    CreatedBy = _currentUserService.Email ?? "mobile",
                },
                cancellationToken);
        }
    }

    private static NearMissDetailDto MapDetail(NearMiss e) =>
        new(
            e.Id,
            e.CompanyId,
            e.PlantId,
            e.DepartmentId,
            e.Department,
            e.Location,
            e.Section,
            e.Category,
            e.Description,
            e.OccurredAt,
            e.ReportedByUserId,
            e.Status,
            e.InvestigatorUserId,
            e.InvestigationNotes,
            e.RootCause,
            e.RootCauseCategory,
            e.CorrectiveActionPlan,
            e.ResponsiblePersonUserId,
            e.TargetDate,
            e.ClosureNotes,
            e.CreatedDate);
}

public class CorrectiveActionService : ICorrectiveActionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateCorrectiveActionDto> _validator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISafetyNotificationDispatcher _notifications;
    private readonly IDateTimeService _dateTimeService;

    public CorrectiveActionService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditService auditService,
        IValidator<CreateCorrectiveActionDto> validator,
        ICurrentUserService currentUserService,
        ISafetyNotificationDispatcher notifications,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
        _validator = validator;
        _currentUserService = currentUserService;
        _notifications = notifications;
        _dateTimeService = dateTimeService;
    }

    public async Task<IReadOnlyList<CorrectiveActionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.CorrectiveActions.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CorrectiveActionDto>>(items);
    }

    public async Task<CorrectiveActionDto> CreateAsync(CreateCorrectiveActionDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var entity = _mapper.Map<CorrectiveAction>(dto);
        entity.Status = CorrectiveActionStatus.Assigned;
        entity.AssignedAt = _dateTimeService.UtcNow;
        await _unitOfWork.CorrectiveActions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Create", nameof(CorrectiveAction), entity.Id, newValues: entity, cancellationToken: cancellationToken);

        await _notifications.NotifyUsersAsync(
            new[] { entity.AssignedToUserId },
            "Corrective Action Assigned",
            entity.Description,
            NotificationPriority.High,
            WorkflowNotificationType.CorrectiveActionAssigned,
            relatedEntityType: RelatedEntityTypes.CorrectiveAction,
            relatedEntityId: entity.Id,
            cancellationToken);

        return _mapper.Map<CorrectiveActionDto>(entity);
    }

    public async Task<CorrectiveActionDto> StartProgressAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetOwnedOrAssigned(id, cancellationToken);
        entity.Status = CorrectiveActionStatus.InProgress;
        await SaveTransition(entity, "StartProgress", cancellationToken);
        return _mapper.Map<CorrectiveActionDto>(entity);
    }

    public async Task<CorrectiveActionDto> CompleteAsync(
        Guid id,
        CompleteCorrectiveActionDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetOwnedOrAssigned(id, cancellationToken);
        entity.Status = CorrectiveActionStatus.Completed;
        entity.CompletedAt = _dateTimeService.UtcNow;
        await SaveTransition(entity, "Complete", cancellationToken);

        await _notifications.NotifyRolesAsync(
            new[] { AppRoles.SafetyManager, AppRoles.SafetyOfficer },
            "Corrective Action Completed",
            $"Corrective action completed: {entity.Description}",
            NotificationPriority.High,
            WorkflowNotificationType.CorrectiveActionCompleted,
            relatedEntityType: RelatedEntityTypes.CorrectiveAction,
            relatedEntityId: entity.Id,
            cancellationToken);

        if (entity.NearMissId.HasValue)
        {
            var nearMiss = await _unitOfWork.NearMisses.GetByIdAsync(entity.NearMissId.Value, cancellationToken);
            if (nearMiss != null && nearMiss.Status == NearMissStatus.CorrectiveActionAssigned)
            {
                var related = await _unitOfWork.CorrectiveActions.FindAsync(
                    c => c.NearMissId == nearMiss.Id, cancellationToken);
                if (related.All(c => c.Status is CorrectiveActionStatus.Completed or CorrectiveActionStatus.Verified or CorrectiveActionStatus.Closed))
                {
                    nearMiss.Status = NearMissStatus.AwaitingVerification;
                    _unitOfWork.NearMisses.Update(nearMiss);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }

        return _mapper.Map<CorrectiveActionDto>(entity);
    }

    public async Task<CorrectiveActionDto> VerifyAsync(
        Guid id,
        VerifyCorrectiveActionDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();
        var entity = await _unitOfWork.CorrectiveActions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CorrectiveAction), id);
        if (entity.Status != CorrectiveActionStatus.Completed)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Only completed actions can be verified." },
            });
        }

        entity.Status = CorrectiveActionStatus.Verified;
        entity.VerifiedByUserId = _currentUserService.UserId;
        entity.VerifiedAt = _dateTimeService.UtcNow;
        await SaveTransition(entity, "Verify", cancellationToken);
        return _mapper.Map<CorrectiveActionDto>(entity);
    }

    public async Task<CorrectiveActionDto> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();
        var entity = await _unitOfWork.CorrectiveActions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CorrectiveAction), id);
        if (entity.Status != CorrectiveActionStatus.Verified)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Only verified actions can be closed." },
            });
        }

        entity.Status = CorrectiveActionStatus.Closed;
        entity.ClosedAt = _dateTimeService.UtcNow;
        await SaveTransition(entity, "Close", cancellationToken);
        return _mapper.Map<CorrectiveActionDto>(entity);
    }

    private async Task<CorrectiveAction> GetOwnedOrAssigned(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var entity = await _unitOfWork.CorrectiveActions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CorrectiveAction), id);
        if (entity.AssignedToUserId != userId && !IsSupervisor())
        {
            throw new UnauthorizedAppException("Not assigned to this corrective action.");
        }

        return entity;
    }

    private bool IsSupervisor() =>
        _currentUserService.Roles.Any(r =>
            r is AppRoles.Administrator or AppRoles.HseManager or AppRoles.Supervisor or AppRoles.SafetyManager);

    private void EnsureSafetyLead()
    {
        if (!_currentUserService.Roles.Any(r =>
                r is AppRoles.Administrator or AppRoles.HseManager or AppRoles.SafetyManager
                    or AppRoles.SafetyOfficer))
        {
            throw new UnauthorizedAppException("Safety lead access required.");
        }
    }

    private async Task SaveTransition(CorrectiveAction entity, string action, CancellationToken cancellationToken)
    {
        entity.ModifiedDate = _dateTimeService.UtcNow;
        entity.ModifiedBy = _currentUserService.Email ?? entity.ModifiedBy;
        _unitOfWork.CorrectiveActions.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(action, RelatedEntityTypes.CorrectiveAction, entity.Id, cancellationToken: cancellationToken);
    }
}

public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public AttachmentService(IUnitOfWork unitOfWork, IMapper mapper, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<AttachmentDto> CreateAsync(CreateAttachmentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Attachment>(dto);
        await _unitOfWork.Attachments.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Create", nameof(Attachment), entity.Id, newValues: entity, cancellationToken: cancellationToken);
        return _mapper.Map<AttachmentDto>(entity);
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Attachments.FindAsync(
            a => a.RelatedEntityType == entityType && a.RelatedEntityId == entityId,
            cancellationToken);
        return _mapper.Map<IReadOnlyList<AttachmentDto>>(items);
    }
}
