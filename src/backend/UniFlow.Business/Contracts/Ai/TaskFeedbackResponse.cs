namespace UniFlow.Business.Contracts.Ai;

public sealed class TaskFeedbackResponse
{
    public string Message { get; set; } = string.Empty;

    public string Tone { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public bool IsFallback { get; set; }
}
