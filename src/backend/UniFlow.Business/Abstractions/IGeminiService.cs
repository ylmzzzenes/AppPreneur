using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface IGeminiService
{
    Task<Result<string>> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
}
