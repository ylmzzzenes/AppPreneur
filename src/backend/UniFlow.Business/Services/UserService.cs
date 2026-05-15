using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Users;
using UniFlow.Business.Helpers;
using UniFlow.DataAccess.UnitOfWork;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class UserService(IUnitOfWork unitOfWork) : IUserService
{
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

        if (request.PersonalityVibe.HasValue)
        {
            user.PersonalityVibe = request.PersonalityVibe.Value;
        }

        if (request.Major is not null)
        {
            user.Major = UserProfileNormalizer.NormalizeMajor(request.Major);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<UserProfileResponse>.Success(ToProfile(user));
    }

    private static UserProfileResponse ToProfile(User user) => new()
    {
        UserId = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
        PersonalityVibe = user.PersonalityVibe,
        Major = user.Major,
    };
}
