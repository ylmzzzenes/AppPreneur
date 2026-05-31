using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.Configuration;
using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Syllabus;

/// <summary>
/// Uses AI parsing when configured; otherwise heuristic parsing when fallback is enabled.
/// </summary>
public sealed class SyllabusParsingServiceResolver(
    IOptions<AiOptions> aiOptions,
    IHostEnvironment hostEnvironment,
    SyllabusParsingService aiParsing,
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
        var ai = aiOptions.Value;

        if (string.Equals(ai.Provider, AiProviders.Fake, StringComparison.OrdinalIgnoreCase))
        {
            return heuristicParsing;
        }

        if (string.IsNullOrWhiteSpace(ai.ApiKey) && ai.EnableFallback)
        {
            if (hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing"))
            {
                return heuristicParsing;
            }
        }

        return aiParsing;
    }
}
