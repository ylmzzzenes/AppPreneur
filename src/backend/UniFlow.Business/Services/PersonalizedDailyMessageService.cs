using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.AiProduct;
using UniFlow.Business.Configuration;
using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Entity.ReadModels;

namespace UniFlow.Business.Services;

public sealed class PersonalizedDailyMessageService(
    IDailyMessageService dailyMessageService,
    IAiProvider aiProvider,
    IOptions<AiOptions> aiOptions,
    ILogger<PersonalizedDailyMessageService> logger) : IPersonalizedDailyMessageService
{
    public async Task<string> BuildDailyMessageAsync(
        DailyMessageContext context,
        AiUserProfileContext? profile,
        CancellationToken cancellationToken = default)
    {
        var options = aiOptions.Value;

        if (string.Equals(options.Provider, AiProviders.Fake, StringComparison.OrdinalIgnoreCase)
            || (string.IsNullOrWhiteSpace(options.ApiKey) && options.EnableFallback))
        {
            return dailyMessageService.BuildDailyMessage(context);
        }

        profile ??= new AiUserProfileContext { PersonalityVibe = context.PersonalityVibe };

        var prompt = BuildPrompt(context, profile);
        var sw = Stopwatch.StartNew();

        try
        {
            var generated = await aiProvider.GenerateTextAsync(
                new AiTextRequest
                {
                    UserPrompt = prompt,
                    PromptVersion = options.PromptVersion,
                    Model = options.Model,
                    Temperature = 0.5,
                    Metadata = new Dictionary<string, string> { ["kind"] = "daily-message" },
                },
                cancellationToken).ConfigureAwait(false);

            sw.Stop();
            var message = generated.Content.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return dailyMessageService.BuildDailyMessage(context);
            }

            logger.LogInformation(
                "Daily message generated. UserId={UserId}, Provider={Provider}, InputLength={InputLength}, OutputLength={OutputLength}, ElapsedMs={ElapsedMs}, IsFallback={IsFallback}",
                context.UserId, generated.Provider, prompt.Length, message.Length, sw.ElapsedMilliseconds, generated.IsFallback);

            return message;
        }
        catch (AiProviderException ex)
        {
            sw.Stop();
            AiRequestLogger.LogFailed(logger, ex.Provider, ex.Code);
            return dailyMessageService.BuildDailyMessage(context);
        }
    }

    private static string BuildPrompt(DailyMessageContext context, AiUserProfileContext profile)
    {
        var bigThree = context.BigThreeTasks.Count > 0
            ? string.Join(", ", context.BigThreeTasks.Select(t => t.Title))
            : "yok";

        var template = AiPromptLoader.Load("daily-message-personalized.md");
        return template
            .Replace("{{DISPLAY_NAME}}", profile.DisplayName, StringComparison.Ordinal)
            .Replace("{{MAJOR}}", profile.Major ?? "Belirtilmedi", StringComparison.Ordinal)
            .Replace("{{ACADEMIC_GOAL}}", profile.AcademicGoal ?? "Belirtilmedi", StringComparison.Ordinal)
            .Replace("{{PERSONALITY_VIBE}}", profile.PersonalityVibe.ToString(), StringComparison.Ordinal)
            .Replace("{{PENDING_TODAY}}", context.PendingTodayCount.ToString(), StringComparison.Ordinal)
            .Replace("{{COMPLETED_TODAY}}", context.CompletedTodayCount.ToString(), StringComparison.Ordinal)
            .Replace("{{OVERDUE_COUNT}}", context.OverdueTasksCount.ToString(), StringComparison.Ordinal)
            .Replace("{{BIG_THREE_TITLES}}", bigThree, StringComparison.Ordinal);
    }
}
