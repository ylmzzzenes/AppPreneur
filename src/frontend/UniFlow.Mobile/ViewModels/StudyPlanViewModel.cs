using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class StudyPlanViewModel(IApiClient apiClient, IAuthTokenStore tokenStore) : ObservableObject
{
    public ObservableCollection<CourseResponseDto> Courses { get; } = new();
    public ObservableCollection<StudyPlanDayResponseDto> PlanDays { get; } = new();

    public int[] DayOptions { get; } = [3, 7, 14];

    [ObservableProperty]
    private int selectedDays = 7;

    [ObservableProperty]
    private CourseResponseDto? selectedCourse;

    [ObservableProperty]
    private string focusText = string.Empty;

    [ObservableProperty]
    private string planTitle = string.Empty;

    [ObservableProperty]
    private string planSummary = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    private bool isLoading;

    [ObservableProperty]
    private bool hasPlan;

    [ObservableProperty]
    private string? errorMessage;

    private bool CanGenerate => !IsLoading;

    [RelayCommand]
    private async Task LoadCoursesAsync(CancellationToken cancellationToken)
    {
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            ErrorMessage = "İnternet bağlantısı yok.";
            return;
        }

        var result = await apiClient.GetCoursesAsync(cancellationToken).ConfigureAwait(false);

        if (await AuthSessionNavigator.HandleUnauthorizedIfNeededAsync(
                result.Error?.Code, tokenStore, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Courses.Clear();
            if (result.IsSuccess && result.Data is not null)
            {
                foreach (var course in result.Data)
                {
                    Courses.Add(course);
                }
            }
            else if (!result.IsSuccess)
            {
                ErrorMessage = result.Error?.Message ?? "Dersler yüklenemedi.";
            }
        }).ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;
        HasPlan = false;
        PlanDays.Clear();

        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                ErrorMessage = "İnternet bağlantısı yok.";
                return;
            }

            var request = new StudyPlanRequestDto
            {
                Days = SelectedDays,
                Focus = string.IsNullOrWhiteSpace(FocusText) ? null : FocusText.Trim(),
                CourseId = SelectedCourse?.Id,
            };

            var result = await apiClient.GenerateStudyPlanAsync(request, cancellationToken).ConfigureAwait(false);

            if (await AuthSessionNavigator.HandleUnauthorizedIfNeededAsync(
                    result.Error?.Code, tokenStore, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!result.IsSuccess || result.Data is null)
                {
                    ErrorMessage = result.Error?.Message ?? "Plan oluşturulamadı.";
                    return;
                }

                PlanTitle = result.Data.Title;
                PlanSummary = result.Data.Summary;
                foreach (var day in result.Data.Days)
                {
                    PlanDays.Add(day);
                }

                HasPlan = PlanDays.Count > 0;
            }).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                ErrorMessage = "Plan oluşturulamadı.").ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false).ConfigureAwait(false);
        }
    }
}
