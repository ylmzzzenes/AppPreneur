using UniFlow.Business.Contracts.Users;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface IUserService
{
    Task<Result<UserProfileResponse>> UpdateOnboardingAsync(
        long userId,
        OnboardingUpdateRequest request,
        CancellationToken cancellationToken = default);
}
