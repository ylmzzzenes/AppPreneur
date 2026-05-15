namespace UniFlow.Business.Contracts.Syllabus;

/// <summary>
/// Serialized into <see cref="Entity.Entities.SyllabusScanSession.PreviewJson"/>.
/// </summary>
internal sealed class SyllabusScanPreviewPayload
{
    public string SourceSummary { get; set; } = string.Empty;

    public List<SyllabusDetectedItemDto> DetectedItems { get; set; } = [];
}
