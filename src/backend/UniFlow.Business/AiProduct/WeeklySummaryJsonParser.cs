using System.Text.Json;
using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Results;

namespace UniFlow.Business.AiProduct;

internal static class WeeklySummaryJsonParser
{
    internal static Result<(string Summary, string StrongPoint, string ImprovementPoint, string NextWeekFocus)> ParseNarrative(string aiContent)
    {
        var json = ExtractJsonObject(aiContent);
        if (json is null)
        {
            return Result<(string, string, string, string)>.Fail("WEEKLY_SUMMARY_JSON", "No JSON object found in AI response.");
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var summary = root.GetProperty("summary").GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(summary))
            {
                return Result<(string, string, string, string)>.Fail("WEEKLY_SUMMARY_JSON", "Summary is required.");
            }

            return Result<(string, string, string, string)>.Success((
                summary.Trim(),
                root.TryGetProperty("strongPoint", out var strongEl) ? strongEl.GetString() ?? string.Empty : string.Empty,
                root.TryGetProperty("improvementPoint", out var improveEl) ? improveEl.GetString() ?? string.Empty : string.Empty,
                root.TryGetProperty("nextWeekFocus", out var focusEl) ? focusEl.GetString() ?? string.Empty : string.Empty));
        }
        catch (JsonException)
        {
            return Result<(string, string, string, string)>.Fail("WEEKLY_SUMMARY_JSON", "AI response is not valid JSON.");
        }
        catch (KeyNotFoundException)
        {
            return Result<(string, string, string, string)>.Fail("WEEKLY_SUMMARY_JSON", "Weekly summary JSON is missing required fields.");
        }
    }

    private static string? ExtractJsonObject(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            return null;
        }

        var start = aiResponse.IndexOf('{');
        var end = aiResponse.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return aiResponse[start..(end + 1)];
        }

        return null;
    }
}
