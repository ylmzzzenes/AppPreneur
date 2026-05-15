using UniFlow.Business.Contracts.Syllabus;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface ISyllabusFileValidationService
{
    /// <summary>
    /// Validates upload metadata and reads the stream up to <see cref="SyllabusUploadConstants.MaxFileSizeBytes"/>.
    /// </summary>
    Task<Result<ValidatedSyllabusFile>> ValidateAndReadAsync(
        SyllabusUploadInput? upload,
        CancellationToken cancellationToken = default);
}
