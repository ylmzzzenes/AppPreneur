using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class OnboardingViewModel(
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
    private string displayName = string.Empty;

    [ObservableProperty]
    private string major = string.Empty;

    [ObservableProperty]
    private string academicGoal = string.Empty;

    [ObservableProperty]
    private string dailyStudyTargetMinutes = "120";

    [ObservableProperty]
    private PersonalityVibeOption selectedPersonality = new("Samimi", PersonalityVibeDto.Friendly);

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isBusy;

    private bool CanSubmit => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            StatusMessage = "Görünen ad gerekli.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Major))
        {
            StatusMessage = "Bölüm gerekli.";
            return;
        }

        if (!int.TryParse(DailyStudyTargetMinutes.Trim(), out var minutes) || minutes is < 0 or > 720)
        {
            StatusMessage = "Günlük hedef 0–720 dakika arasında olmalı.";
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

            var result = await apiClient.UpdateOnboardingAsync(
                    new UpdateOnboardingRequestDto
                    {
                        DisplayName = DisplayName.Trim(),
                        Major = Major.Trim(),
                        AcademicGoal = string.IsNullOrWhiteSpace(AcademicGoal) ? null : AcademicGoal.Trim(),
                        PersonalityVibe = SelectedPersonality.Value,
                        DailyStudyTargetMinutes = minutes,
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            if (result.Error?.Code == "UNAUTHORIZED")
            {
                await AuthSessionNavigator.HandleUnauthorizedIfNeededAsync(
                    result.Error.Code, tokenStore, userSession, cancellationToken).ConfigureAwait(false);
                return;
            }

            if (!result.IsSuccess || result.Data is null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    StatusMessage = result.Error?.Message ?? "Profil kaydedilemedi.").ConfigureAwait(false);
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
                    userSession.SetDisplayName(result.Data.DisplayName))
                .ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.GoToAsync($"//{Routes.MainTabs}/{Routes.Dashboard}")).ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }
    }
}

public sealed record PersonalityVibeOption(string Label, PersonalityVibeDto Value)
{
    public override string ToString() => Label;
}
