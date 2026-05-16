using System.Reflection;
using Microsoft.Extensions.Logging;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Syllabus;

public sealed class SyllabusParsingService : ISyllabusParsingService
{
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

        var extractResult = AiJsonExtractor.ExtractJson(generated.Data);
        if (!extractResult.IsSuccess || extractResult.Data is null)
        {
            return Result<IReadOnlyList<SyllabusTaskDraft>>.Fail(
                extractResult.Error?.Code ?? "SYLLABUS_JSON_NOT_FOUND",
                extractResult.Error?.Message ?? "Could not extract JSON from AI response.");
        }

        var parseResult = AiJsonExtractor.ParseTaskArray(extractResult.Data);
        if (!parseResult.IsSuccess)
        {
            if (parseResult.Error?.Code == "SYLLABUS_JSON")
            {
                _logger.LogWarning("Syllabus JSON parse failed.");
            }

            return parseResult;
        }

        return parseResult;
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
}
