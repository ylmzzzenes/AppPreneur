using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.Configuration;
using UniFlow.Business.Services;
using Xunit;

namespace UniFlow.Business.Tests.Services;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task ReplyAsync_UsesAiProvider_NotLegacyGeminiService()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResponse
            {
                Content = "Mocked chat reply",
                Provider = AiProviders.Fake,
                Model = "fake-model",
                PromptVersion = "v1",
            });

        IChatService chatService = new ChatService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions { PromptVersion = "v1", Model = "fake-model" }),
            NullLogger<ChatService>.Instance);

        var result = await chatService.ReplyAsync("Merhaba");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("Mocked chat reply");

        await aiProvider.Received(1).GenerateTextAsync(
            Arg.Is<AiTextRequest>(r =>
                r.UserPrompt.Contains("Merhaba", StringComparison.Ordinal)
                && r.Metadata != null
                && r.Metadata["kind"] == "chat"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplyAsync_MapsAiProviderException_ToResultFail()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<AiTextResponse>>(_ => throw new AiProviderException("AI_CONFIG", "missing key", AiProviders.Gemini));

        var chatService = new ChatService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions()),
            NullLogger<ChatService>.Instance);

        var result = await chatService.ReplyAsync("test");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("AI_CONFIG");
    }
}
