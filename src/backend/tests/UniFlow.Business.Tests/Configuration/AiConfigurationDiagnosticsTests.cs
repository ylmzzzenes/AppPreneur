using FluentAssertions;
using Microsoft.Extensions.Hosting;
using UniFlow.Business.Ai;
using UniFlow.Business.Configuration;
using Xunit;

namespace UniFlow.Business.Tests.Configuration;

public sealed class AiConfigurationDiagnosticsTests
{
    [Fact]
    public void Create_DoesNotExposeApiKey_InSnapshot()
    {
        var snapshot = AiConfigurationDiagnostics.Create(
            Environments.Development,
            new AiOptions
            {
                Provider = AiProviders.Gemini,
                ApiKey = "super-secret-key",
                Model = "gemini-2.5-flash",
                EnableFallback = true,
            });

        snapshot.ApiKeyConfigured.Should().BeTrue();
        snapshot.ToString().Should().NotContain("super-secret");
    }

    [Fact]
    public void Create_GeminiWithoutKey_InDevelopment_DescribesFallbackBehavior()
    {
        var snapshot = AiConfigurationDiagnostics.Create(
            Environments.Development,
            new AiOptions
            {
                Provider = AiProviders.Gemini,
                ApiKey = "",
                Model = "gemini-2.5-flash",
                EnableFallback = true,
            });

        snapshot.ApiKeyConfigured.Should().BeFalse();
        snapshot.BaseUrl.Should().BeNull();
        snapshot.EffectiveBehavior.Should().Contain("No API key");
        snapshot.EffectiveBehavior.Should().Contain("AI_CONFIG");
    }

    [Fact]
    public void Create_OpenAiCompatible_IncludesBaseUrl()
    {
        var snapshot = AiConfigurationDiagnostics.Create(
            Environments.Development,
            new AiOptions
            {
                Provider = AiProviders.OpenAiCompatible,
                ApiKey = "key",
                BaseUrl = "https://api.groq.com/openai/v1",
                Model = "llama-3.1-8b-instant",
            });

        snapshot.BaseUrl.Should().Be("https://api.groq.com/openai/v1");
    }

    [Fact]
    public void Create_FakeProvider_DescribesDeterministicBehavior()
    {
        var snapshot = AiConfigurationDiagnostics.Create(
            Environments.Development,
            new AiOptions
            {
                Provider = AiProviders.Fake,
                Model = "fake-model",
            });

        snapshot.EffectiveBehavior.Should().Contain("Fake provider");
    }
}
