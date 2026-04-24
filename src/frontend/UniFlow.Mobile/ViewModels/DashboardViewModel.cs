using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class DashboardViewModel(IApiClient apiClient, IAuthTokenStore tokenStore) : ObservableObject
{
    public ObservableCollection<TaskItemResponseDto> Tasks { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? statusMessage;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        StatusMessage = null;
        Tasks.Clear();
        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                StatusMessage = "İnternet bağlantısı yok.";
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
                    StatusMessage = result.Error?.Message ?? "Görevler yüklenemedi.";
                    return;
                }

                foreach (var t in ordered)
                {
                    Tasks.Add(t);
                }
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                StatusMessage = ex.Message);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
        }
    }

    [RelayCommand]
    private async Task LogoutAsync(CancellationToken cancellationToken)
    {
        await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync($"//{Routes.Login}"));
    }
}
