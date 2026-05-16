using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniFlow.API.Tests.Infrastructure;
using UniFlow.Business.Contracts.Auth;
using UniFlow.Business.Contracts.Dashboard;
using Xunit;

namespace UniFlow.API.Tests.Integration;

public sealed class AuthIntegrationTests : IClassFixture<UniFlowApiFactory>
{
    private readonly UniFlowApiFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(UniFlowApiFactory factory)
    {
        _factory = factory;
        factory.ResetDatabaseAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsSuccessWithToken()
    {
        var request = AuthTestHelper.CreateValidRegisterRequest();

        var response = await _client.PostAsJsonAsync(AuthRoutes.Register, request);
        var body = await AuthTestHelper.ReadResultAsync<AuthResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.Email.Should().Be(request.Email.Trim().ToLowerInvariant());
        body.Data.UserId.Should().BePositive();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var request = AuthTestHelper.CreateValidRegisterRequest("duplicate");

        var first = await AuthTestHelper.RegisterAsync(_client, request);
        first.IsSuccess.Should().BeTrue();

        var duplicateResponse = await _client.PostAsJsonAsync(AuthRoutes.Register, request);
        var duplicateBody = await AuthTestHelper.ReadResultAsync<AuthResponse>(duplicateResponse);

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        duplicateBody.Should().NotBeNull();
        duplicateBody!.IsSuccess.Should().BeFalse();
        duplicateBody.Error!.Code.Should().Be("AUTH_DUPLICATE_EMAIL");
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var request = AuthTestHelper.CreateValidRegisterRequest();
        await AuthTestHelper.RegisterAsync(_client, request);

        var (response, body) = await AuthTestHelper.LoginAsync(
            _client,
            AuthTestHelper.CreateLoginRequest(request.Email, request.Password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.Email.Should().Be(request.Email.Trim().ToLowerInvariant());
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var request = AuthTestHelper.CreateValidRegisterRequest();
        await AuthTestHelper.RegisterAsync(_client, request);

        var (response, body) = await AuthTestHelper.LoginAsync(
            _client,
            AuthTestHelper.CreateLoginRequest(request.Email, "WrongPass9!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeFalse();
        body.Error!.Code.Should().Be("AUTH_INVALID");
    }

    [Fact]
    public async Task Register_InvalidEmail_ReturnsBadRequest()
    {
        var request = AuthTestHelper.CreateValidRegisterRequest();
        request.Email = "not-an-email";

        var response = await _client.PostAsJsonAsync(AuthRoutes.Register, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShortPassword_ReturnsBadRequest()
    {
        var request = AuthTestHelper.CreateValidRegisterRequest();
        request.Password = "short";

        var response = await _client.PostAsJsonAsync(AuthRoutes.Register, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Response_AccessTokenIsNotEmpty()
    {
        var request = AuthTestHelper.CreateValidRegisterRequest();
        await AuthTestHelper.RegisterAsync(_client, request);

        var (_, body) = await AuthTestHelper.LoginAsync(
            _client,
            AuthTestHelper.CreateLoginRequest(request.Email, request.Password));

        body!.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsOk()
    {
        var request = AuthTestHelper.CreateValidRegisterRequest();
        var registered = await AuthTestHelper.RegisterAsync(_client, request);
        registered.IsSuccess.Should().BeTrue();

        using var authedClient = _factory.CreateBearerClient(registered.Data!.AccessToken);

        var response = await authedClient.GetAsync(AuthRoutes.DashboardToday);
        var body = await AuthTestHelper.ReadResultAsync<DashboardTodayResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeTrue();
    }
}
