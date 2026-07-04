using System.Security.Claims;
using JobSafetyPro.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace JobSafetyPro.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public Guid? CompanyId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("company_id"), out var id)
            ? id
            : null;

    public Guid? PlantId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("plant_id"), out var id)
            ? id
            : null;

    public IReadOnlyList<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? new List<string>();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
