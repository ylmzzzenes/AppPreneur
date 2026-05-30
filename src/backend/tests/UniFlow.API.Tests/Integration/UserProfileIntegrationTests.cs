using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniFlow.API.Tests.Infrastructure;
using UniFlow.Business.Contracts.Users;
using UniFlow.Entity.Enums;
using Xunit;

namespace UniFlow.API.Tests.Integration;

public sealed class UserProfileIntegrationTests : IClassFixture<UniFlowApiFactory>
{
    private readonly UniFlowApiFactory _factory;
    private readonly HttpClient _client;

    public UserProfileIntegrationTests(UniFlowApiFactory factory)
    {
        _factory = factory;
        factory.ResetDatabaseAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMe_AuthenticatedUser_ReturnsOwnProfile()
    {
        var register = AuthTestHelper.CreateValidRegisterRequest("profile-get");
        var auth = await AuthTestHelper.RegisterAsync(_client, register);
        auth.IsSuccess.Should().BeTrue();

        using var bearerClient = _factory.CreateBearerClient(auth.Data!.AccessToken);
        var response = await bearerClient.GetAsync(UserRoutes.Me);
        var body = await AuthTestHelper.ReadResultAsync<UserProfileResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeTrue();
        body.Data!.Id.Should().Be(auth.Data.UserId);
        body.Data.Email.Should().Be(register.Email.Trim().ToLowerInvariant());
        body.Data.DisplayName.Should().Be(register.DisplayName);
        body.Data.IsOnboardingCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetMe_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync(UserRoutes.Me);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateOnboarding_AuthenticatedUser_UpdatesOwnProfile()
    {
        var register = AuthTestHelper.CreateValidRegisterRequest("profile-patch");
        var auth = await AuthTestHelper.RegisterAsync(_client, register);
        auth.IsSuccess.Should().BeTrue();

        var request = new OnboardingUpdateRequest
        {
            DisplayName = "Elif",
            Major = "Computer Engineering",
            AcademicGoal = "Pass exams and keep weekly study plan",
            PersonalityVibe = PersonalityVibe.Strict,
            DailyStudyTargetMinutes = 120,
        };

        using var bearerClient = _factory.CreateBearerClient(auth.Data!.AccessToken);
        var response = await bearerClient.PatchAsJsonAsync(UserRoutes.Onboarding, request);
        var body = await AuthTestHelper.ReadResultAsync<UserProfileResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeTrue();
        body.Data!.DisplayName.Should().Be("Elif");
        body.Data.Major.Should().Be("Computer Engineering");
        body.Data.AcademicGoal.Should().Be("Pass exams and keep weekly study plan");
        body.Data.PersonalityVibe.Should().Be(PersonalityVibe.Strict);
        body.Data.DailyStudyTargetMinutes.Should().Be(120);
        body.Data.IsOnboardingCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOnboarding_InvalidRequest_ReturnsValidationError()
    {
        var register = AuthTestHelper.CreateValidRegisterRequest("profile-invalid");
        var auth = await AuthTestHelper.RegisterAsync(_client, register);
        auth.IsSuccess.Should().BeTrue();

        var request = new OnboardingUpdateRequest
        {
            DailyStudyTargetMinutes = -5,
        };

        using var bearerClient = _factory.CreateBearerClient(auth.Data!.AccessToken);
        var response = await bearerClient.PatchAsJsonAsync(UserRoutes.Onboarding, request);
        var body = await AuthTestHelper.ReadResultAsync<UserProfileResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOnboarding_OnlyAffectsAuthenticatedUser()
    {
        var userA = AuthTestHelper.CreateValidRegisterRequest("profile-user-a");
        var userB = AuthTestHelper.CreateValidRegisterRequest("profile-user-b");

        var authA = await AuthTestHelper.RegisterAsync(_client, userA);
        var authB = await AuthTestHelper.RegisterAsync(_client, userB);
        authA.IsSuccess.Should().BeTrue();
        authB.IsSuccess.Should().BeTrue();

        var updateRequest = new OnboardingUpdateRequest
        {
            DisplayName = "User A Updated",
            Major = "Physics",
            PersonalityVibe = PersonalityVibe.Motivational,
        };

        using var clientA = _factory.CreateBearerClient(authA.Data!.AccessToken);
        var updateResponse = await clientA.PatchAsJsonAsync(UserRoutes.Onboarding, updateRequest);
        var updateBody = await AuthTestHelper.ReadResultAsync<UserProfileResponse>(updateResponse);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updateBody!.IsSuccess.Should().BeTrue();

        using var clientB = _factory.CreateBearerClient(authB.Data!.AccessToken);
        var profileBResponse = await clientB.GetAsync(UserRoutes.Me);
        var profileB = await AuthTestHelper.ReadResultAsync<UserProfileResponse>(profileBResponse);

        profileBResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        profileB!.Data!.DisplayName.Should().Be(userB.DisplayName);
        profileB.Data.Major.Should().BeNull();
        profileB.Data.IsOnboardingCompleted.Should().BeFalse();
    }
}
