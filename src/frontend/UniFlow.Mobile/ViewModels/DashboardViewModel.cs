using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class DashboardViewModel(
    IApiClient apiClient,
    IAuthTokenStore tokenStore,
    IUserSessionInfo userSession) : ObservableObject
{
    private CancellationTokenSource? _snackCts;

    public ObservableCollection<DashboardTaskItemDto> BigThreeTasks { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? snackMessage;

    [ObservableProperty]
    private string userInitial = userSession.GetAvatarLetter();

    [ObservableProperty]
    private string dailyMessage = string.Empty;

    [ObservableProperty]
    private string aiMood = string.Empty;

    [ObservableProperty]
    private int pendingTodayCount;

    [ObservableProperty]
    private int completedTodayCount;

    [ObservableProperty]
    private int overdueTasksCount;

    [ObservableProperty]
    private bool hasDashboardData;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        BigThreeTasks.Clear();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            UserInitial = userSession.GetAvatarLetter();
            _snackCts?.Cancel();
            SnackMessage = null;
        });

        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                EnqueueSnack("İnternet bağlantısı yok.");
                return;
            }

            var result = await apiClient.GetDashboardTodayAsync(cancellationToken).ConfigureAwait(false);

            if (result.Error?.Code == "UNAUTHORIZED")
            {
                await HandleUnauthorizedAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!result.IsSuccess || result.Data is null)
                {
                    EnqueueSnack(result.Error?.Message ?? "Dashboard yüklenemedi.");
                    HasDashboardData = false;
                    return;
                }

                var data = result.Data;
                DailyMessage = data.DailyMessage;
                AiMood = data.AiMood;
                PendingTodayCount = data.PendingTodayCount;
                CompletedTodayCount = data.CompletedTodayCount;
                OverdueTasksCount = data.OverdueTasksCount;
                HasDashboardData = true;

                foreach (var task in data.BigThreeTasks)
                {
                    BigThreeTasks.Add(task);
                }
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            await MainThread.InvokeOnMainThreadAsync(() => EnqueueSnack("Dashboard yüklenemedi.")).ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private async Task MarkTaskDoneAsync(DashboardTaskItemDto task, CancellationToken cancellationToken)
    {
        await UpdateTaskStatusAsync(task, TaskItemStatus.Done, cancellationToken).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task MarkTaskPendingAsync(DashboardTaskItemDto task, CancellationToken cancellationToken)
    {
        await UpdateTaskStatusAsync(task, TaskItemStatus.Pending, cancellationToken).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task GoToProfileAsync() =>
        await Shell.Current.GoToAsync(Routes.Profile).ConfigureAwait(false);

    [RelayCommand]
    private async Task GoToSyllabusAsync() =>
        await Shell.Current.GoToAsync($"//{Routes.MainTabs}/{Routes.Syllabus}").ConfigureAwait(false);

    [RelayCommand]
    private async Task LogoutAsync(CancellationToken cancellationToken)
    {
        await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
        userSession.Clear();
        await MainThread.InvokeOnMainThreadAsync(() => UserInitial = "?").ConfigureAwait(false);
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync($"//{Routes.Login}")).ConfigureAwait(false);
    }

    private async Task UpdateTaskStatusAsync(
        DashboardTaskItemDto task,
        TaskItemStatus newStatus,
        CancellationToken cancellationToken)
    {
        var previousStatus = task.Status;
        task.Status = newStatus;

        var result = await apiClient.UpdateTaskStatusAsync(task.Id, newStatus, cancellationToken).ConfigureAwait(false);

        if (result.Error?.Code == "UNAUTHORIZED")
        {
            task.Status = previousStatus;
            await HandleUnauthorizedAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!result.IsSuccess)
        {
            task.Status = previousStatus;
            EnqueueSnack(result.Error?.Message ?? "Görev durumu güncellenemedi.");
            return;
        }

        if (LoadCommand.CanExecute(null))
        {
            LoadCommand.Execute(null);
        }
    }

    private async Task HandleUnauthorizedAsync(CancellationToken cancellationToken)
    {
        await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
        userSession.Clear();
        EnqueueSnack("Oturum süresi doldu. Lütfen tekrar giriş yapın.");
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync($"//{Routes.Login}")).ConfigureAwait(false);
    }

    private void EnqueueSnack(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _snackCts?.Cancel();
            _snackCts = new CancellationTokenSource();
            var token = _snackCts.Token;
            SnackMessage = message;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3200, token).ConfigureAwait(false);
                    await MainThread.InvokeOnMainThreadAsync(() => SnackMessage = null).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }, token);
        });
    }
}
