using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UniFlow.Business.Contracts.Syllabus;

namespace UniFlow.Business.Helpers;

internal static class SyllabusScanHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string ComputeSourceTextHash(string sourceText)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sourceText));
        return Convert.ToHexString(bytes);
    }

    public static string BuildSourceSummary(string sourceText)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            return string.Empty;
        }

        var trimmed = sourceText.Trim();
        if (trimmed.Length <= SyllabusScanConstants.MaxSourceSummaryLength)
        {
            return trimmed;
        }

        return trimmed[..SyllabusScanConstants.MaxSourceSummaryLength];
    }

    public static string SerializePreview(SyllabusScanPreviewPayload payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        if (json.Length > SyllabusScanConstants.MaxPreviewJsonLength)
        {
            throw new InvalidOperationException(
                $"Preview payload exceeds maximum length of {SyllabusScanConstants.MaxPreviewJsonLength} characters.");
        }

        return json;
    }

    public static SyllabusScanPreviewPayload DeserializePreview(string previewJson) =>
        JsonSerializer.Deserialize<SyllabusScanPreviewPayload>(previewJson, JsonOptions)
        ?? new SyllabusScanPreviewPayload();

    public static SyllabusDetectedItemDto ToDetectedItem(Dtos.SyllabusTaskDraft draft) => new()
    {
        Title = draft.Title,
        Description = draft.Description,
        DueDate = draft.DueDate,
        Type = draft.Category,
        PriorityScore = draft.PriorityScore,
    };

    public static Dtos.SyllabusTaskDraft ToDraft(SyllabusDetectedItemDto item) => new()
    {
        Title = item.Title.Trim(),
        Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
        DueDate = item.DueDate,
        Category = string.IsNullOrWhiteSpace(item.Type) ? null : item.Type.Trim(),
        PriorityScore = item.PriorityScore,
    };
}
