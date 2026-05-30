using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

[QueryProperty(nameof(CourseIdText), "courseId")]
public partial class CourseEditViewModel(IApiClient apiClient, IAuthTokenStore tokenStore) : ObservableObject
{
    private long _courseId;

    public string? CourseIdText
    {
        set
        {
            if (long.TryParse(value, out var id) && id > 0)
            {
                _courseId = id;
                IsEditMode = true;
                _ = LoadAsync(CancellationToken.None);
            }
        }
    }

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string code = string.Empty;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string color = "#6366F1";

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (_courseId <= 0)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await apiClient.GetCourseAsync(_courseId, cancellationToken).ConfigureAwait(false);
            if (await HandleAuthAsync(result.Error?.Code, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            if (!result.IsSuccess || result.Data is null)
            {
                StatusMessage = result.Error?.Message ?? "Ders bulunamadı.";
                return;
            }

            var course = result.Data;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Code = course.Code;
                Title = course.Title;
                Description = course.Description ?? string.Empty;
                Color = course.Color ?? "#6366F1";
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
        if (string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(Title))
        {
            StatusMessage = "Kod ve başlık gerekli.";
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            ApiResultDto<CourseResponseDto> result;
            if (IsEditMode)
            {
                result = await apiClient.UpdateCourseAsync(
                        _courseId,
                        new UpdateCourseRequestDto
                        {
                            Code = Code.Trim(),
                            Title = Title.Trim(),
                            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                            Color = string.IsNullOrWhiteSpace(Color) ? null : Color.Trim(),
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                result = await apiClient.CreateCourseAsync(
                        new CreateCourseRequestDto
                        {
                            Code = Code.Trim(),
                            Title = Title.Trim(),
                            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                            Color = string.IsNullOrWhiteSpace(Color) ? null : Color.Trim(),
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (await HandleAuthAsync(result.Error?.Code, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            if (!result.IsSuccess)
            {
                StatusMessage = result.Error?.Message ?? "Kayıt başarısız.";
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync("..")).ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }
    }

    [RelayCommand]
    private Task CancelAsync() => Shell.Current.GoToAsync("..");

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
