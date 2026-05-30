using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Users;
using UniFlow.Business.Helpers;
using UniFlow.DataAccess.UnitOfWork;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class UserService(IUnitOfWork unitOfWork) : IUserService
{
    public async Task<Result<UserProfileResponse>> GetProfileAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserProfileResponse>.Fail("USER_NOT_FOUND", "User was not found.");
        }

        return Result<UserProfileResponse>.Success(ToProfile(user));
    }

    public async Task<Result<UserProfileResponse>> UpdateOnboardingAsync(
        long userId,
        OnboardingUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Repository<User>().GetByIdForUpdateAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserProfileResponse>.Fail("USER_NOT_FOUND", "User was not found.");
        }

        if (request.DisplayName is not null)
        {
            user.DisplayName = UserProfileNormalizer.NormalizeDisplayName(request.DisplayName)
                ?? user.DisplayName;
        }

        if (request.Major is not null)
        {
            user.Major = UserProfileNormalizer.NormalizeMajor(request.Major);
        }

        if (request.AcademicGoal is not null)
        {
            user.AcademicGoal = UserProfileNormalizer.NormalizeAcademicGoal(request.AcademicGoal);
        }

        if (request.PersonalityVibe.HasValue)
        {
            user.PersonalityVibe = request.PersonalityVibe.Value;
        }

        if (request.DailyStudyTargetMinutes.HasValue)
        {
            user.DailyStudyTargetMinutes = request.DailyStudyTargetMinutes.Value;
        }

        user.IsOnboardingCompleted = true;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<UserProfileResponse>.Success(ToProfile(user));
    }

    private static UserProfileResponse ToProfile(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
        Major = user.Major,
        AcademicGoal = user.AcademicGoal,
        PersonalityVibe = user.PersonalityVibe,
        DailyStudyTargetMinutes = user.DailyStudyTargetMinutes,
        IsOnboardingCompleted = user.IsOnboardingCompleted,
        CreatedAt = user.CreatedDate,
        UpdatedAt = user.UpdatedDate,
    };
}
