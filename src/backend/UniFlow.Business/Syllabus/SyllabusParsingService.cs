using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Syllabus;

public sealed class SyllabusParsingService : ISyllabusParsingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IGeminiService _gemini;
    private readonly ILogger<SyllabusParsingService> _logger;
    private readonly Lazy<string> _promptTemplate;

    public SyllabusParsingService(IGeminiService gemini, ILogger<SyllabusParsingService> logger)
    {
        _gemini = gemini;
        _logger = logger;
        _promptTemplate = new Lazy<string>(LoadPromptTemplate, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<Result<IReadOnlyList<SyllabusTaskDraft>>> ParseTasksFromSyllabusTextAsync(
        string syllabusText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(syllabusText))
        {
            return Result<IReadOnlyList<SyllabusTaskDraft>>.Fail("SYLLABUS_EMPTY", "Syllabus text is empty.");
        }

        var prompt = _promptTemplate.Value.Replace("{{SYLLABUS_TEXT}}", syllabusText.Trim(), StringComparison.Ordinal);
        var generated = await _gemini.GenerateTextAsync(prompt, cancellationToken);
        if (!generated.IsSuccess || generated.Data is null)
        {
            return Result<IReadOnlyList<SyllabusTaskDraft>>.Fail(
                generated.Error?.Code ?? "GEMINI_FAILED",
                generated.Error?.Message ?? "Gemini generation failed.");
        }

        var json = StripCodeFences(generated.Data);
        try
        {
            var rows = JsonSerializer.Deserialize<List<SyllabusTaskDraftDto>>(json, JsonOptions);
            if (rows is null || rows.Count == 0)
            {
                return Result<IReadOnlyList<SyllabusTaskDraft>>.Success(Array.Empty<SyllabusTaskDraft>());
            }

            var list = new List<SyllabusTaskDraft>(rows.Count);
            foreach (var row in rows)
            {
                list.Add(new SyllabusTaskDraft
                {
                    Title = row.Title ?? string.Empty,
                    Description = row.Description,
                    DueDate = row.DueDate,
                    Category = row.Category,
                });
            }

            return Result<IReadOnlyList<SyllabusTaskDraft>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Syllabus JSON parse failed. Payload: {Payload}", json);
            return Result<IReadOnlyList<SyllabusTaskDraft>>.Fail("SYLLABUS_JSON", $"Invalid JSON from model: {ex.Message}");
        }
    }

    private static string LoadPromptTemplate()
    {
        var assembly = typeof(SyllabusParsingService).Assembly;
        var name = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("syllabus-extract.md", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Embedded resource syllabus-extract.md was not found.");

        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Could not open embedded resource {name}.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string StripCodeFences(string text)
    {
        var t = text.Trim();
        if (t.StartsWith("```", StringComparison.Ordinal))
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
        }

        return t.Trim();
    }

    private sealed class SyllabusTaskDraftDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Category { get; set; }
    }
}
