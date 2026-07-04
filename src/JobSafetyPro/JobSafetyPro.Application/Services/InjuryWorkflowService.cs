using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Workflow;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public interface IInjuryWorkflowService
{
    Task<InjuryDetailDto> StartInvestigationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InjuryDetailDto> RecordInvestigationAsync(Guid id, InjuryInvestigationDto dto, CancellationToken cancellationToken = default);
    Task<InjuryDetailDto> SetMedicalTreatmentAsync(Guid id, InjuryMedicalOutcomeDto dto, CancellationToken cancellationToken = default);
    Task<InjuryDetailDto> SetReturnToWorkAsync(Guid id, InjuryReturnToWorkDto dto, CancellationToken cancellationToken = default);
    Task<InjuryDetailDto> CloseAsync(Guid id, CloseInjuryDto dto, CancellationToken cancellationToken = default);
}

public class InjuryWorkflowService : IInjuryWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IAuditService _auditService;
    private readonly IInjuryService _injuryService;

    public InjuryWorkflowService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IAuditService auditService,
        IInjuryService injuryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _auditService = auditService;
        _injuryService = injuryService;
    }

    public async Task<InjuryDetailDto> StartInvestigationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureSafetyOfficer();
        var injury = await GetInjury(id, cancellationToken);
        var old = injury.Status;
        injury.Status = InjuryStatus.UnderInvestigation;
        injury.InvestigatedByUserId = _currentUserService.UserId;
        injury.InvestigatedAt = _dateTimeService.UtcNow;
        await SaveWithAudit(injury, "StartInvestigation", old, injury.Status, cancellationToken);
        return await _injuryService.GetByIdAsync(id, cancellationToken);
    }

    public async Task<InjuryDetailDto> RecordInvestigationAsync(
        Guid id,
        InjuryInvestigationDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyOfficer();
        var injury = await GetInjury(id, cancellationToken);
        injury.RootCause = dto.RootCause.Trim();
        injury.CorrectiveAction = dto.CorrectiveAction.Trim();
        injury.Status = InjuryStatus.UnderInvestigation;
        await SaveWithAudit(injury, "RecordInvestigation", null, dto, cancellationToken);
        return await _injuryService.GetByIdAsync(id, cancellationToken);
    }

    public async Task<InjuryDetailDto> SetMedicalTreatmentAsync(
        Guid id,
        InjuryMedicalOutcomeDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyOfficer();
        var injury = await GetInjury(id, cancellationToken);
        injury.MedicalOutcome = dto.MedicalOutcome.Trim();
        injury.Status = InjuryStatus.MedicalTreatment;
        await SaveWithAudit(injury, "MedicalTreatment", null, dto, cancellationToken);
        return await _injuryService.GetByIdAsync(id, cancellationToken);
    }

    public async Task<InjuryDetailDto> SetReturnToWorkAsync(
        Guid id,
        InjuryReturnToWorkDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyOfficer();
        var injury = await GetInjury(id, cancellationToken);
        injury.ReturnToWorkDate = dto.ReturnToWorkDate;
        injury.Status = InjuryStatus.ReturnToWork;
        await SaveWithAudit(injury, "ReturnToWork", null, dto, cancellationToken);
        return await _injuryService.GetByIdAsync(id, cancellationToken);
    }

    public async Task<InjuryDetailDto> CloseAsync(
        Guid id,
        CloseInjuryDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyOfficer();
        var userId = _currentUserService.UserId!.Value;
        var injury = await GetInjury(id, cancellationToken);
        var old = injury.Status;
        injury.Status = InjuryStatus.Closed;
        injury.ClosedAt = _dateTimeService.UtcNow;
        injury.ClosedByUserId = userId;
        await SaveWithAudit(injury, "Close", old, injury.Status, cancellationToken);
        return await _injuryService.GetByIdAsync(id, cancellationToken);
    }

    private async Task<Domain.Entities.Incidents.Injury> GetInjury(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Injuries.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Incidents.Injury), id);
    }

    private async Task SaveWithAudit(
        Domain.Entities.Incidents.Injury injury,
        string action,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken)
    {
        injury.ModifiedDate = _dateTimeService.UtcNow;
        injury.ModifiedBy = _currentUserService.Email ?? injury.ModifiedBy;
        _unitOfWork.Injuries.Update(injury);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(action, RelatedEntityTypes.Injury, injury.Id, oldValues, newValues, cancellationToken);
    }

    private void EnsureSafetyOfficer()
    {
        if (!_currentUserService.Roles.Any(r =>
                r is AppRoles.SafetyManager or AppRoles.SafetyOfficer or AppRoles.Administrator
                    or AppRoles.HseManager))
        {
            throw new UnauthorizedAppException("Safety officer access required.");
        }
    }
}
