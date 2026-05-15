using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Auth;
using UniFlow.Business.Helpers;
using UniFlow.DataAccess.Queries;
using UniFlow.DataAccess.UnitOfWork;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Enums;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class AuthService(
    IUserQueries userQueries,
    IUnitOfWork unitOfWork,
    IJwtTokenIssuer jwtTokenIssuer) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await userQueries.EmailExistsAsync(email, cancellationToken).ConfigureAwait(false))
        {
            return Result<AuthResponse>.Fail("AUTH_DUPLICATE_EMAIL", "This email is already registered.");
        }

        var user = new User
        {
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            PersonalityVibe = request.PersonalityVibe ?? PersonalityVibe.Friendly,
            Major = UserProfileNormalizer.NormalizeMajor(request.Major),
        };

        unitOfWork.Repository<User>().Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var (token, expires) = jwtTokenIssuer.CreateAccessToken(user);
        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userQueries.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Fail("AUTH_INVALID", "Invalid email or password.");
        }

        var (token, expires) = jwtTokenIssuer.CreateAccessToken(user);
        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
        });
    }
}
