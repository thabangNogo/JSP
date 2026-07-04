using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.DTOs.Workflow;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;
using AutoMapper;

namespace JobSafetyPro.Application.Services;

public interface IJsaWorkflowService
{
    Task<IReadOnlyList<JobSafetyAssessmentSummaryDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<JsaReviewDto> GetForReviewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobSafetyAssessmentDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobSafetyAssessmentDto> RejectAsync(Guid id, RejectJsaDto dto, CancellationToken cancellationToken = default);
    Task NotifySubmittedAsync(Guid assessmentId, CancellationToken cancellationToken = default);
}

public class JsaWorkflowService : IJsaWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ISafetyNotificationDispatcher _notifications;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly IJobSafetyAssessmentService _assessmentService;

    public JsaWorkflowService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ISafetyNotificationDispatcher notifications,
        IAuditService auditService,
        IMapper mapper,
        IJobSafetyAssessmentService assessmentService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _notifications = notifications;
        _auditService = auditService;
        _mapper = mapper;
        _assessmentService = assessmentService;
    }

    public async Task<IReadOnlyList<JobSafetyAssessmentSummaryDto>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureSupervisor();
        var items = await _unitOfWork.JobSafetyAssessments.GetForReportsAsync(
            status: JsaStatus.Submitted,
            cancellationToken: cancellationToken);
        return items.Select(j => new JobSafetyAssessmentSummaryDto(
            j.Id,
            j.Title,
            j.Status,
            j.CurrentStep,
            j.LastSavedAt,
            j.CreatedDate,
            j.Department,
            j.Location,
            j.Section,
            j.WorkLocationId,
            j.WorkSectionId)).ToList();
    }

    public async Task<JsaReviewDto> GetForReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureSupervisor();
        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);

        var employeeName = entity.SignOffName != null && entity.SignOffSurname != null
            ? $"{entity.SignOffName} {entity.SignOffSurname}"
            : null;

        return new JsaReviewDto(
            entity.Id,
            entity.Title,
            entity.JobDescription,
            entity.Department,
            entity.Location,
            entity.Section,
            entity.Status,
            employeeName,
            entity.SignOffName,
            entity.SignOffSurname,
            entity.SignOffCompanyNumber,
            entity.SignOffOccupation,
            entity.SignatureStoragePath,
            entity.RejectionReason,
            _mapper.Map<IReadOnlyList<HazardDto>>(entity.Hazards),
            _mapper.Map<IReadOnlyList<ControlMeasureDto>>(entity.ControlMeasures));
    }

    public async Task<JobSafetyAssessmentDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureSupervisor();
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");

        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);

        if (entity.Status != JsaStatus.Submitted)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Only submitted assessments can be approved." },
            });
        }

        var oldStatus = entity.Status;
        var now = _dateTimeService.UtcNow;
        entity.Status = JsaStatus.Approved;
        entity.ApprovedByUserId = userId;
        entity.ApprovedAt = now;
        entity.ValidFrom = now;
        entity.RejectionReason = null;
        entity.RejectedByUserId = null;
        entity.RejectedAt = null;
        entity.ModifiedDate = now;
        entity.ModifiedBy = _currentUserService.Email ?? userId.ToString();

        _unitOfWork.JobSafetyAssessments.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "Approve",
            nameof(JobSafetyAssessment),
            entity.Id,
            new { Status = oldStatus },
            new { Status = entity.Status, entity.ApprovedByUserId, entity.ApprovedAt },
            cancellationToken);

        if (entity.CreatedByUserId.HasValue)
        {
            await _notifications.NotifyUsersAsync(
                new[] { entity.CreatedByUserId.Value },
                "Assessment Approved",
                $"Your assessment \"{entity.Title}\" has been approved. You may begin work.",
                NotificationPriority.High,
                WorkflowNotificationType.AssessmentApproved,
                relatedEntityType: RelatedEntityTypes.JobSafetyAssessment,
                relatedEntityId: entity.Id,
                cancellationToken);
        }

        return await _assessmentService.GetByIdAsync(id, cancellationToken);
    }

    public async Task<JobSafetyAssessmentDto> RejectAsync(
        Guid id,
        RejectJsaDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSupervisor();
        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["rejectionReason"] = new[] { "Rejection reason is required." },
            });
        }

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");

        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobSafetyAssessment), id);

        if (entity.Status != JsaStatus.Submitted)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Only submitted assessments can be rejected." },
            });
        }

        var oldStatus = entity.Status;
        var now = _dateTimeService.UtcNow;
        entity.Status = JsaStatus.Draft;
        entity.RejectedByUserId = userId;
        entity.RejectedAt = now;
        entity.RejectionReason = dto.RejectionReason.Trim();
        entity.ApprovedByUserId = null;
        entity.ApprovedAt = null;
        entity.ModifiedDate = now;
        entity.ModifiedBy = _currentUserService.Email ?? userId.ToString();

        _unitOfWork.JobSafetyAssessments.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "Reject",
            nameof(JobSafetyAssessment),
            entity.Id,
            new { Status = oldStatus },
            new { Status = entity.Status, entity.RejectionReason },
            cancellationToken);

        if (entity.CreatedByUserId.HasValue)
        {
            await _notifications.NotifyUsersAsync(
                new[] { entity.CreatedByUserId.Value },
                "Assessment Rejected",
                $"Your assessment \"{entity.Title}\" was rejected: {entity.RejectionReason}",
                NotificationPriority.High,
                WorkflowNotificationType.AssessmentRejected,
                relatedEntityType: RelatedEntityTypes.JobSafetyAssessment,
                relatedEntityId: entity.Id,
                cancellationToken);
        }

        return await _assessmentService.GetByIdAsync(id, cancellationToken);
    }

    public async Task NotifySubmittedAsync(Guid assessmentId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.JobSafetyAssessments.GetByIdAsync(assessmentId, cancellationToken);
        if (entity == null) return;

        var title = "Assessment Submitted";
        var message = $"Assessment \"{entity.Title}\" submitted by {entity.SignOffName} {entity.SignOffSurname} awaits review.";

        await _notifications.NotifyRolesAsync(
            new[] { AppRoles.Supervisor, AppRoles.SafetyManager },
            title,
            message,
            NotificationPriority.High,
            WorkflowNotificationType.AssessmentSubmitted,
            relatedEntityType: RelatedEntityTypes.JobSafetyAssessment,
            relatedEntityId: entity.Id,
            cancellationToken);
    }

    private void EnsureSupervisor()
    {
        if (!_currentUserService.Roles.Any(r =>
                r is AppRoles.Administrator or AppRoles.HseManager or AppRoles.Supervisor))
        {
            throw new UnauthorizedAppException("Supervisor access required.");
        }
    }
}
