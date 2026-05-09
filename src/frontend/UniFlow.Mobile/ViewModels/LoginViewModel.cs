using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class LoginViewModel(IApiClient apiClient, IAuthTokenStore tokenStore, IUserSessionInfo userSession) : ObservableObject
{
    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool obscurePassword = true;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private void TogglePasswordVisibility() => ObscurePassword = !ObscurePassword;

    [RelayCommand]
    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "E-posta ve şifre gerekli.";
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

            var result = await apiClient.LoginAsync(
                    new LoginRequestDto { Email = Email.Trim(), Password = Password },
                    cancellationToken)
                .ConfigureAwait(false);

            if (!result.IsSuccess || result.Data is null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    StatusMessage = result.Error?.Message ?? "Giriş başarısız.");
                return;
            }

            await tokenStore.SetTokenAsync(result.Data.AccessToken, cancellationToken).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
                    userSession.SetDisplayName(result.Data.DisplayName))
                .ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.GoToAsync($"//{Routes.MainTabs}/{Routes.Dashboard}"));
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }
    }

    [RelayCommand]
    private Task OpenRegisterAsync() =>
        Shell.Current.GoToAsync(Routes.Register);
}
