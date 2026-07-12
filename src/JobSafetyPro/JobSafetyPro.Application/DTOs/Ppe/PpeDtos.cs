using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Application.DTOs.Ppe;

public record PpeCatalogueItemDto(
    Guid Id,
    string ItemName,
    PpeCategory Category,
    int QuantityInStock,
    int MinimumStockLevel,
    string? Description,
    bool IsActive,
    bool IsLowStock);

public record CreatePpeCatalogueItemDto(
    string ItemName,
    PpeCategory Category,
    int QuantityInStock,
    int MinimumStockLevel,
    string? Description,
    bool IsActive = true);

public record UpdatePpeCatalogueItemDto(
    string ItemName,
    PpeCategory Category,
    int QuantityInStock,
    int MinimumStockLevel,
    string? Description,
    bool IsActive);

public record PpeRequestListDto(
    Guid Id,
    string RequestNumber,
    string EmployeeName,
    string Department,
    string PpeItemName,
    int Quantity,
    PpeRequestPriority Priority,
    PpeRequestStatus Status,
    DateTime RequestedDate,
    DateTime RequiredByDate,
    int AgeDays,
    bool IsOverdue);

public record PpeRequestDetailDto(
    Guid Id,
    string RequestNumber,
    Guid EmployeeUserId,
    string EmployeeName,
    Guid? WorkDepartmentId,
    string Department,
    string Location,
    string Section,
    Guid PpeCatalogueItemId,
    string PpeItemName,
    int Quantity,
    string Reason,
    PpeRequestPriority Priority,
    DateTime RequestedDate,
    DateTime RequiredByDate,
    string? Comments,
    PpeRequestStatus Status,
    bool IsOverdue,
    DateTime? DispatchDate,
    string? CollectedByEmployee,
    DateTime? CollectedDate,
    DateTime? CompletedDate,
    string? DispatchNotes,
    IReadOnlyList<PpeStatusHistoryDto> StatusHistory);

public record PpeStatusHistoryDto(
    Guid Id,
    PpeRequestStatus? OldStatus,
    PpeRequestStatus NewStatus,
    string Action,
    string ActionByUserName,
    DateTime ActionDate,
    string? Comments);

public record CreatePpeRequestDto(
    Guid EmployeeUserId,
    Guid? WorkDepartmentId,
    string Department,
    string Location,
    string Section,
    Guid PpeCatalogueItemId,
    int Quantity,
    string Reason,
    PpeRequestPriority Priority,
    DateTime RequiredByDate,
    string? Comments);

public record UpdatePpeRequestDto(
    Guid? WorkDepartmentId,
    string Department,
    string Location,
    string Section,
    Guid PpeCatalogueItemId,
    int Quantity,
    string Reason,
    PpeRequestPriority Priority,
    DateTime RequiredByDate,
    string? Comments);

public record RejectPpeRequestDto(string Reason);

public record DispatchPpeRequestDto(
    DateTime DispatchDate,
    string CollectedByEmployee,
    string? EmployeeSignature,
    string? SafetyOfficerSignature,
    string? Notes);

public record CollectPpeRequestDto(DateTime CollectedDate);

public record PpeRequestFilterDto(
    Guid? EmployeeUserId = null,
    string? Department = null,
    PpeRequestStatus? Status = null,
    PpeRequestPriority? Priority = null,
    Guid? PpeCatalogueItemId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    bool? ArchivedOnly = null,
    bool? IssuedOnly = null);

public record PpeDashboardKpiDto(
    int OpenRequests,
    int PendingApproval,
    int Preparing,
    int ReadyForCollection,
    int CollectedToday,
    int OverdueRequests,
    int LowStockItems);

public record PpeReportRowDto(string Label, int Count, string? Meta = null);

public record EmployeePpeHistoryDto(
    Guid RequestId,
    string RequestNumber,
    string PpeItemName,
    DateTime RequestedDate,
    PpeRequestStatus Status,
    DateTime? DispatchDate,
    DateTime? CollectedDate,
    IReadOnlyList<PpeStatusHistoryDto> Timeline);

public record MyPpeSummaryDto(
    IReadOnlyList<PpeRequestListDto> CurrentRequests,
    IReadOnlyList<EmployeePpeHistoryDto> History);
