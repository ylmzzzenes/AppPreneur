using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class CoursesViewModel(
    IApiClient apiClient,
    IAuthTokenStore tokenStore) : ObservableObject
{
    public ObservableCollection<CourseResponseDto> Courses { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? statusMessage;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        StatusMessage = null;
        Courses.Clear();
        try
        {
            var result = await apiClient.GetCoursesAsync(cancellationToken).ConfigureAwait(false);
            if (await HandleAuthAsync(result.Error?.Code, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            if (!result.IsSuccess || result.Data is null)
            {
                StatusMessage = result.Error?.Message ?? "Dersler yüklenemedi.";
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var course in result.Data)
                {
                    Courses.Add(course);
                }
            }).ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
        }
    }

    [RelayCommand]
    private Task AddCourseAsync() =>
        Shell.Current.GoToAsync(Routes.CourseEdit);

    [RelayCommand]
    private Task EditCourseAsync(CourseResponseDto course) =>
        Shell.Current.GoToAsync($"{Routes.CourseEdit}?courseId={course.Id}");

    [RelayCommand]
    private async Task DeleteCourseAsync(CourseResponseDto course, CancellationToken cancellationToken)
    {
        var confirm = await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.DisplayAlert(
                "Dersi sil",
                $"{course.Code} — {course.Title} silinsin mi?",
                "Sil",
                "İptal")).ConfigureAwait(false);

        if (!confirm)
        {
            return;
        }

        var result = await apiClient.DeleteCourseAsync(course.Id, cancellationToken).ConfigureAwait(false);
        if (await HandleAuthAsync(result.Error?.Code, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.Error?.Message ?? "Ders silinemedi.";
            return;
        }

        if (LoadCommand.CanExecute(null))
        {
            LoadCommand.Execute(null);
        }
    }

    private async Task<bool> HandleAuthAsync(string? errorCode, CancellationToken cancellationToken)
    {
        if (errorCode != "UNAUTHORIZED")
        {
            return false;
        }

        await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync($"//{Routes.Login}")).ConfigureAwait(false);
        return true;
    }
}
