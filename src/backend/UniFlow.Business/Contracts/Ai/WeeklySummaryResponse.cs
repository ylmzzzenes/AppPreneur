namespace UniFlow.Business.Contracts.Ai;

public sealed class WeeklySummaryResponse
{
    public string Summary { get; set; } = string.Empty;

    public int CompletedCount { get; set; }

    public int MissedCount { get; set; }

    public int PendingCount { get; set; }

    public string StrongPoint { get; set; } = string.Empty;

    public string ImprovementPoint { get; set; } = string.Empty;

    public string NextWeekFocus { get; set; } = string.Empty;

    public bool IsFallback { get; set; }
}
