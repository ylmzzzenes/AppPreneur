using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Services;

internal static class TaskFeedbackHelper
{
    internal static async Task TryShowFeedbackAsync(
        IApiClient apiClient,
        long taskId,
        TaskItemStatus newStatus,
        CancellationToken cancellationToken)
    {
        try
        {
            var feedback = await apiClient.GenerateTaskFeedbackAsync(
                new TaskFeedbackRequestDto { TaskId = taskId, NewStatus = newStatus },
                cancellationToken).ConfigureAwait(false);

            if (!feedback.IsSuccess || feedback.Data is null)
            {
                return;
            }

            var message = $"{feedback.Data.Message}\n\nSonraki adım: {feedback.Data.NextAction}";
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current?.CurrentPage is Page page)
                {
                    await page.DisplayAlert("AI Geri Bildirim", message, "Tamam").ConfigureAwait(true);
                }
            }).ConfigureAwait(false);
        }
        catch
        {
            // Feedback failure must not break status update flow.
        }
    }
}
