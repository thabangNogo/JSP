namespace JobSafetyPro.Application.DTOs.Auth;

public record LoginRequestDto(string Email, string Password);

public record RefreshTokenRequestDto(string RefreshToken);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserProfileDto User);

public record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    Guid CompanyId,
    Guid? PlantId,
    Guid? DepartmentId,
    IReadOnlyList<string> Roles);
