using System.Text.Json;
using UniFlow.Business.Contracts.Syllabus;

namespace UniFlow.Business.Helpers;

internal static class SyllabusPreviewBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string BuildPreviewJson(string sourceSummary, IReadOnlyList<SyllabusDetectedItemDto> detectedItems)
    {
        var payload = new SyllabusScanPreviewPayload
        {
            SourceSummary = sourceSummary,
            DetectedItems = detectedItems.ToList(),
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    public static string ShrinkPreviewJsonToFit(
        string previewJson,
        int maxPreviewJsonLength,
        int maxSourceSummaryLength)
    {
        if (previewJson.Length <= maxPreviewJsonLength)
        {
            return previewJson;
        }

        var payload = SyllabusScanHelper.DeserializePreview(previewJson);

        while (payload.DetectedItems.Count > 0)
        {
            previewJson = JsonSerializer.Serialize(payload, JsonOptions);
            if (previewJson.Length <= maxPreviewJsonLength)
            {
                return previewJson;
            }

            payload.DetectedItems.RemoveAt(payload.DetectedItems.Count - 1);
        }

        if (payload.SourceSummary.Length > Math.Min(500, maxSourceSummaryLength))
        {
            payload.SourceSummary = payload.SourceSummary[..Math.Min(500, maxSourceSummaryLength)];
            previewJson = JsonSerializer.Serialize(payload, JsonOptions);
        }

        if (previewJson.Length > maxPreviewJsonLength)
        {
            throw new InvalidOperationException(
                $"Preview payload exceeds maximum length of {maxPreviewJsonLength} characters.");
        }

        return previewJson;
    }
}
