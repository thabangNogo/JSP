using JobSafetyPro.Application.DTOs.Auth;

namespace JobSafetyPro.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<UserProfileDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
