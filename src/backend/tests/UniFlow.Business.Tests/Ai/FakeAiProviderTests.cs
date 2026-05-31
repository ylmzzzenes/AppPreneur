using FluentAssertions;
using Microsoft.Extensions.Options;
using UniFlow.Business.Ai;
using UniFlow.Business.Ai.Providers;
using UniFlow.Business.Configuration;
using Xunit;

namespace UniFlow.Business.Tests.Ai;

public sealed class FakeAiProviderTests
{
    [Fact]
    public async Task GenerateTextAsync_ReturnsDeterministicContent()
    {
        var provider = new FakeAiProvider(Microsoft.Extensions.Options.Options.Create(new AiOptions { PromptVersion = "v1" }));

        var first = await provider.GenerateTextAsync(new AiTextRequest { UserPrompt = "anything" });
        var second = await provider.GenerateTextAsync(new AiTextRequest { UserPrompt = "different" });

        first.Content.Should().Be(second.Content);
        first.Provider.Should().Be(AiProviders.Fake);
        first.IsFallback.Should().BeTrue();
        first.Model.Should().Be("fake-model");
        first.PromptVersion.Should().Be("v1");
    }

    [Fact]
    public async Task GenerateTextAsync_ChatKind_ReturnsChatReply()
    {
        var provider = new FakeAiProvider(Microsoft.Extensions.Options.Options.Create(new AiOptions()));

        var response = await provider.GenerateTextAsync(new AiTextRequest
        {
            UserPrompt = "hi",
            Metadata = new Dictionary<string, string> { ["kind"] = "chat" },
        });

        response.Content.Should().Be("Fake assistant reply for local testing.");
    }
}
