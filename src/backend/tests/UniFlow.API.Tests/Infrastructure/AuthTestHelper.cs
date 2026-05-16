using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UniFlow.Business.Contracts.Auth;

namespace UniFlow.API.Tests.Infrastructure;

internal static class AuthTestHelper
{
    public static RegisterRequest CreateValidRegisterRequest(string? uniqueSuffix = null)
    {
        var suffix = uniqueSuffix ?? Guid.NewGuid().ToString("N")[..12];
        return new RegisterRequest
        {
            Email = $"integration.{suffix}@example.com",
            DisplayName = "Integration Test User",
            Password = "ValidPass1!",
        };
    }

    public static LoginRequest CreateLoginRequest(string email, string password) =>
        new() { Email = email, Password = password };

    public static async Task<ApiResultDto<AuthResponse>> RegisterAsync(
        HttpClient client,
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await client.PostAsJsonAsync(AuthRoutes.Register, request, cancellationToken)
            .ConfigureAwait(false);
        var body = await ReadResultAsync<AuthResponse>(response, cancellationToken).ConfigureAwait(false);
        return body ?? throw new InvalidOperationException("Register response body was empty.");
    }

    public static async Task<(HttpResponseMessage Response, ApiResultDto<AuthResponse>? Body)> LoginAsync(
        HttpClient client,
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await client.PostAsJsonAsync(AuthRoutes.Login, request, cancellationToken)
            .ConfigureAwait(false);
        var body = await ReadResultAsync<AuthResponse>(response, cancellationToken).ConfigureAwait(false);
        return (response, body);
    }

    public static Task<ApiResultDto<T>?> ReadResultAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default) =>
        response.Content.ReadFromJsonAsync<ApiResultDto<T>>(ApiTestJson.Options, cancellationToken);

    public static HttpClient WithBearerToken(HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
