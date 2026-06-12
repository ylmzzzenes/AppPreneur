namespace UniFlow.Mobile.Services;

/// <summary>
/// Clears credentials and navigates to login when the API reports an expired or invalid token.
/// </summary>
internal static class AuthSessionNavigator
{
    public static async Task<bool> HandleUnauthorizedIfNeededAsync(
        string? errorCode,
        IAuthTokenStore tokenStore,
        IUserSessionInfo? userSession = null,
        CancellationToken cancellationToken = default)
    {
        if (errorCode != "UNAUTHORIZED")
        {
            return false;
        }

        await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
        userSession?.Clear();
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync($"//{Routes.Login}")).ConfigureAwait(false);
        return true;
    }
}
