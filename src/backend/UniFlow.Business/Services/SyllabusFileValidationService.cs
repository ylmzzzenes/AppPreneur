using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Syllabus;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class SyllabusFileValidationService : ISyllabusFileValidationService
{
    private const int ReadBufferSize = 81_920;

    public async Task<Result<ValidatedSyllabusFile>> ValidateAndReadAsync(
        SyllabusUploadInput? upload,
        CancellationToken cancellationToken = default)
    {
        if (upload?.Content is null)
        {
            return Result<ValidatedSyllabusFile>.Fail(
                "SYLLABUS_FILE_REQUIRED",
                "Syllabus file is required.");
        }

        if (upload.DeclaredLength is 0)
        {
            return Result<ValidatedSyllabusFile>.Fail(
                "SYLLABUS_FILE_EMPTY",
                "Uploaded file is empty.");
        }

        if (upload.DeclaredLength > SyllabusUploadConstants.MaxFileSizeBytes)
        {
            return Result<ValidatedSyllabusFile>.Fail(
                "SYLLABUS_FILE_TOO_LARGE",
                $"File size must not exceed {SyllabusUploadConstants.MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        var extension = Path.GetExtension(upload.FileName);
        if (string.IsNullOrEmpty(extension)
            || !SyllabusUploadConstants.AllowedExtensions.Contains(extension))
        {
            return Result<ValidatedSyllabusFile>.Fail(
                "SYLLABUS_FILE_EXTENSION_NOT_SUPPORTED",
                "File extension is not supported. Allowed: .pdf, .jpg, .jpeg, .png.");
        }

        var contentType = NormalizeContentType(upload.ContentType);
        if (contentType is null || !SyllabusUploadConstants.AllowedContentTypes.Contains(contentType))
        {
            return Result<ValidatedSyllabusFile>.Fail(
                "SYLLABUS_FILE_TYPE_NOT_SUPPORTED",
                "Content-Type is not supported. Allowed: application/pdf, image/jpeg, image/png.");
        }

        var readResult = await ReadBoundedAsync(upload.Content, cancellationToken).ConfigureAwait(false);
        if (!readResult.IsSuccess || readResult.Data is null)
        {
            return Result<ValidatedSyllabusFile>.Fail(
                readResult.Error?.Code ?? "SYLLABUS_FILE_EMPTY",
                readResult.Error?.Message ?? "Could not read uploaded file.");
        }

        return Result<ValidatedSyllabusFile>.Success(new ValidatedSyllabusFile
        {
            Content = readResult.Data,
            ContentType = contentType,
            FileName = upload.FileName,
        });
    }

    private static async Task<Result<byte[]>> ReadBoundedAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek && stream.Length > SyllabusUploadConstants.MaxFileSizeBytes)
        {
            return Result<byte[]>.Fail(
                "SYLLABUS_FILE_TOO_LARGE",
                $"File size must not exceed {SyllabusUploadConstants.MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        using var buffer = new MemoryStream();
        var chunk = new byte[ReadBufferSize];
        long totalRead = 0;

        while (true)
        {
            var read = await stream.ReadAsync(chunk.AsMemory(0, ReadBufferSize), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
            if (totalRead > SyllabusUploadConstants.MaxFileSizeBytes)
            {
                return Result<byte[]>.Fail(
                    "SYLLABUS_FILE_TOO_LARGE",
                    $"File size must not exceed {SyllabusUploadConstants.MaxFileSizeBytes / (1024 * 1024)} MB.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        if (totalRead == 0)
        {
            return Result<byte[]>.Fail("SYLLABUS_FILE_EMPTY", "Uploaded file is empty.");
        }

        return Result<byte[]>.Success(buffer.ToArray());
    }

    private static string? NormalizeContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        var semicolon = contentType.IndexOf(';');
        return (semicolon >= 0 ? contentType[..semicolon] : contentType).Trim();
    }
}
