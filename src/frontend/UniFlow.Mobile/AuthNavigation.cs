using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile;

internal static class AuthNavigation
{
    public static async Task NavigateAfterAuthenticationAsync(
        IApiClient apiClient,
        IAuthTokenStore tokenStore,
        IUserSessionInfo userSession,
        CancellationToken cancellationToken = default)
    {
        var profileResult = await apiClient.GetMyProfileAsync(cancellationToken).ConfigureAwait(false);

        if (profileResult.Error?.Code == "UNAUTHORIZED")
        {
            await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            userSession.Clear();
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.GoToAsync($"//{Routes.Login}")).ConfigureAwait(false);
            return;
        }

        if (profileResult.IsSuccess && profileResult.Data is not null)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                    userSession.SetDisplayName(profileResult.Data.DisplayName))
                .ConfigureAwait(false);

            var route = profileResult.Data.IsOnboardingCompleted
                ? $"//{Routes.MainTabs}/{Routes.Dashboard}"
                : Routes.Onboarding;

            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.GoToAsync(route)).ConfigureAwait(false);
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync(Routes.Onboarding)).ConfigureAwait(false);
    }
}
