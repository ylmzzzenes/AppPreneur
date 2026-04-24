using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class RegisterViewModel(IApiClient apiClient, IAuthTokenStore tokenStore) : ObservableObject
{
    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task RegisterAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Tüm alanları doldurun.";
            return;
        }

        if (Password.Length < 8)
        {
            StatusMessage = "Şifre en az 8 karakter olmalı.";
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                StatusMessage = "İnternet bağlantısı yok.";
                return;
            }

            var result = await apiClient.RegisterAsync(
                    new RegisterRequestDto
                    {
                        DisplayName = DisplayName.Trim(),
                        Email = Email.Trim(),
                        Password = Password,
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            if (!result.IsSuccess || result.Data is null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    StatusMessage = result.Error?.Message ?? "Kayıt başarısız.");
                return;
            }

            await tokenStore.SetTokenAsync(result.Data.AccessToken, cancellationToken).ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.GoToAsync($"//{Routes.MainTabs}/{Routes.Dashboard}"));
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
