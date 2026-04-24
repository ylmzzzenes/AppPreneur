using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface ISyllabusParsingService
{
    Task<Result<IReadOnlyList<SyllabusTaskDraft>>> ParseTasksFromSyllabusTextAsync(
        string syllabusText,
        CancellationToken cancellationToken = default);
}
