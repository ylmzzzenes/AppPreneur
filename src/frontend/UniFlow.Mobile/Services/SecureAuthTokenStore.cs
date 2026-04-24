namespace UniFlow.Mobile.Services;

public sealed class SecureAuthTokenStore : IAuthTokenStore
{
    private const string Key = "uniflow_jwt";

    public Task<string?> GetTokenAsync(CancellationToken cancellationToken = default) =>
        SecureStorage.Default.GetAsync(Key);

    public Task SetTokenAsync(string token, CancellationToken cancellationToken = default) =>
        SecureStorage.Default.SetAsync(Key, token);

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        SecureStorage.Default.Remove(Key);
        await Task.CompletedTask;
    }

    public async Task<bool> HasTokenAsync(CancellationToken cancellationToken = default)
    {
        var t = await GetTokenAsync(cancellationToken).ConfigureAwait(false);
        return !string.IsNullOrEmpty(t);
    }
}
