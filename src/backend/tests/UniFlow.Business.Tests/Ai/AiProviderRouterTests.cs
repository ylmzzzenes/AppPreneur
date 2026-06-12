using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using UniFlow.Business.Ai;
using UniFlow.Business.Ai.Providers;
using UniFlow.Business.Configuration;
using UniFlow.Business.Configuration.Validators;
using Xunit;

namespace UniFlow.Business.Tests.Ai;

public sealed class AiProviderRouterTests
{
    [Theory]
    [InlineData(AiProviders.Gemini, typeof(GeminiAiProvider))]
    [InlineData(AiProviders.OpenAiCompatible, typeof(OpenAiCompatibleProvider))]
    [InlineData(AiProviders.Fake, typeof(FakeAiProvider))]
    public async Task ResolveProvider_RoutesToExpectedImplementation(string providerName, Type expectedInnerType)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<AiOptions>().Configure(o =>
        {
            o.Provider = providerName;
            o.Model = providerName == AiProviders.OpenAiCompatible ? "gpt-4o-mini" : "gemini-2.0-flash";
            o.BaseUrl = "https://api.openai.com/v1";
            o.ApiKey = providerName == AiProviders.Fake ? string.Empty : "test-key";
        });
        services.AddSingleton<GeminiAiProvider>();
        services.AddSingleton<OpenAiCompatibleProvider>();
        services.AddSingleton<FakeAiProvider>();
        services.AddSingleton<IAiProvider, AiProviderRouter>();

        services.AddHttpClient(GeminiAiProvider.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new StubHttpMessageHandler(
                """{"candidates":[{"content":{"parts":[{"text":"gemini-ok"}]}}]}"""));
        services.AddHttpClient(OpenAiCompatibleProvider.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new StubHttpMessageHandler(
                """{"choices":[{"message":{"content":"ok"}}]}"""));

        var sp = services.BuildServiceProvider();
        var router = sp.GetRequiredService<IAiProvider>();

        var response = await router.GenerateTextAsync(new AiTextRequest { UserPrompt = "hello" });

        response.Provider.Should().BeEquivalentTo(providerName);
        response.Content.Should().NotBeNullOrWhiteSpace();
        sp.GetRequiredService(expectedInnerType).Should().NotBeNull();
    }

    [Fact]
    public void UnknownProvider_ThrowsAtResolveTime()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<AiOptions>().Configure(o =>
        {
            o.Provider = "UnknownProvider";
            o.Model = "test";
        });
        services.AddSingleton<GeminiAiProvider>();
        services.AddSingleton<OpenAiCompatibleProvider>();
        services.AddSingleton<FakeAiProvider>();
        services.AddSingleton<IAiProvider, AiProviderRouter>();
        services.AddHttpClient(GeminiAiProvider.HttpClientName);
        services.AddHttpClient(OpenAiCompatibleProvider.HttpClientName);

        var sp = services.BuildServiceProvider();
        var router = sp.GetRequiredService<IAiProvider>();

        var act = () => router.GenerateTextAsync(new AiTextRequest { UserPrompt = "x" }).GetAwaiter().GetResult();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*UnknownProvider*");
    }

    private sealed class StubHttpMessageHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.Contains("chat/completions", StringComparison.Ordinal) == true)
            {
                var json = JsonSerializer.Serialize(new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "user", content = "hello" },
                    },
                });
                request.Content!.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult()
                    .Should().Contain("hello");
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }
}

public sealed class AiOptionsValidatorTests
{
    private static AiOptionsValidator CreateValidator(string environment = "Development") =>
        new(new FakeHostEnvironment(environment));

    [Fact]
    public void UnknownProvider_ReturnsValidationError()
    {
        var result = CreateValidator().Validate(null, new AiOptions
        {
            Provider = "BadProvider",
            Model = "test",
        });

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("BadProvider");
    }

    [Fact]
    public void OpenAiCompatible_WithoutBaseUrl_FailsValidation()
    {
        var result = CreateValidator().Validate(null, new AiOptions
        {
            Provider = AiProviders.OpenAiCompatible,
            Model = "gpt-4o-mini",
            BaseUrl = "",
            ApiKey = "key",
        });

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("BaseUrl");
    }

    [Fact]
    public void FakeProvider_InDevelopment_DoesNotRequireApiKey()
    {
        var result = CreateValidator("Development").Validate(null, new AiOptions
        {
            Provider = AiProviders.Fake,
            Model = "fake",
        });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void FakeProvider_InTesting_DoesNotRequireApiKey()
    {
        var result = CreateValidator("Testing").Validate(null, new AiOptions
        {
            Provider = AiProviders.Fake,
            Model = "fake",
        });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void FakeProvider_InProduction_FailsValidation()
    {
        var result = CreateValidator("Production").Validate(null, new AiOptions
        {
            Provider = AiProviders.Fake,
            Model = "fake",
        });

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Fake");
    }

    [Fact]
    public void OpenAiCompatible_InProduction_WithoutApiKey_FailsValidation()
    {
        var result = CreateValidator("Production").Validate(null, new AiOptions
        {
            Provider = AiProviders.OpenAiCompatible,
            Model = "gpt-4o-mini",
            BaseUrl = "https://api.example.com/v1",
            ApiKey = "",
        });

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ApiKey");
    }

    [Fact]
    public void Gemini_InProduction_WithoutApiKey_FailsValidation()
    {
        var result = CreateValidator("Production").Validate(null, new AiOptions
        {
            Provider = AiProviders.Gemini,
            Model = "gemini-2.0-flash",
            ApiKey = "",
        });

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ApiKey");
    }

    [Fact]
    public void Gemini_InDevelopment_WithoutApiKey_PassesValidation()
    {
        var result = CreateValidator("Development").Validate(null, new AiOptions
        {
            Provider = AiProviders.Gemini,
            Model = "gemini-2.5-flash",
            ApiKey = "",
            EnableFallback = true,
        });

        result.Succeeded.Should().BeTrue();
    }

    private sealed class FakeHostEnvironment(string name) : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
