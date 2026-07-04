using AutoMapper;
using FluentValidation;
using JobSafetyPro.Application.DTOs.Auth;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace JobSafetyPro.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IValidator<LoginRequestDto> loginValidator,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _loginValidator = loginValidator;
        _accessTokenMinutes = configuration.GetValue("Jwt:AccessTokenExpirationMinutes", 15);
        _refreshTokenDays = configuration.GetValue("Jwt:RefreshTokenExpirationDays", 7);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAppException("Invalid email or password.");

        if (!user.IsActive || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAppException("Invalid email or password.");

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var user = await _unitOfWork.Users.GetByRefreshTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedAppException("Invalid refresh token.");

        var storedToken = user.RefreshTokens.FirstOrDefault(t =>
            t.TokenHash == tokenHash &&
            t.RevokedAt == null &&
            t.ExpiresAt > _dateTimeService.UtcNow);

        if (storedToken == null)
            throw new UnauthorizedAppException("Invalid or expired refresh token.");

        var newRefreshPlain = _tokenService.GenerateRefreshToken();
        storedToken.RevokedAt = _dateTimeService.UtcNow;
        storedToken.ReplacedByTokenHash = _tokenService.HashToken(newRefreshPlain);
        _unitOfWork.RefreshTokens.Update(storedToken);

        return await IssueTokensAsync(user, cancellationToken, newRefreshPlain);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashToken(refreshToken);
        var user = await _unitOfWork.Users.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);
        if (user == null) return;

        var token = user.RefreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        if (token == null) return;

        token.RevokedAt = _dateTimeService.UtcNow;
        _unitOfWork.RefreshTokens.Update(token);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == null)
            throw new UnauthorizedAppException("User is not authenticated.");

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(_currentUserService.UserId.Value, cancellationToken)
            ?? throw new NotFoundException(nameof(User), _currentUserService.UserId.Value);

        return MapUserProfile(user);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(
        User user,
        CancellationToken cancellationToken,
        string? existingRefreshPlain = null)
    {
        user = await _unitOfWork.Users.GetByIdWithRolesAsync(user.Id, cancellationToken) ?? user;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Email, user.CompanyId, user.PlantId, roles);

        var refreshTokenPlain = existingRefreshPlain ?? _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(refreshTokenPlain),
            ExpiresAt = _dateTimeService.UtcNow.AddDays(_refreshTokenDays)
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Login", nameof(User), user.Id, cancellationToken: cancellationToken);

        return new AuthResponseDto(
            accessToken,
            refreshTokenPlain,
            _dateTimeService.UtcNow.AddMinutes(_accessTokenMinutes),
            MapUserProfile(user));
    }

    private static UserProfileDto MapUserProfile(User user) =>
        new(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CompanyId,
            user.PlantId,
            user.DepartmentId,
            user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList());
}
