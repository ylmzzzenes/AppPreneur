namespace UniFlow.Mobile.Services;

public interface IAuthTokenStore
{
    Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);

    Task SetTokenAsync(string token, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);

    Task<bool> HasTokenAsync(CancellationToken cancellationToken = default);
}
