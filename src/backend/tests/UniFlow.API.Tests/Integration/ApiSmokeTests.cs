using System.Net;
using FluentAssertions;
using UniFlow.API.Tests.Infrastructure;
using Xunit;

namespace UniFlow.API.Tests.Integration;

public sealed class ApiSmokeTests : IClassFixture<UniFlowApiFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(UniFlowApiFactory factory)
    {
        factory.ResetDatabaseAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_ReturnsOk_InTestingEnvironment()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedDashboard_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/dashboard/today");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Root_ReturnsOk_InTestingEnvironment()
    {
        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
