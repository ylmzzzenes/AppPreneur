using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using UniFlow.Business.Ai;
using UniFlow.Business.AiProduct;
using UniFlow.Business.Configuration;
using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Business.Services;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;
using Xunit;

namespace UniFlow.Business.Tests.Services;

public sealed class PersonalizedDailyMessageServiceTests
{
    [Fact]
    public async Task FakeProvider_UsesDeterministicFallback()
    {
        var daily = new DailyMessageService();
        var ai = Substitute.For<IAiProvider>();
        var options = Microsoft.Extensions.Options.Options.Create(new AiOptions { Provider = AiProviders.Fake, EnableFallback = true });

        var sut = new PersonalizedDailyMessageService(daily, ai, options, NullLogger<PersonalizedDailyMessageService>.Instance);

        var context = new DailyMessageContext
        {
            UserId = 1,
            Today = new DateTime(2026, 6, 1),
            PersonalityVibe = PersonalityVibe.Friendly,
            OverdueTasksCount = 1,
            CompletedTodayCount = 0,
            PendingTodayCount = 2,
        };

        var message = await sut.BuildDailyMessageAsync(context, new AiUserProfileContext(), CancellationToken.None);

        message.Should().NotBeNullOrWhiteSpace();
        await ai.DidNotReceive().GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AiFailure_ReturnsDeterministicFallback()
    {
        var daily = new DailyMessageService();
        var ai = Substitute.For<IAiProvider>();
        ai.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<AiTextResponse>>(_ => throw new AiProviderException("AI_HTTP", "fail", AiProviders.Gemini));

        var options = Microsoft.Extensions.Options.Options.Create(new AiOptions
        {
            Provider = AiProviders.Gemini,
            ApiKey = "key",
            Model = "gemini-2.0-flash",
        });

        var sut = new PersonalizedDailyMessageService(daily, ai, options, NullLogger<PersonalizedDailyMessageService>.Instance);

        var context = new DailyMessageContext
        {
            UserId = 1,
            Today = new DateTime(2026, 6, 1),
            PersonalityVibe = PersonalityVibe.Friendly,
        };

        var message = await sut.BuildDailyMessageAsync(context, new AiUserProfileContext(), CancellationToken.None);

        message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AiSuccess_ReturnsAiMessage()
    {
        var daily = new DailyMessageService();
        var ai = Substitute.For<IAiProvider>();
        ai.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResponse { Content = "AI personalized message", Provider = AiProviders.Gemini, Model = "m" });

        var options = Microsoft.Extensions.Options.Options.Create(new AiOptions
        {
            Provider = AiProviders.Gemini,
            ApiKey = "key",
            Model = "gemini-2.0-flash",
        });

        var sut = new PersonalizedDailyMessageService(daily, ai, options, NullLogger<PersonalizedDailyMessageService>.Instance);

        var context = new DailyMessageContext { UserId = 1, Today = DateTime.UtcNow.Date, PersonalityVibe = PersonalityVibe.Friendly };

        var message = await sut.BuildDailyMessageAsync(context, new AiUserProfileContext(), CancellationToken.None);

        message.Should().Be("AI personalized message");
    }
}

public sealed class StudyPlanJsonParserTests
{
    [Fact]
    public void Parse_ValidJson_ReturnsPlan()
    {
        const string json = """
            {
              "title": "Plan",
              "summary": "Summary",
              "days": [
                {
                  "date": "2026-06-01",
                  "focus": "Focus",
                  "tasks": [{ "title": "Task", "estimatedMinutes": 30, "reason": "Why" }],
                  "tip": "Tip"
                }
              ]
            }
            """;

        var result = StudyPlanJsonParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be("Plan");
        result.Data.Days.Should().ContainSingle();
    }

    [Fact]
    public void Parse_InvalidJson_Fails()
    {
        var result = StudyPlanJsonParser.Parse("not json");
        result.IsSuccess.Should().BeFalse();
    }
}

public sealed class TaskFeedbackFallbackBuilderTests
{
    [Fact]
    public void DoneStatus_ProducesMotivationalMessage()
    {
        var task = new Entity.ReadModels.TaskItemSummary { Title = "Midterm prep" };
        var profile = new AiUserProfileContext { PersonalityVibe = PersonalityVibe.Friendly };

        var feedback = TaskFeedbackFallbackBuilder.Build(task, TaskItemStatus.Done, profile);

        feedback.Message.Should().Contain("Midterm prep");
        feedback.IsFallback.Should().BeTrue();
    }

    [Fact]
    public void MissedStatus_DiffersFromDone()
    {
        var task = new Entity.ReadModels.TaskItemSummary { Title = "Quiz" };
        var profile = new AiUserProfileContext { PersonalityVibe = PersonalityVibe.Strict };

        var done = TaskFeedbackFallbackBuilder.Build(task, TaskItemStatus.Done, profile);
        var missed = TaskFeedbackFallbackBuilder.Build(task, TaskItemStatus.Missed, profile);

        done.Message.Should().NotBe(missed.Message);
    }
}
