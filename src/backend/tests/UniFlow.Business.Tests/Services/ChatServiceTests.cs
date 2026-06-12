using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using UniFlow.Business.Ai;
using UniFlow.Business.Configuration;
using UniFlow.Business.Services;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;
using Xunit;

namespace UniFlow.Business.Tests.Services;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task ReplyAsync_IncludesUserContext_InUserPrompt()
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

        var userQueries = Substitute.For<IUserQueries>();
        userQueries.GetAiProfileContextAsync(1, Arg.Any<CancellationToken>())
            .Returns(new AiUserProfileContext { DisplayName = "Enes", Major = "BM" });

        var courseQueries = Substitute.For<ICourseQueries>();
        courseQueries.ListForUserAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<CourseSummary>
            {
                new() { Code = "MAT111", Title = "Matematik I", ActiveTaskCount = 2, TaskCount = 5 },
            });

        var taskQueries = Substitute.For<ITaskQueries>();
        taskQueries.ListForUserAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<TaskItemSummary>
            {
                new()
                {
                    Title = "Vize hazırlığı",
                    CourseCode = "MAT111",
                    Status = TaskItemStatus.Pending,
                    PriorityScore = 80,
                },
            });

        var chatService = new ChatService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions { PromptVersion = "v1", Model = "fake-model" }),
            userQueries,
            courseQueries,
            taskQueries,
            NullLogger<ChatService>.Instance);

        var result = await chatService.ReplyAsync(1, "Görevlerim nasıl?");

        result.IsSuccess.Should().BeTrue();
        await aiProvider.Received(1).GenerateTextAsync(
            Arg.Is<AiTextRequest>(r =>
                r.UserPrompt.Contains("MAT111", StringComparison.Ordinal)
                && r.UserPrompt.Contains("Vize hazırlığı", StringComparison.Ordinal)
                && r.UserPrompt.Contains("Görevlerim nasıl?", StringComparison.Ordinal)
                && r.Metadata != null
                && r.Metadata["kind"] == "chat"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplyAsync_UsesCustomChatSystemPrompt_WhenConfigured()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResponse
            {
                Content = "Custom system reply",
                Provider = AiProviders.Gemini,
                Model = "gemini-2.5-flash",
                PromptVersion = "v1",
            });

        var chatService = CreateChatService(aiProvider, new AiOptions
        {
            PromptVersion = "v1",
            Model = "gemini-2.5-flash",
            ChatSystemPrompt = "Özel sistem talimatı.",
        });

        await chatService.ReplyAsync(1, "Selam");

        await aiProvider.Received(1).GenerateTextAsync(
            Arg.Is<AiTextRequest>(r => r.SystemPrompt == "Özel sistem talimatı." && r.UserPrompt.Contains("Selam", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplyAsync_MapsAiProviderException_ToResultFail()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<AiTextResponse>>(_ => throw new AiProviderException("AI_CONFIG", "missing key", AiProviders.Gemini));

        var chatService = CreateChatService(aiProvider, new AiOptions());

        var result = await chatService.ReplyAsync(1, "test");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("AI_CONFIG");
    }

    private static ChatService CreateChatService(IAiProvider aiProvider, AiOptions options)
    {
        var userQueries = Substitute.For<IUserQueries>();
        userQueries.GetAiProfileContextAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(new AiUserProfileContext());

        var courseQueries = Substitute.For<ICourseQueries>();
        courseQueries.ListForUserAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<CourseSummary>());

        var taskQueries = Substitute.For<ITaskQueries>();
        taskQueries.ListForUserAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TaskItemSummary>());

        return new ChatService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(options),
            userQueries,
            courseQueries,
            taskQueries,
            NullLogger<ChatService>.Instance);
    }
}
