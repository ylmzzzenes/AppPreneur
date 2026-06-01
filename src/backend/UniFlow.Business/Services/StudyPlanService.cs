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
using UniFlow.Entity.Enums;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class StudyPlanService(
    ITaskQueries taskQueries,
    ICourseQueries courseQueries,
    IUserQueries userQueries,
    IAiProvider aiProvider,
    IOptions<AiOptions> aiOptions,
    ILogger<StudyPlanService> logger) : IStudyPlanService
{
    public async Task<Result<StudyPlanResponse>> GenerateAsync(
        long userId,
        StudyPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CourseId.HasValue)
        {
            var course = await courseQueries.GetOwnedAsync(request.CourseId.Value, userId, cancellationToken)
                .ConfigureAwait(false);
            if (course is null)
            {
                return Result<StudyPlanResponse>.Fail("COURSE_NOT_FOUND", "Course was not found.");
            }
        }

        var profile = await userQueries.GetAiProfileContextAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? new AiUserProfileContext();

        var allTasks = await taskQueries.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var today = DateTime.UtcNow.Date;
        var end = today.AddDays(request.Days);

        var filtered = allTasks
            .Where(t => t.Status != TaskItemStatus.Done)
            .Where(t => !t.DueDate.HasValue || t.DueDate.Value.Date <= end)
            .Where(t => !request.CourseId.HasValue || t.CourseId == request.CourseId.Value)
            .OrderByDescending(t => t.PriorityScore ?? 0)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .Take(StudyPlanFallbackBuilder.MaxTasks)
            .ToList();

        var options = aiOptions.Value;
        var startDate = today;
        var prompt = BuildPrompt(profile, request, filtered, startDate, options.PromptVersion);
        var sw = Stopwatch.StartNew();

        try
        {
            var generated = await aiProvider.GenerateTextAsync(
                new AiTextRequest
                {
                    UserPrompt = prompt,
                    PromptVersion = options.PromptVersion,
                    Model = options.Model,
                    Temperature = 0.3,
                    Metadata = new Dictionary<string, string>
                    {
                        ["kind"] = "study-plan",
                        ["taskCount"] = filtered.Count.ToString(),
                        ["courseId"] = request.CourseId?.ToString() ?? "all",
                    },
                },
                cancellationToken).ConfigureAwait(false);

            sw.Stop();
            LogCompleted(userId, generated, prompt.Length, filtered.Count, request.CourseId, sw.ElapsedMilliseconds);

            var parsed = StudyPlanJsonParser.Parse(generated.Content);
            if (!parsed.IsSuccess || parsed.Data is null)
            {
                logger.LogWarning(
                    "Study plan JSON parse failed. UserId={UserId}, TaskCount={TaskCount}, Code={Code}",
                    userId, filtered.Count, parsed.Error?.Code);
                return Result<StudyPlanResponse>.Success(
                    StudyPlanFallbackBuilder.Build(filtered, request.Days, request.Focus, startDate));
            }

            parsed.Data.IsFallback = generated.IsFallback;
            return Result<StudyPlanResponse>.Success(parsed.Data);
        }
        catch (AiProviderException ex)
        {
            sw.Stop();
            AiRequestLogger.LogFailed(logger, ex.Provider, ex.Code);
            logger.LogWarning(
                "Study plan AI failed, using fallback. UserId={UserId}, TaskCount={TaskCount}, ElapsedMs={ElapsedMs}",
                userId, filtered.Count, sw.ElapsedMilliseconds);
            return Result<StudyPlanResponse>.Success(
                StudyPlanFallbackBuilder.Build(filtered, request.Days, request.Focus, startDate));
        }
    }

    private static string BuildPrompt(
        AiUserProfileContext profile,
        StudyPlanRequest request,
        IReadOnlyList<Entity.ReadModels.TaskItemSummary> tasks,
        DateTime startDate,
        string promptVersion)
    {
        var template = AiPromptLoader.Load("study-plan-generate.md");
        return template
            .Replace("{{DISPLAY_NAME}}", profile.DisplayName, StringComparison.Ordinal)
            .Replace("{{MAJOR}}", profile.Major ?? "Belirtilmedi", StringComparison.Ordinal)
            .Replace("{{ACADEMIC_GOAL}}", profile.AcademicGoal ?? "Belirtilmedi", StringComparison.Ordinal)
            .Replace("{{PERSONALITY_VIBE}}", profile.PersonalityVibe.ToString(), StringComparison.Ordinal)
            .Replace("{{DAYS}}", request.Days.ToString(), StringComparison.Ordinal)
            .Replace("{{FOCUS}}", request.Focus ?? "Genel çalışma", StringComparison.Ordinal)
            .Replace("{{START_DATE}}", startDate.ToString("yyyy-MM-dd"), StringComparison.Ordinal)
            .Replace("{{TASKS_JSON}}", StudyPlanFallbackBuilder.BuildTasksJson(tasks), StringComparison.Ordinal)
            .Replace("{{PROMPT_VERSION}}", promptVersion, StringComparison.Ordinal);
    }

    private void LogCompleted(long userId, AiTextResponse response, int inputLength, int taskCount, long? courseId, long elapsedMs)
    {
        logger.LogInformation(
            "Study plan generated. UserId={UserId}, Provider={Provider}, Model={Model}, PromptVersion={PromptVersion}, InputLength={InputLength}, OutputLength={OutputLength}, TaskCount={TaskCount}, CourseId={CourseId}, ElapsedMs={ElapsedMs}, IsFallback={IsFallback}",
            userId, response.Provider, response.Model, response.PromptVersion,
            inputLength, response.Content.Length, taskCount, courseId, elapsedMs, response.IsFallback);
    }
}
