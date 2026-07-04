using System.Text.Json;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Shared;
using JobSafetyPro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace JobSafetyPro.Infrastructure.Services;

public class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions AuditJsonOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        Guid entityId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = _currentUserService.UserId,
            CompanyId = _currentUserService.CompanyId ?? Guid.Empty,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues == null ? null : JsonSerializer.Serialize(oldValues, AuditJsonOptions),
            NewValues = newValues == null ? null : JsonSerializer.Serialize(newValues, AuditJsonOptions),
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            Timestamp = _dateTimeService.UtcNow,
            CreatedDate = _dateTimeService.UtcNow,
            CreatedBy = _currentUserService.Email ?? _currentUserService.UserId?.ToString() ?? "system"
        };

        await _unitOfWork.AuditLogs.AddAuditLogAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
