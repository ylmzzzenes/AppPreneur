using System.Text.Json;
using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.ReadModels;
using UniFlow.Entity.Results;

namespace UniFlow.Business.AiProduct;

public static class StudyPlanJsonParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static Result<StudyPlanResponse> Parse(string aiContent)
    {
        var json = ExtractJsonObject(aiContent);
        if (json is null)
        {
            return Result<StudyPlanResponse>.Fail("STUDY_PLAN_JSON", "No JSON object found in AI response.");
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var title = root.GetProperty("title").GetString() ?? string.Empty;
            var summary = root.GetProperty("summary").GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title))
            {
                return Result<StudyPlanResponse>.Fail("STUDY_PLAN_JSON", "Study plan title is required.");
            }

            var days = new List<StudyPlanDayResponse>();
            if (root.TryGetProperty("days", out var daysElement) && daysElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var dayEl in daysElement.EnumerateArray())
                {
                    var day = new StudyPlanDayResponse
                    {
                        Date = dayEl.GetProperty("date").GetString() ?? string.Empty,
                        Focus = dayEl.TryGetProperty("focus", out var focusEl) ? focusEl.GetString() ?? string.Empty : string.Empty,
                        Tip = dayEl.TryGetProperty("tip", out var tipEl) ? tipEl.GetString() ?? string.Empty : string.Empty,
                    };

                    var tasks = new List<StudyPlanTaskResponse>();
                    if (dayEl.TryGetProperty("tasks", out var tasksEl) && tasksEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var taskEl in tasksEl.EnumerateArray())
                        {
                            tasks.Add(new StudyPlanTaskResponse
                            {
                                Title = taskEl.GetProperty("title").GetString() ?? string.Empty,
                                EstimatedMinutes = taskEl.TryGetProperty("estimatedMinutes", out var minEl) && minEl.TryGetInt32(out var mins) ? mins : 30,
                                Reason = taskEl.TryGetProperty("reason", out var reasonEl) ? reasonEl.GetString() ?? string.Empty : string.Empty,
                            });
                        }
                    }

                    day.Tasks = tasks;
                    days.Add(day);
                }
            }

            if (days.Count == 0)
            {
                return Result<StudyPlanResponse>.Fail("STUDY_PLAN_JSON", "Study plan must include at least one day.");
            }

            return Result<StudyPlanResponse>.Success(new StudyPlanResponse
            {
                Title = title.Trim(),
                Summary = summary.Trim(),
                Days = days,
            });
        }
        catch (JsonException)
        {
            return Result<StudyPlanResponse>.Fail("STUDY_PLAN_JSON", "AI response is not valid JSON.");
        }
        catch (KeyNotFoundException)
        {
            return Result<StudyPlanResponse>.Fail("STUDY_PLAN_JSON", "Study plan JSON is missing required fields.");
        }
    }

    private static string? ExtractJsonObject(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            return null;
        }

        var unfenced = Syllabus.AiJsonExtractor.ExtractJson(aiResponse);
        if (unfenced.IsSuccess && unfenced.Data is not null)
        {
            var trimmed = unfenced.Data.TrimStart();
            if (trimmed.StartsWith('{'))
            {
                return unfenced.Data;
            }
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
