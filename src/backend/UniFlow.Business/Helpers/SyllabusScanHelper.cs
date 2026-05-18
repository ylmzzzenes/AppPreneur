using System.Text.Json;
using UniFlow.Business.Contracts.Syllabus;
using UniFlow.Business.Dtos;

namespace UniFlow.Business.Helpers;

internal static class SyllabusScanHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static SyllabusScanPreviewPayload DeserializePreview(string previewJson) =>
        JsonSerializer.Deserialize<SyllabusScanPreviewPayload>(previewJson, JsonOptions)
        ?? new SyllabusScanPreviewPayload();

    public static SyllabusDetectedItemDto ToDetectedItem(SyllabusTaskDraft draft) => new()
    {
        Title = draft.Title,
        Description = draft.Description,
        DueDate = draft.DueDate,
        Type = draft.Category,
        PriorityScore = draft.PriorityScore,
    };

    public static SyllabusTaskDraft ToDraft(SyllabusDetectedItemDto item) => new()
    {
        Title = item.Title.Trim(),
        Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
        DueDate = item.DueDate,
        Category = string.IsNullOrWhiteSpace(item.Type) ? null : item.Type.Trim(),
        PriorityScore = item.PriorityScore,
    };
}
