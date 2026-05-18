using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Configuration;
using UniFlow.Business.Contracts.Syllabus;

namespace UniFlow.Business.Services;

public sealed class SyllabusTextStoragePolicy : ISyllabusTextStoragePolicy
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly SyllabusTextStorageOptions _options;

    public SyllabusTextStoragePolicy(IOptions<SyllabusTextStorageOptions> options)
    {
        _options = options.Value;
    }

    public string ComputeSourceTextHash(string? sourceText)
    {
        var normalized = Normalize(sourceText);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }

    public string BuildSourcePreview(string? sourceText)
    {
        var normalized = Normalize(sourceText);
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        return Truncate(normalized, _options.MaxStoredSourceTextLength);
    }

    public string? PrepareStoredSourceText(string? sourceText)
    {
        if (!_options.StoreRawSourceText)
        {
            return null;
        }

        var normalized = Normalize(sourceText);
        if (normalized.Length == 0)
        {
            return null;
        }

        return Truncate(normalized, _options.MaxStoredSourceTextLength);
    }

    public string SerializePreview(string sourceSummary, IReadOnlyList<SyllabusDetectedItemDto> detectedItems)
    {
        var working = new SyllabusScanPreviewPayload
        {
            SourceSummary = Truncate(sourceSummary, _options.MaxStoredSourceTextLength),
            DetectedItems = detectedItems
                .Select(item => new SyllabusDetectedItemDto
                {
                    Title = item.Title,
                    Description = item.Description,
                    DueDate = item.DueDate,
                    Type = item.Type,
                    PriorityScore = item.PriorityScore,
                })
                .ToList(),
        };

        var json = JsonSerializer.Serialize(working, JsonOptions);
        if (json.Length <= _options.MaxStoredPreviewJsonLength)
        {
            return json;
        }

        while (working.DetectedItems.Count > 0 && json.Length > _options.MaxStoredPreviewJsonLength)
        {
            working.DetectedItems.RemoveAt(working.DetectedItems.Count - 1);
            json = JsonSerializer.Serialize(working, JsonOptions);
        }

        if (json.Length > _options.MaxStoredPreviewJsonLength)
        {
            working.SourceSummary = Truncate(working.SourceSummary, Math.Min(500, _options.MaxStoredSourceTextLength));
            json = JsonSerializer.Serialize(working, JsonOptions);
        }

        if (json.Length > _options.MaxStoredPreviewJsonLength)
        {
            throw new InvalidOperationException(
                $"Preview payload exceeds maximum length of {_options.MaxStoredPreviewJsonLength} characters.");
        }

        return json;
    }

    private static string Normalize(string? sourceText) => sourceText?.Trim() ?? string.Empty;

    private static string Truncate(string value, int maxLength)
    {
        if (maxLength <= 0 || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
