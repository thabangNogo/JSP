namespace JobSafetyPro.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Email { get; }

    Guid? CompanyId { get; }

    Guid? PlantId { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsAuthenticated { get; }
}

public interface IDateTimeService
{
    DateTime UtcNow { get; }
}

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, Guid companyId, Guid? plantId, IEnumerable<string> roles);

    string GenerateRefreshToken();

    string HashToken(string token);
}

public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityType,
        Guid entityId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}
