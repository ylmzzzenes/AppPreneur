using UniFlow.Business.Contracts.Syllabus;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface ISyllabusIngestionService
{
    Task<Result<SyllabusIngestionResult>> IngestAsync(
        long userId,
        string courseCode,
        string courseTitle,
        SyllabusUploadInput? upload,
        CancellationToken cancellationToken = default);
}
