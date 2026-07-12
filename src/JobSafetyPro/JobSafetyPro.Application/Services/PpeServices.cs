using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Ppe;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Ppe;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public interface IPpeCatalogueService
{
    Task<IReadOnlyList<PpeCatalogueItemDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<PpeCatalogueItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PpeCatalogueItemDto> CreateAsync(CreatePpeCatalogueItemDto dto, CancellationToken cancellationToken = default);
    Task<PpeCatalogueItemDto> UpdateAsync(Guid id, UpdatePpeCatalogueItemDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IPpeRequestService
{
    Task<IReadOnlyList<PpeRequestListDto>> GetAllAsync(PpeRequestFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> CreateAsync(CreatePpeRequestDto dto, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> UpdateAsync(Guid id, UpdatePpeRequestDto dto, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> SubmitForApprovalAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> RejectAsync(Guid id, RejectPpeRequestDto dto, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> StartPreparingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> DispatchAsync(Guid id, DispatchPpeRequestDto dto, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> CollectAsync(Guid id, CollectPpeRequestDto dto, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PpeRequestDetailDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeePpeHistoryDto>> GetEmployeeHistoryAsync(Guid employeeUserId, CancellationToken cancellationToken = default);
    Task<MyPpeSummaryDto> GetMyPpeAsync(CancellationToken cancellationToken = default);
}

public interface IPpeDashboardService
{
    Task<PpeDashboardKpiDto> GetDashboardKpisAsync(CancellationToken cancellationToken = default);
}

public interface IPpeReportService
{
    Task<IReadOnlyList<PpeRequestListDto>> GetOutstandingRequestsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PpeRequestListDto>> GetIssuedRegisterAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PpeReportRowDto>> GetByDepartmentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PpeReportRowDto>> GetByEmployeeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PpeReportRowDto>> GetByItemAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PpeRequestListDto>> GetOverdueAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PpeReportRowDto>> GetUsageTrendsAsync(CancellationToken cancellationToken = default);
}

public interface IPpeEscalationService
{
    Task ProcessEscalationsAsync(CancellationToken cancellationToken = default);
    Task CheckLowStockAsync(CancellationToken cancellationToken = default);
}

public class PpeCatalogueService : IPpeCatalogueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;
    private readonly IAuditService _audit;
    private readonly ISafetyNotificationDispatcher _notifications;

    public PpeCatalogueService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IDateTimeService dateTime,
        IAuditService audit,
        ISafetyNotificationDispatcher notifications)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<PpeCatalogueItemDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.PpeCatalogueItems.GetAllAsync(cancellationToken);
        return items
            .Where(i => !activeOnly || i.IsActive)
            .OrderBy(i => i.ItemName)
            .Select(MapCatalogue)
            .ToList();
    }

    public async Task<PpeCatalogueItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await GetCatalogueEntity(id, cancellationToken);
        return MapCatalogue(item);
    }

    public async Task<PpeCatalogueItemDto> CreateAsync(CreatePpeCatalogueItemDto dto, CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();
        var now = _dateTime.UtcNow;
        var entity = new PpeCatalogueItem
        {
            ItemName = dto.ItemName.Trim(),
            Category = dto.Category,
            QuantityInStock = dto.QuantityInStock,
            MinimumStockLevel = dto.MinimumStockLevel,
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedDate = now,
            CreatedBy = Actor(),
        };
        await _unitOfWork.PpeCatalogueItems.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Create", RelatedEntityTypes.PpeCatalogueItem, entity.Id, cancellationToken: cancellationToken);
        await NotifyLowStockIfNeeded(entity, cancellationToken);
        return MapCatalogue(entity);
    }

    public async Task<PpeCatalogueItemDto> UpdateAsync(Guid id, UpdatePpeCatalogueItemDto dto, CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();
        var entity = await GetCatalogueEntity(id, cancellationToken);
        entity.ItemName = dto.ItemName.Trim();
        entity.Category = dto.Category;
        entity.QuantityInStock = dto.QuantityInStock;
        entity.MinimumStockLevel = dto.MinimumStockLevel;
        entity.Description = dto.Description?.Trim();
        entity.IsActive = dto.IsActive;
        entity.ModifiedDate = _dateTime.UtcNow;
        entity.ModifiedBy = Actor();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await NotifyLowStockIfNeeded(entity, cancellationToken);
        return MapCatalogue(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();
        var entity = await GetCatalogueEntity(id, cancellationToken);
        entity.IsDeleted = true;
        entity.ModifiedDate = _dateTime.UtcNow;
        entity.ModifiedBy = Actor();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task NotifyLowStockIfNeeded(PpeCatalogueItem entity, CancellationToken cancellationToken)
    {
        if (entity.QuantityInStock > entity.MinimumStockLevel) return;
        await _notifications.NotifyRolesAsync(
            new[] { AppRoles.SafetyManager, AppRoles.SafetyOfficer, AppRoles.HseManager },
            "PPE Stock Running Low",
            $"{entity.ItemName} stock is low ({entity.QuantityInStock} remaining, minimum {entity.MinimumStockLevel}).",
            NotificationPriority.High,
            WorkflowNotificationType.PpeStockLow,
            RelatedEntityTypes.PpeCatalogueItem,
            entity.Id,
            cancellationToken);
    }

    private async Task<PpeCatalogueItem> GetCatalogueEntity(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PpeCatalogueItems.GetByIdAsync(id, cancellationToken);
        return entity ?? throw new NotFoundException(nameof(PpeCatalogueItem), id);
    }

    private void EnsureSafetyLead()
    {
        if (!_currentUser.Roles.Any(r => r is AppRoles.SafetyManager or AppRoles.SafetyOfficer or AppRoles.HseManager or AppRoles.Administrator))
            throw new ForbiddenAppException("Safety Officer or Safety Manager role required.");
    }

    private string Actor() => _currentUser.Email ?? _currentUser.UserId?.ToString() ?? "system";

    private static PpeCatalogueItemDto MapCatalogue(PpeCatalogueItem e) =>
        new(e.Id, e.ItemName, e.Category, e.QuantityInStock, e.MinimumStockLevel, e.Description, e.IsActive,
            e.QuantityInStock <= e.MinimumStockLevel);
}

public class PpeRequestService : IPpeRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;
    private readonly IAuditService _audit;
    private readonly ISafetyNotificationDispatcher _notifications;
    private readonly IFcmPushService _fcm;

    public PpeRequestService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IDateTimeService dateTime,
        IAuditService audit,
        ISafetyNotificationDispatcher notifications,
        IFcmPushService fcm)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _audit = audit;
        _notifications = notifications;
        _fcm = fcm;
    }

    public async Task<IReadOnlyList<PpeRequestListDto>> GetAllAsync(PpeRequestFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var requests = (await _unitOfWork.PpeRequests.GetAllAsync(cancellationToken)).ToList();
        var catalogue = (await _unitOfWork.PpeCatalogueItems.GetAllAsync(cancellationToken)).ToDictionary(c => c.Id);
        var today = _dateTime.UtcNow.Date;

        IEnumerable<PpeRequest> query = requests;

        if (filter?.EmployeeUserId is Guid empId)
            query = query.Where(r => r.EmployeeUserId == empId);
        if (!string.IsNullOrWhiteSpace(filter?.Department))
            query = query.Where(r => r.Department.Contains(filter.Department, StringComparison.OrdinalIgnoreCase));
        if (filter?.Status is PpeRequestStatus status)
            query = query.Where(r => r.Status == status);
        if (filter?.Priority is PpeRequestPriority priority)
            query = query.Where(r => r.Priority == priority);
        if (filter?.PpeCatalogueItemId is Guid itemId)
            query = query.Where(r => r.PpeCatalogueItemId == itemId);
        if (filter?.FromDate is DateTime from)
            query = query.Where(r => r.RequestedDate >= from);
        if (filter?.ToDate is DateTime to)
            query = query.Where(r => r.RequestedDate <= to);
        if (filter?.ArchivedOnly == true)
            query = query.Where(r => r.Status == PpeRequestStatus.Archived);
        else if (filter?.ArchivedOnly == false)
            query = query.Where(r => r.Status != PpeRequestStatus.Archived);
        if (filter?.IssuedOnly == true)
            query = query.Where(r => r.Status is PpeRequestStatus.Dispatched or PpeRequestStatus.Collected or PpeRequestStatus.Completed);

        return query
            .OrderByDescending(r => r.RequestedDate)
            .Select(r => MapList(r, catalogue.GetValueOrDefault(r.PpeCatalogueItemId), today))
            .ToList();
    }

    public async Task<PpeRequestDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetRequestEntity(id, cancellationToken);
        return await MapDetail(entity, cancellationToken);
    }

    public async Task<PpeRequestDetailDto> CreateAsync(CreatePpeRequestDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanCreate();
        var userId = _currentUser.UserId ?? throw new UnauthorizedAppException("User is not authenticated.");
        var companyId = _currentUser.CompanyId ?? throw new ValidationAppException(new Dictionary<string, string[]>
        {
            ["company"] = new[] { "Company context is required." },
        });

        var employee = await _unitOfWork.Users.GetByIdAsync(dto.EmployeeUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), dto.EmployeeUserId);
        var catalogue = await _unitOfWork.PpeCatalogueItems.GetByIdAsync(dto.PpeCatalogueItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(PpeCatalogueItem), dto.PpeCatalogueItemId);

        var now = _dateTime.UtcNow;
        var entity = new PpeRequest
        {
            RequestNumber = await GenerateRequestNumberAsync(cancellationToken),
            CompanyId = companyId,
            EmployeeUserId = dto.EmployeeUserId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            WorkDepartmentId = dto.WorkDepartmentId,
            Department = dto.Department.Trim(),
            Location = dto.Location.Trim(),
            Section = dto.Section.Trim(),
            PpeCatalogueItemId = dto.PpeCatalogueItemId,
            Quantity = dto.Quantity,
            Reason = dto.Reason.Trim(),
            Priority = dto.Priority,
            RequestedDate = now,
            RequiredByDate = dto.RequiredByDate.Date,
            Comments = dto.Comments?.Trim(),
            Status = PpeRequestStatus.Requested,
            RequestedByUserId = userId,
            CreatedDate = now,
            CreatedBy = Actor(),
        };

        await _unitOfWork.PpeRequests.AddAsync(entity, cancellationToken);
        await AddStatusHistory(entity, null, entity.Status, "Create", null, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await NotifyEmployee(entity, "PPE Requested", $"Your PPE request for {catalogue.ItemName} has been submitted.", WorkflowNotificationType.PpeRequested, cancellationToken);
        await _notifications.NotifyRolesAsync(
            new[] { AppRoles.SafetyManager },
            "New PPE Request",
            $"{entity.EmployeeName} requested {catalogue.ItemName} (x{entity.Quantity}).",
            MapPriority(entity.Priority),
            WorkflowNotificationType.PpeRequested,
            RelatedEntityTypes.PpeRequest,
            entity.Id,
            cancellationToken);

        return await MapDetail(entity, cancellationToken);
    }

    public async Task<PpeRequestDetailDto> UpdateAsync(Guid id, UpdatePpeRequestDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanCreate();
        var entity = await GetRequestEntity(id, cancellationToken);
        EnsureEditable(entity);

        entity.WorkDepartmentId = dto.WorkDepartmentId;
        entity.Department = dto.Department.Trim();
        entity.Location = dto.Location.Trim();
        entity.Section = dto.Section.Trim();
        entity.PpeCatalogueItemId = dto.PpeCatalogueItemId;
        entity.Quantity = dto.Quantity;
        entity.Reason = dto.Reason.Trim();
        entity.Priority = dto.Priority;
        entity.RequiredByDate = dto.RequiredByDate.Date;
        entity.Comments = dto.Comments?.Trim();
        entity.ModifiedDate = _dateTime.UtcNow;
        entity.ModifiedBy = Actor();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapDetail(entity, cancellationToken);
    }

    public Task<PpeRequestDetailDto> SubmitForApprovalAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, PpeRequestStatus.PendingApproval, "SubmitForApproval",
            new[] { PpeRequestStatus.Requested }, null, WorkflowNotificationType.PpeRequested, cancellationToken);

    public async Task<PpeRequestDetailDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureSafetyManager();
        var entity = await TransitionAsync(id, PpeRequestStatus.Approved, "Approve",
            new[] { PpeRequestStatus.Requested, PpeRequestStatus.PendingApproval }, null,
            WorkflowNotificationType.PpeApproved, cancellationToken);
        return await StartPreparingAsync(id, cancellationToken);
    }

    public Task<PpeRequestDetailDto> RejectAsync(Guid id, RejectPpeRequestDto dto, CancellationToken cancellationToken = default)
    {
        EnsureSafetyManager();
        return TransitionAsync(id, PpeRequestStatus.Rejected, "Reject",
            new[] { PpeRequestStatus.Requested, PpeRequestStatus.PendingApproval }, dto.Reason,
            WorkflowNotificationType.PpeRejected, cancellationToken);
    }

    public Task<PpeRequestDetailDto> StartPreparingAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, PpeRequestStatus.Preparing, "StartPreparing",
            new[] { PpeRequestStatus.Approved, PpeRequestStatus.Requested, PpeRequestStatus.PendingApproval }, null,
            WorkflowNotificationType.PpePreparing, cancellationToken);

    public async Task<PpeRequestDetailDto> DispatchAsync(Guid id, DispatchPpeRequestDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanCreate();
        var entity = await GetRequestEntity(id, cancellationToken);
        if (entity.Status is not (PpeRequestStatus.Preparing or PpeRequestStatus.Approved))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Only preparing requests can be dispatched." },
            });
        }

        var userId = _currentUser.UserId!.Value;
        var catalogue = await _unitOfWork.PpeCatalogueItems.GetByIdAsync(entity.PpeCatalogueItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(PpeCatalogueItem), entity.PpeCatalogueItemId);
        if (catalogue.QuantityInStock < entity.Quantity)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["stock"] = new[] { $"Insufficient stock. Available: {catalogue.QuantityInStock}." },
            });
        }

        catalogue.QuantityInStock -= entity.Quantity;
        entity.DispatchDate = dto.DispatchDate;
        entity.IssuedByUserId = userId;
        entity.CollectedByEmployee = dto.CollectedByEmployee.Trim();
        entity.EmployeeSignature = dto.EmployeeSignature?.Trim();
        entity.SafetyOfficerSignature = dto.SafetyOfficerSignature?.Trim();
        entity.DispatchNotes = dto.Notes?.Trim();
        await TransitionCore(entity, PpeRequestStatus.Dispatched, "Dispatch", dto.Notes, cancellationToken);

        await NotifyEmployee(entity, "PPE Dispatched", $"Your {catalogue.ItemName} is ready for collection.", WorkflowNotificationType.PpeDispatched, cancellationToken);
        await NotifyEmployee(entity, "PPE Ready For Collection", $"Please collect your {catalogue.ItemName} at the safety office.", WorkflowNotificationType.PpeReadyForCollection, cancellationToken);

        return await MapDetail(entity, cancellationToken);
    }

    public async Task<PpeRequestDetailDto> CollectAsync(Guid id, CollectPpeRequestDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetRequestEntity(id, cancellationToken);
        if (entity.Status != PpeRequestStatus.Dispatched)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Only dispatched requests can be collected." },
            });
        }

        entity.CollectedDate = dto.CollectedDate;
        await TransitionCore(entity, PpeRequestStatus.Collected, "Collect", null, cancellationToken);
        await NotifyEmployee(entity, "PPE Collected", "Your PPE collection has been recorded.", WorkflowNotificationType.PpeCollected, cancellationToken);
        return await MapDetail(entity, cancellationToken);
    }

    public Task<PpeRequestDetailDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, PpeRequestStatus.Completed, "Complete",
            new[] { PpeRequestStatus.Collected, PpeRequestStatus.Dispatched }, null, WorkflowNotificationType.PpeCollected, cancellationToken);

    public Task<PpeRequestDetailDto> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureCanCreate();
        return TransitionAsync(id, PpeRequestStatus.Archived, "Archive",
            new[] { PpeRequestStatus.Completed, PpeRequestStatus.Rejected, PpeRequestStatus.Cancelled }, null,
            WorkflowNotificationType.General, cancellationToken, setArchivedDate: true);
    }

    public Task<PpeRequestDetailDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureCanCreate();
        return TransitionAsync(id, PpeRequestStatus.Cancelled, "Cancel",
            new[] { PpeRequestStatus.Requested, PpeRequestStatus.PendingApproval, PpeRequestStatus.Approved, PpeRequestStatus.Preparing },
            null, WorkflowNotificationType.General, cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeePpeHistoryDto>> GetEmployeeHistoryAsync(Guid employeeUserId, CancellationToken cancellationToken = default)
    {
        var requests = (await _unitOfWork.PpeRequests.GetAllAsync(cancellationToken))
            .Where(r => r.EmployeeUserId == employeeUserId)
            .OrderByDescending(r => r.RequestedDate)
            .ToList();
        var catalogue = (await _unitOfWork.PpeCatalogueItems.GetAllAsync(cancellationToken)).ToDictionary(c => c.Id);
        var result = new List<EmployeePpeHistoryDto>();
        foreach (var r in requests)
        {
            var history = await GetHistoryForRequest(r.Id, cancellationToken);
            result.Add(new EmployeePpeHistoryDto(
                r.Id, r.RequestNumber,
                catalogue.GetValueOrDefault(r.PpeCatalogueItemId)?.ItemName ?? "PPE Item",
                r.RequestedDate, r.Status, r.DispatchDate, r.CollectedDate, history));
        }
        return result;
    }

    public async Task<MyPpeSummaryDto> GetMyPpeAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAppException("User is not authenticated.");
        var filter = new PpeRequestFilterDto(EmployeeUserId: userId);
        var current = (await GetAllAsync(filter, cancellationToken))
            .Where(r => r.Status is not (PpeRequestStatus.Archived or PpeRequestStatus.Completed or PpeRequestStatus.Cancelled or PpeRequestStatus.Rejected))
            .ToList();
        var history = await GetEmployeeHistoryAsync(userId, cancellationToken);
        return new MyPpeSummaryDto(current, history);
    }

    private async Task<PpeRequestDetailDto> TransitionAsync(
        Guid id,
        PpeRequestStatus newStatus,
        string action,
        PpeRequestStatus[] allowedFrom,
        string? comments,
        WorkflowNotificationType notificationType,
        CancellationToken cancellationToken,
        bool setArchivedDate = false)
    {
        var entity = await GetRequestEntity(id, cancellationToken);
        if (!allowedFrom.Contains(entity.Status))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { $"Cannot {action} from status {entity.Status}." },
            });
        }

        if (newStatus == PpeRequestStatus.Rejected && !string.IsNullOrWhiteSpace(comments))
            entity.RejectionReason = comments.Trim();
        if (newStatus == PpeRequestStatus.Approved)
        {
            entity.ApprovedByUserId = _currentUser.UserId;
            entity.ApprovedAt = _dateTime.UtcNow;
        }
        if (setArchivedDate)
            entity.ArchivedDate = _dateTime.UtcNow;
        if (newStatus == PpeRequestStatus.Completed)
            entity.CompletedDate = _dateTime.UtcNow;

        await TransitionCore(entity, newStatus, action, comments, cancellationToken);

        if (notificationType != WorkflowNotificationType.General)
        {
            var title = action switch
            {
                "Approve" => "PPE Approved",
                "Reject" => "PPE Rejected",
                "StartPreparing" => "PPE Preparing",
                _ => $"PPE {newStatus}",
            };
            await NotifyEmployee(entity, title, $"Your PPE request {entity.RequestNumber} is now {newStatus}.", notificationType, cancellationToken);
        }

        return await MapDetail(entity, cancellationToken);
    }

    private async Task TransitionCore(PpeRequest entity, PpeRequestStatus newStatus, string action, string? comments, CancellationToken cancellationToken)
    {
        var old = entity.Status;
        entity.Status = newStatus;
        entity.ModifiedDate = _dateTime.UtcNow;
        entity.ModifiedBy = Actor();
        await AddStatusHistory(entity, old, newStatus, action, comments, cancellationToken);
        await _audit.LogAsync(action, RelatedEntityTypes.PpeRequest, entity.Id,
            new { Status = old.ToString() }, new { Status = newStatus.ToString() }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task AddStatusHistory(PpeRequest entity, PpeRequestStatus? oldStatus, PpeRequestStatus newStatus, string action, string? comments, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var history = new PpeRequestStatusHistory
        {
            PpeRequestId = entity.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Action = action,
            ActionByUserId = userId,
            ActionByUserName = Actor(),
            ActionDate = _dateTime.UtcNow,
            Comments = comments,
            CreatedDate = _dateTime.UtcNow,
            CreatedBy = Actor(),
        };
        await _unitOfWork.PpeRequestStatusHistories.AddAsync(history, cancellationToken);
    }

    private async Task NotifyEmployee(PpeRequest entity, string title, string message, WorkflowNotificationType type, CancellationToken cancellationToken)
    {
        await _notifications.NotifyUsersAsync(
            new[] { entity.EmployeeUserId },
            title,
            message,
            MapPriority(entity.Priority),
            type,
            RelatedEntityTypes.PpeRequest,
            entity.Id,
            cancellationToken);
        await _fcm.SendToUserAsync(entity.EmployeeUserId, title, message, cancellationToken);
    }

    private async Task<string> GenerateRequestNumberAsync(CancellationToken cancellationToken)
    {
        var today = _dateTime.UtcNow.Date;
        var count = (await _unitOfWork.PpeRequests.GetAllAsync(cancellationToken))
            .Count(r => r.RequestedDate.Date == today);
        return $"PPE-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    private async Task<PpeRequest> GetRequestEntity(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PpeRequests.GetByIdAsync(id, cancellationToken);
        return entity ?? throw new NotFoundException(nameof(PpeRequest), id);
    }

    private async Task<PpeRequestDetailDto> MapDetail(PpeRequest r, CancellationToken cancellationToken)
    {
        var catalogue = await _unitOfWork.PpeCatalogueItems.GetByIdAsync(r.PpeCatalogueItemId, cancellationToken);
        var history = await GetHistoryForRequest(r.Id, cancellationToken);
        var today = _dateTime.UtcNow.Date;
        return new PpeRequestDetailDto(
            r.Id, r.RequestNumber, r.EmployeeUserId, r.EmployeeName, r.WorkDepartmentId, r.Department,
            r.Location, r.Section, r.PpeCatalogueItemId, catalogue?.ItemName ?? "PPE Item", r.Quantity,
            r.Reason, r.Priority, r.RequestedDate, r.RequiredByDate, r.Comments, r.Status,
            IsOverdue(r, today), r.DispatchDate, r.CollectedByEmployee, r.CollectedDate, r.CompletedDate,
            r.DispatchNotes, history);
    }

    private async Task<IReadOnlyList<PpeStatusHistoryDto>> GetHistoryForRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var items = (await _unitOfWork.PpeRequestStatusHistories.GetAllAsync(cancellationToken))
            .Where(h => h.PpeRequestId == requestId)
            .OrderBy(h => h.ActionDate)
            .Select(h => new PpeStatusHistoryDto(h.Id, h.OldStatus, h.NewStatus, h.Action, h.ActionByUserName, h.ActionDate, h.Comments))
            .ToList();
        return items;
    }

    private static PpeRequestListDto MapList(PpeRequest r, PpeCatalogueItem? catalogue, DateTime today) =>
        new(r.Id, r.RequestNumber, r.EmployeeName, r.Department, catalogue?.ItemName ?? "PPE Item", r.Quantity,
            r.Priority, r.Status, r.RequestedDate, r.RequiredByDate,
            Math.Max(0, (today - r.RequestedDate.Date).Days), IsOverdue(r, today));

    private static bool IsOverdue(PpeRequest r, DateTime today) =>
        r.Status is not (PpeRequestStatus.Completed or PpeRequestStatus.Archived or PpeRequestStatus.Cancelled or PpeRequestStatus.Rejected)
        && r.RequiredByDate.Date < today;

    private void EnsureCanCreate()
    {
        if (!IsSafetyLead()) throw new ForbiddenAppException("Safety Officer or Safety Manager role required.");
    }

    private void EnsureSafetyManager()
    {
        if (!IsSafetyManager()) throw new ForbiddenAppException("Safety Manager role required.");
    }

    private void EnsureEditable(PpeRequest entity)
    {
        if (entity.Status is not (PpeRequestStatus.Requested or PpeRequestStatus.PendingApproval))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Only open requests can be edited." },
            });
        }
    }

    private bool IsSafetyLead() =>
        _currentUser.Roles.Any(r => r is AppRoles.SafetyManager or AppRoles.SafetyOfficer or AppRoles.HseManager or AppRoles.Administrator);

    private bool IsSafetyManager() =>
        _currentUser.Roles.Any(r => r is AppRoles.SafetyManager or AppRoles.HseManager or AppRoles.Administrator);

    private string Actor() => _currentUser.Email ?? _currentUser.UserId?.ToString() ?? "system";

    private static NotificationPriority MapPriority(PpeRequestPriority priority) => priority switch
    {
        PpeRequestPriority.Urgent => NotificationPriority.Critical,
        PpeRequestPriority.High => NotificationPriority.High,
        _ => NotificationPriority.Normal,
    };
}

public class PpeDashboardService : IPpeDashboardService
{
    private readonly IPpeRequestService _requests;
    private readonly IPpeCatalogueService _catalogue;
    private readonly IDateTimeService _dateTime;

    public PpeDashboardService(IPpeRequestService requests, IPpeCatalogueService catalogue, IDateTimeService dateTime)
    {
        _requests = requests;
        _catalogue = catalogue;
        _dateTime = dateTime;
    }

    public async Task<PpeDashboardKpiDto> GetDashboardKpisAsync(CancellationToken cancellationToken = default)
    {
        var all = await _requests.GetAllAsync(cancellationToken: cancellationToken);
        var today = _dateTime.UtcNow.Date;
        var catalogue = await _catalogue.GetAllAsync(cancellationToken: cancellationToken);
        return new PpeDashboardKpiDto(
            all.Count(r => r.Status is not (PpeRequestStatus.Completed or PpeRequestStatus.Archived or PpeRequestStatus.Cancelled or PpeRequestStatus.Rejected)),
            all.Count(r => r.Status == PpeRequestStatus.PendingApproval),
            all.Count(r => r.Status == PpeRequestStatus.Preparing),
            all.Count(r => r.Status == PpeRequestStatus.Dispatched),
            all.Count(r => r.Status == PpeRequestStatus.Collected && r.RequestedDate.Date == today),
            all.Count(r => r.IsOverdue),
            catalogue.Count(c => c.IsLowStock));
    }
}

public class PpeReportService : IPpeReportService
{
    private readonly IPpeRequestService _requests;

    public PpeReportService(IPpeRequestService requests) => _requests = requests;

    public async Task<IReadOnlyList<PpeRequestListDto>> GetOutstandingRequestsAsync(CancellationToken cancellationToken = default)
    {
        var all = await _requests.GetAllAsync(cancellationToken: cancellationToken);
        return all.Where(r => r.Status is not (PpeRequestStatus.Completed or PpeRequestStatus.Archived or PpeRequestStatus.Cancelled or PpeRequestStatus.Rejected)).ToList();
    }

    public async Task<IReadOnlyList<PpeRequestListDto>> GetIssuedRegisterAsync(CancellationToken cancellationToken = default) =>
        await _requests.GetAllAsync(new PpeRequestFilterDto(IssuedOnly: true), cancellationToken);

    public async Task<IReadOnlyList<PpeReportRowDto>> GetByDepartmentAsync(CancellationToken cancellationToken = default)
    {
        var all = await _requests.GetAllAsync(cancellationToken: cancellationToken);
        return all.GroupBy(r => r.Department).Select(g => new PpeReportRowDto(g.Key, g.Count())).OrderByDescending(r => r.Count).ToList();
    }

    public async Task<IReadOnlyList<PpeReportRowDto>> GetByEmployeeAsync(CancellationToken cancellationToken = default)
    {
        var all = await _requests.GetAllAsync(cancellationToken: cancellationToken);
        return all.GroupBy(r => r.EmployeeName).Select(g => new PpeReportRowDto(g.Key, g.Count())).OrderByDescending(r => r.Count).ToList();
    }

    public async Task<IReadOnlyList<PpeReportRowDto>> GetByItemAsync(CancellationToken cancellationToken = default)
    {
        var all = await _requests.GetAllAsync(cancellationToken: cancellationToken);
        return all.GroupBy(r => r.PpeItemName).Select(g => new PpeReportRowDto(g.Key, g.Count())).OrderByDescending(r => r.Count).ToList();
    }

    public async Task<IReadOnlyList<PpeRequestListDto>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var all = await _requests.GetAllAsync(cancellationToken: cancellationToken);
        return all.Where(r => r.IsOverdue).ToList();
    }

    public async Task<IReadOnlyList<PpeReportRowDto>> GetUsageTrendsAsync(CancellationToken cancellationToken = default)
    {
        var all = await _requests.GetAllAsync(cancellationToken: cancellationToken);
        return all
            .GroupBy(r => r.RequestedDate.ToString("yyyy-MM"))
            .Select(g => new PpeReportRowDto(g.Key, g.Count()))
            .OrderBy(r => r.Label)
            .ToList();
    }
}

public class PpeEscalationService : IPpeEscalationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTime;
    private readonly ISafetyNotificationDispatcher _notifications;
    private readonly IPpeCatalogueService _catalogue;

    public PpeEscalationService(
        IUnitOfWork unitOfWork,
        IDateTimeService dateTime,
        ISafetyNotificationDispatcher notifications,
        IPpeCatalogueService catalogue)
    {
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _notifications = notifications;
        _catalogue = catalogue;
    }

    public async Task ProcessEscalationsAsync(CancellationToken cancellationToken = default)
    {
        var today = _dateTime.UtcNow.Date;
        var requests = (await _unitOfWork.PpeRequests.GetAllAsync(cancellationToken))
            .Where(r => r.Status is not (PpeRequestStatus.Completed or PpeRequestStatus.Archived or PpeRequestStatus.Cancelled or PpeRequestStatus.Rejected))
            .ToList();

        foreach (var r in requests)
        {
            var age = (today - r.RequestedDate.Date).Days;
            if (age is not (7 or 14 or 30)) continue;

            var title = $"PPE Request Waiting {age} Days";
            var message = $"Request {r.RequestNumber} for {r.EmployeeName} has been open for {age} days.";
            await _notifications.NotifyRolesAsync(
                new[] { AppRoles.SafetyManager, AppRoles.SafetyOfficer },
                title,
                message,
                NotificationPriority.High,
                WorkflowNotificationType.PpeWaitingEscalation,
                RelatedEntityTypes.PpeRequest,
                r.Id,
                cancellationToken);
        }

        var overdue = requests.Where(r => r.RequiredByDate.Date < today).ToList();
        if (overdue.Count > 0)
        {
            await _notifications.NotifyRolesAsync(
                new[] { AppRoles.SafetyManager },
                "Overdue PPE Requests",
                $"{overdue.Count} PPE request(s) are past the required date.",
                NotificationPriority.High,
                WorkflowNotificationType.PpeOverdue,
                RelatedEntityTypes.PpeRequest,
                overdue[0].Id,
                cancellationToken);
        }
    }

    public async Task CheckLowStockAsync(CancellationToken cancellationToken = default)
    {
        var items = await _catalogue.GetAllAsync(activeOnly: true, cancellationToken: cancellationToken);
        foreach (var item in items.Where(i => i.IsLowStock))
        {
            await _notifications.NotifyRolesAsync(
                new[] { AppRoles.SafetyManager, AppRoles.SafetyOfficer },
                "PPE Stock Running Low",
                $"{item.ItemName} stock is low ({item.QuantityInStock} remaining).",
                NotificationPriority.High,
                WorkflowNotificationType.PpeStockLow,
                RelatedEntityTypes.PpeCatalogueItem,
                item.Id,
                cancellationToken);
        }
    }
}
