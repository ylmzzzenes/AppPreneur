using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.AiProduct;
using UniFlow.Business.Configuration;
using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.ReadModels;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class TaskFeedbackService(
    ITaskQueries taskQueries,
    IUserQueries userQueries,
    IAiProvider aiProvider,
    IOptions<AiOptions> aiOptions,
    ILogger<TaskFeedbackService> logger) : ITaskFeedbackService
{
    public async Task<Result<TaskFeedbackResponse>> GenerateAsync(
        long userId,
        TaskFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await taskQueries.GetOwnedAsync(request.TaskId, userId, cancellationToken).ConfigureAwait(false);
        if (task is null)
        {
            return Result<TaskFeedbackResponse>.Fail("TASK_NOT_FOUND", "Task was not found.");
        }

        var summary = MapSummary(task);
        var profile = await userQueries.GetAiProfileContextAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? new AiUserProfileContext();

        var options = aiOptions.Value;
        var prompt = BuildPrompt(profile, summary, request.NewStatus);
        var sw = Stopwatch.StartNew();

        try
        {
            var generated = await aiProvider.GenerateTextAsync(
                new AiTextRequest
                {
                    UserPrompt = prompt,
                    PromptVersion = options.PromptVersion,
                    Model = options.Model,
                    Temperature = 0.4,
                    Metadata = new Dictionary<string, string>
                    {
                        ["kind"] = "task-feedback",
                        ["taskId"] = request.TaskId.ToString(),
                        ["status"] = request.NewStatus.ToString(),
                    },
                },
                cancellationToken).ConfigureAwait(false);

            sw.Stop();
            logger.LogInformation(
                "Task feedback generated. UserId={UserId}, TaskId={TaskId}, Provider={Provider}, InputLength={InputLength}, OutputLength={OutputLength}, ElapsedMs={ElapsedMs}",
                userId, request.TaskId, generated.Provider, prompt.Length, generated.Content.Length, sw.ElapsedMilliseconds);

            var parsed = TaskFeedbackJsonParser.Parse(generated.Content);
            if (!parsed.IsSuccess || parsed.Data is null)
            {
                return Result<TaskFeedbackResponse>.Success(
                    TaskFeedbackFallbackBuilder.Build(summary, request.NewStatus, profile));
            }

            parsed.Data.IsFallback = generated.IsFallback;
            return Result<TaskFeedbackResponse>.Success(parsed.Data);
        }
        catch (AiProviderException ex)
        {
            sw.Stop();
            AiRequestLogger.LogFailed(logger, ex.Provider, ex.Code);
            return Result<TaskFeedbackResponse>.Success(
                TaskFeedbackFallbackBuilder.Build(summary, request.NewStatus, profile));
        }
    }

    private static TaskItemSummary MapSummary(Entity.Entities.TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        DueDate = task.DueDate,
        PriorityScore = task.PriorityScore,
        Status = task.Status,
        CourseCode = task.Syllabus.Course.Code,
        CourseTitle = task.Syllabus.Course.Title,
    };

    private static string BuildPrompt(AiUserProfileContext profile, TaskItemSummary task, Entity.Enums.TaskItemStatus newStatus)
    {
        var template = AiPromptLoader.Load("task-feedback.md");
        return template
            .Replace("{{DISPLAY_NAME}}", profile.DisplayName, StringComparison.Ordinal)
            .Replace("{{PERSONALITY_VIBE}}", profile.PersonalityVibe.ToString(), StringComparison.Ordinal)
            .Replace("{{TASK_TITLE}}", task.Title, StringComparison.Ordinal)
            .Replace("{{COURSE_CODE}}", task.CourseCode, StringComparison.Ordinal)
            .Replace("{{COURSE_TITLE}}", task.CourseTitle, StringComparison.Ordinal)
            .Replace("{{DUE_DATE}}", task.DueDate?.ToString("yyyy-MM-dd") ?? "Belirtilmedi", StringComparison.Ordinal)
            .Replace("{{NEW_STATUS}}", newStatus.ToString(), StringComparison.Ordinal)
            .Replace("{{PRIORITY_SCORE}}", (task.PriorityScore ?? 0).ToString(), StringComparison.Ordinal);
    }
}
