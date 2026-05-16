using System.Text.Json;
using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Syllabus;

/// <summary>
/// Extracts and parses JSON task arrays from AI model responses (markdown fences, preamble, etc.).
/// </summary>
public static class AiJsonExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Extracts a JSON array payload from raw AI text without calling external APIs.
    /// </summary>
    public static Result<string> ExtractJson(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            return Result<string>.Fail("SYLLABUS_AI_EMPTY", "AI response is empty.");
        }

        var unfenced = StripMarkdownCodeFences(aiResponse);
        var json = ExtractJsonArray(unfenced);
        if (json is null)
        {
            return Result<string>.Fail(
                "SYLLABUS_JSON_NOT_FOUND",
                "No JSON array found in AI response.");
        }

        return Result<string>.Success(json);
    }

    /// <summary>
    /// Deserializes a JSON array into syllabus task drafts and validates required fields.
    /// </summary>
    public static Result<IReadOnlyList<SyllabusTaskDraft>> ParseTaskArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Result<IReadOnlyList<SyllabusTaskDraft>>.Fail("SYLLABUS_AI_EMPTY", "JSON payload is empty.");
        }

        List<SyllabusTaskDraftDto> rows;
        try
        {
            rows = JsonSerializer.Deserialize<List<SyllabusTaskDraftDto>>(json, JsonOptions)
                ?? [];
        }
        catch (JsonException)
        {
            return Result<IReadOnlyList<SyllabusTaskDraft>>.Fail(
                "SYLLABUS_JSON",
                "AI response is not valid JSON.");
        }

        if (rows.Count == 0)
        {
            return Result<IReadOnlyList<SyllabusTaskDraft>>.Success(Array.Empty<SyllabusTaskDraft>());
        }

        var list = new List<SyllabusTaskDraft>(rows.Count);
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Title))
            {
                return Result<IReadOnlyList<SyllabusTaskDraft>>.Fail(
                    "SYLLABUS_JSON_VALIDATION",
                    "Each task item must include a non-empty title.");
            }

            list.Add(new SyllabusTaskDraft
            {
                Title = row.Title.Trim(),
                Description = row.Description,
                DueDate = row.DueDate,
                Category = row.Category,
            });
        }

        return Result<IReadOnlyList<SyllabusTaskDraft>>.Success(list);
    }

    /// <summary>
    /// Strips markdown code fences and returns the inner content when present.
    /// </summary>
    public static string StripMarkdownCodeFences(string text)
    {
        var t = text.Trim();

        if (t.StartsWith("```", StringComparison.Ordinal))
        {
            return StripLeadingFenceBlock(t);
        }

        var fenceStart = t.IndexOf("```", StringComparison.Ordinal);
        if (fenceStart < 0)
        {
            return t;
        }

        var contentStart = fenceStart + 3;
        var firstNewline = t.IndexOf('\n', contentStart);
        if (firstNewline >= contentStart)
        {
            contentStart = firstNewline + 1;
        }

        var fenceEnd = t.IndexOf("```", contentStart, StringComparison.Ordinal);
        if (fenceEnd > contentStart)
        {
            return t[contentStart..fenceEnd].Trim();
        }

        return t;
    }

    /// <summary>
    /// Locates the first balanced JSON array in the text.
    /// </summary>
    public static string? ExtractJsonArray(string text)
    {
        var start = text.IndexOf('[');
        if (start < 0)
        {
            return null;
        }

        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == '[')
            {
                depth++;
            }
            else if (c == ']')
            {
                depth--;
                if (depth == 0)
                {
                    return text[start..(i + 1)];
                }
            }
        }

        return null;
    }

    private static string StripLeadingFenceBlock(string t)
    {
        var firstNl = t.IndexOf('\n');
        if (firstNl > 0)
        {
            t = t[(firstNl + 1)..];
        }

        var end = t.LastIndexOf("```", StringComparison.Ordinal);
        if (end > 0)
        {
            t = t[..end];
        }

        return t.Trim();
    }

    private sealed class SyllabusTaskDraftDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Category { get; set; }

        public string? ExtraField { get; set; }
    }
}
