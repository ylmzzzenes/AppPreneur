using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class DashboardViewModel(IApiClient apiClient, IAuthTokenStore tokenStore, IUserSessionInfo userSession)
    : ObservableObject
{
    private CancellationTokenSource? _snackCts;

    public ObservableCollection<TaskItemResponseDto> Tasks { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? snackMessage;

    [ObservableProperty]
    private string userInitial = userSession.GetAvatarLetter();

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        Tasks.Clear();

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

            var result = await apiClient.GetTasksAsync(cancellationToken).ConfigureAwait(false);
            var ordered = result.Data is null
                ? null
                : result.Data.OrderByDescending(x => x.PriorityScore).ThenBy(x => x.DueDate).ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!result.IsSuccess || ordered is null)
                {
                    EnqueueSnack(result.Error?.Message ?? "Görevler yüklenemedi.");
                    return;
                }

                foreach (var t in ordered)
                    Tasks.Add(t);
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Yenileme veya sayfa kapanırken iptal — sessiz
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => EnqueueSnack(ex.Message)).ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false).ConfigureAwait(false);
        }
    }

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
