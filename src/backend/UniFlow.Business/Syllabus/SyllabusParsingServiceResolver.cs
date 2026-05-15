using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Configuration;
using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Syllabus;

/// <summary>
/// Uses Gemini parsing when configured; otherwise heuristic parsing in Development.
/// </summary>
public sealed class SyllabusParsingServiceResolver(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    SyllabusParsingService geminiParsing,
    HeuristicSyllabusParsingService heuristicParsing) : ISyllabusParsingService
{
    public Task<Result<IReadOnlyList<SyllabusTaskDraft>>> ParseTasksFromSyllabusTextAsync(
        string syllabusText,
        CancellationToken cancellationToken = default)
    {
        return Resolve().ParseTasksFromSyllabusTextAsync(syllabusText, cancellationToken);
    }

    private ISyllabusParsingService Resolve()
    {
        var apiKey = configuration[$"{UniFlowGeminiOptions.SectionName}:ApiKey"]
            ?? configuration["GEMINI_API_KEY"]
            ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return geminiParsing;
        }

        if (hostEnvironment.IsDevelopment())
        {
            return heuristicParsing;
        }

        return geminiParsing;
    }
}
