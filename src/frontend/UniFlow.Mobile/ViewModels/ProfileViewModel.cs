using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class ProfileViewModel(
    IApiClient apiClient,
    IAuthTokenStore tokenStore,
    IUserSessionInfo userSession) : ObservableObject
{
    public ObservableCollection<PersonalityVibeOption> PersonalityOptions { get; } =
    [
        new("Samimi", PersonalityVibeDto.Friendly),
        new("Disiplinli", PersonalityVibeDto.Strict),
        new("Cesaretlendirici", PersonalityVibeDto.Motivational),
        new("Dobra", PersonalityVibeDto.Sarcastic),
    ];

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string major = string.Empty;

    [ObservableProperty]
    private string academicGoal = string.Empty;

    [ObservableProperty]
    private string dailyStudyTargetMinutes = string.Empty;

    [ObservableProperty]
    private PersonalityVibeOption selectedPersonality = new("Samimi", PersonalityVibeDto.Friendly);

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        StatusMessage = null;
        try
        {
            var result = await apiClient.GetMyProfileAsync(cancellationToken).ConfigureAwait(false);

            if (result.Error?.Code == "UNAUTHORIZED")
            {
                await HandleUnauthorizedAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            if (!result.IsSuccess || result.Data is null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    StatusMessage = result.Error?.Message ?? "Profil yüklenemedi.").ConfigureAwait(false);
                return;
            }

            var profile = result.Data;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Email = profile.Email;
                DisplayName = profile.DisplayName;
                Major = profile.Major ?? string.Empty;
                AcademicGoal = profile.AcademicGoal ?? string.Empty;
                DailyStudyTargetMinutes = profile.DailyStudyTargetMinutes?.ToString() ?? string.Empty;
                SelectedPersonality = PersonalityOptions.FirstOrDefault(o => o.Value == profile.PersonalityVibe)
                    ?? PersonalityOptions[0];
            }).ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
        }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            StatusMessage = "Görünen ad gerekli.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(DailyStudyTargetMinutes)
            && (!int.TryParse(DailyStudyTargetMinutes.Trim(), out var minutes) || minutes is < 0 or > 720))
        {
            StatusMessage = "Günlük hedef 0–720 dakika arasında olmalı.";
            return;
        }

        int? targetMinutes = string.IsNullOrWhiteSpace(DailyStudyTargetMinutes)
            ? null
            : int.Parse(DailyStudyTargetMinutes.Trim());

        IsBusy = true;
        StatusMessage = null;
        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                StatusMessage = "İnternet bağlantısı yok.";
                return;
            }

            var result = await apiClient.UpdateOnboardingAsync(
                    new UpdateOnboardingRequestDto
                    {
                        DisplayName = DisplayName.Trim(),
                        Major = string.IsNullOrWhiteSpace(Major) ? null : Major.Trim(),
                        AcademicGoal = string.IsNullOrWhiteSpace(AcademicGoal) ? null : AcademicGoal.Trim(),
                        PersonalityVibe = SelectedPersonality.Value,
                        DailyStudyTargetMinutes = targetMinutes,
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            if (result.Error?.Code == "UNAUTHORIZED")
            {
                await HandleUnauthorizedAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            if (!result.IsSuccess || result.Data is null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    StatusMessage = result.Error?.Message ?? "Profil güncellenemedi.").ConfigureAwait(false);
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                userSession.SetDisplayName(result.Data.DisplayName);
                StatusMessage = "Profil güncellendi.";
            }).ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => Shell.Current.GoToAsync("..");

    private async Task HandleUnauthorizedAsync(CancellationToken cancellationToken)
    {
        await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
        userSession.Clear();
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync($"//{Routes.Login}")).ConfigureAwait(false);
    }
}
