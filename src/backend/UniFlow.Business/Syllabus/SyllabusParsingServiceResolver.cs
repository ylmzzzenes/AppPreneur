using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Configuration;
using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Syllabus;

/// <summary>
/// Uses Gemini parsing when configured; otherwise heuristic parsing in Development.
/// </summary>
public sealed class SyllabusParsingServiceResolver(
    IOptions<UniFlowGeminiOptions> geminiOptions,
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
        if (!string.IsNullOrWhiteSpace(geminiOptions.Value.ApiKey))
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
