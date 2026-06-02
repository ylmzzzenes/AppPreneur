using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using UniFlow.API.Configuration;
using UniFlow.Business.Configuration;
using Xunit;

namespace UniFlow.API.Tests.Configuration;

public sealed class AiOptionsSecretsTests
{
    [Fact]
    public void GeminiApiKey_FallsBackFromEnvironmentVariable_WhenAiApiKeyEmpty()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ai:Provider"] = "Gemini",
                ["Ai:Model"] = "gemini-2.0-flash",
                ["GEMINI_API_KEY"] = "legacy-env-key",
                ["Jwt:Key"] = "test-jwt-key-at-least-32-characters-long",
                ["UniFlow:Gemini:Model"] = "gemini-2.0-flash",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment("Development"));
        services.AddUniFlowValidatedOptions(configuration);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>().Value;
        options.ApiKey.Should().Be("legacy-env-key");
    }

    private sealed class TestHostEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
