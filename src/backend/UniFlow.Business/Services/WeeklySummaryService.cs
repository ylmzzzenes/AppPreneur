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

public sealed class WeeklySummaryService(
    ITaskQueries taskQueries,
    IUserQueries userQueries,
    IAiProvider aiProvider,
    IOptions<AiOptions> aiOptions,
    ILogger<WeeklySummaryService> logger) : IWeeklySummaryService
{
    public async Task<Result<WeeklySummaryResponse>> GetAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await userQueries.GetAiProfileContextAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? new AiUserProfileContext();

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-6);

        var allTasks = await taskQueries.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var weekTasks = allTasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date >= weekStart && t.DueDate.Value.Date <= today)
            .ToList();

        var completed = weekTasks.Count(t => t.Status == TaskItemStatus.Done);
        var missed = weekTasks.Count(t => t.Status == TaskItemStatus.Missed);
        var pending = weekTasks.Count(t => t.Status == TaskItemStatus.Pending);

        var options = aiOptions.Value;
        var prompt = BuildPrompt(profile, weekTasks, completed, missed, pending);
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
                        ["kind"] = "weekly-summary",
                        ["taskCount"] = weekTasks.Count.ToString(),
                    },
                },
                cancellationToken).ConfigureAwait(false);

            sw.Stop();
            logger.LogInformation(
                "Weekly summary generated. UserId={UserId}, Provider={Provider}, InputLength={InputLength}, OutputLength={OutputLength}, TaskCount={TaskCount}, ElapsedMs={ElapsedMs}",
                userId, generated.Provider, prompt.Length, generated.Content.Length, weekTasks.Count, sw.ElapsedMilliseconds);

            var parsed = WeeklySummaryJsonParser.ParseNarrative(generated.Content);
            if (!parsed.IsSuccess || parsed.Data == default)
            {
                return Result<WeeklySummaryResponse>.Success(
                    WeeklySummaryFallbackBuilder.Build(weekTasks, completed, missed, pending));
            }

            var (summary, strong, improve, focus) = parsed.Data;
            return Result<WeeklySummaryResponse>.Success(new WeeklySummaryResponse
            {
                Summary = summary,
                CompletedCount = completed,
                MissedCount = missed,
                PendingCount = pending,
                StrongPoint = strong,
                ImprovementPoint = improve,
                NextWeekFocus = focus,
                IsFallback = generated.IsFallback,
            });
        }
        catch (AiProviderException ex)
        {
            sw.Stop();
            AiRequestLogger.LogFailed(logger, ex.Provider, ex.Code);
            return Result<WeeklySummaryResponse>.Success(
                WeeklySummaryFallbackBuilder.Build(weekTasks, completed, missed, pending));
        }
    }

    private static string BuildPrompt(
        AiUserProfileContext profile,
        IReadOnlyList<Entity.ReadModels.TaskItemSummary> weekTasks,
        int completed,
        int missed,
        int pending)
    {
        var template = AiPromptLoader.Load("weekly-summary.md");
        return template
            .Replace("{{DISPLAY_NAME}}", profile.DisplayName, StringComparison.Ordinal)
            .Replace("{{MAJOR}}", profile.Major ?? "Belirtilmedi", StringComparison.Ordinal)
            .Replace("{{PERSONALITY_VIBE}}", profile.PersonalityVibe.ToString(), StringComparison.Ordinal)
            .Replace("{{COMPLETED_COUNT}}", completed.ToString(), StringComparison.Ordinal)
            .Replace("{{MISSED_COUNT}}", missed.ToString(), StringComparison.Ordinal)
            .Replace("{{PENDING_COUNT}}", pending.ToString(), StringComparison.Ordinal)
            .Replace("{{TASKS_SUMMARY}}", WeeklySummaryFallbackBuilder.BuildTasksSummary(weekTasks), StringComparison.Ordinal);
    }
}
