using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.Configuration;
using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Syllabus;

public sealed class SyllabusParsingService : ISyllabusParsingService
{
    private readonly IAiProvider _aiProvider;
    private readonly AiOptions _aiOptions;
    private readonly ILogger<SyllabusParsingService> _logger;
    private readonly Lazy<string> _promptTemplate;

    public SyllabusParsingService(
        IAiProvider aiProvider,
        IOptions<AiOptions> aiOptions,
        ILogger<SyllabusParsingService> logger)
    {
        _aiProvider = aiProvider;
        _aiOptions = aiOptions.Value;
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

        AiTextResponse generated;
        try
        {
            generated = await _aiProvider.GenerateTextAsync(
                    new AiTextRequest
                    {
                        UserPrompt = prompt,
                        PromptVersion = _aiOptions.PromptVersion,
                        Model = _aiOptions.Model,
                        Metadata = new Dictionary<string, string> { ["kind"] = "syllabus" },
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            AiRequestLogger.LogCompleted(_logger, generated, prompt.Length);
        }
        catch (AiProviderException ex)
        {
            AiRequestLogger.LogFailed(_logger, ex.Provider, ex.Code);
            return Result<IReadOnlyList<SyllabusTaskDraft>>.Fail(ex.Code, ex.Message);
        }

        var extractResult = AiJsonExtractor.ExtractJson(generated.Content);
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

        if (parseResult.Data is { Count: 0 })
        {
            var fallback = SyllabusContentTaskExtractor.ExtractContentTasks(syllabusText);
            if (fallback.Count > 0)
            {
                _logger.LogInformation(
                    "Syllabus AI returned no tasks; extracted {Count} tasks from course objective/content.",
                    fallback.Count);
                return Result<IReadOnlyList<SyllabusTaskDraft>>.Success(fallback);
            }
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
