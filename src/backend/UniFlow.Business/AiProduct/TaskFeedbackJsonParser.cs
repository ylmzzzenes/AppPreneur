using System.Text.Json;
using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Results;

namespace UniFlow.Business.AiProduct;

internal static class TaskFeedbackJsonParser
{
    internal static Result<TaskFeedbackResponse> Parse(string aiContent)
    {
        var json = ExtractJsonObject(aiContent);
        if (json is null)
        {
            return Result<TaskFeedbackResponse>.Fail("TASK_FEEDBACK_JSON", "No JSON object found in AI response.");
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var message = root.GetProperty("message").GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                return Result<TaskFeedbackResponse>.Fail("TASK_FEEDBACK_JSON", "Feedback message is required.");
            }

            return Result<TaskFeedbackResponse>.Success(new TaskFeedbackResponse
            {
                Message = message.Trim(),
                Tone = root.TryGetProperty("tone", out var toneEl) ? toneEl.GetString() ?? string.Empty : string.Empty,
                NextAction = root.TryGetProperty("nextAction", out var nextEl) ? nextEl.GetString() ?? string.Empty : string.Empty,
            });
        }
        catch (JsonException)
        {
            return Result<TaskFeedbackResponse>.Fail("TASK_FEEDBACK_JSON", "AI response is not valid JSON.");
        }
        catch (KeyNotFoundException)
        {
            return Result<TaskFeedbackResponse>.Fail("TASK_FEEDBACK_JSON", "Feedback JSON is missing required fields.");
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
