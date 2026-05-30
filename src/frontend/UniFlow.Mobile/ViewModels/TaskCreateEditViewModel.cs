using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

[QueryProperty(nameof(TaskIdText), "taskId")]
[QueryProperty(nameof(CourseIdText), "courseId")]
public partial class TaskCreateEditViewModel(IApiClient apiClient, IAuthTokenStore tokenStore) : ObservableObject
{
    private long _taskId;

    public string? TaskIdText
    {
        set
        {
            if (long.TryParse(value, out var id) && id > 0)
            {
                _taskId = id;
                IsEditMode = true;
                _ = LoadTaskAsync(CancellationToken.None);
            }
        }
    }

    public string? CourseIdText
    {
        set => _preselectedCourseId = long.TryParse(value, out var id) ? id : 0;
    }

    private long _preselectedCourseId;

    public ObservableCollection<CourseResponseDto> Courses { get; } = new();

    public IReadOnlyList<TaskStatusOption> StatusOptions { get; } =
    [
        new("Bekliyor", TaskItemStatus.Pending),
        new("Tamamlandı", TaskItemStatus.Done),
        new("Kaçırıldı", TaskItemStatus.Missed),
    ];

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private CourseResponseDto? selectedCourse;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private DateTime dueDate = DateTime.Today;

    [ObservableProperty]
    private bool hasDueDate = true;

    [ObservableProperty]
    private string estimatedMinutes = "60";

    [ObservableProperty]
    private string priorityScore = "50";

    [ObservableProperty]
    private TaskStatusOption selectedStatus = new("Bekliyor", TaskItemStatus.Pending);

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
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
                Courses.Clear();
                foreach (var course in result.Data)
                {
                    Courses.Add(course);
                }

                SelectedCourse = Courses.FirstOrDefault(c => c.Id == _preselectedCourseId)
                    ?? Courses.FirstOrDefault();
            }).ConfigureAwait(false);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
        }
    }

    private async Task LoadTaskAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);

            var result = await apiClient.GetTaskAsync(_taskId, cancellationToken).ConfigureAwait(false);
            if (await HandleAuthAsync(result.Error?.Code, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            if (!result.IsSuccess || result.Data is null)
            {
                StatusMessage = result.Error?.Message ?? "Görev bulunamadı.";
                return;
            }

            var task = result.Data;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SelectedCourse = Courses.FirstOrDefault(c => c.Id == task.CourseId) ?? Courses.FirstOrDefault();
                Title = task.Title;
                Description = task.Description ?? string.Empty;
                HasDueDate = task.DueDate.HasValue;
                DueDate = task.DueDate?.Date ?? DateTime.Today;
                EstimatedMinutes = task.EstimatedMinutes?.ToString() ?? string.Empty;
                PriorityScore = task.PriorityScore?.ToString() ?? string.Empty;
                SelectedStatus = StatusOptions.FirstOrDefault(s => s.Value == task.Status) ?? StatusOptions[0];
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
        if (SelectedCourse is null)
        {
            StatusMessage = "Ders seçin.";
            return;
        }

        var courseId = SelectedCourse.Id;

        if (string.IsNullOrWhiteSpace(Title))
        {
            StatusMessage = "Başlık gerekli.";
            return;
        }

        int? minutes = null;
        if (!string.IsNullOrWhiteSpace(EstimatedMinutes))
        {
            if (!int.TryParse(EstimatedMinutes.Trim(), out var parsed) || parsed is < 0 or > 1440)
            {
                StatusMessage = "Tahmini süre 0–1440 dakika olmalı.";
                return;
            }

            minutes = parsed;
        }

        int? priority = null;
        if (!string.IsNullOrWhiteSpace(PriorityScore))
        {
            if (!int.TryParse(PriorityScore.Trim(), out var parsed) || parsed is < 0 or > 100)
            {
                StatusMessage = "Öncelik 0–100 arasında olmalı.";
                return;
            }

            priority = parsed;
        }

        var due = HasDueDate ? DueDate.Date : (DateTime?)null;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            ApiResultDto<TaskItemResponseDto> result;
            if (IsEditMode)
            {
                result = await apiClient.UpdateTaskAsync(
                        _taskId,
                        new UpdateTaskRequestDto
                        {
                            CourseId = courseId,
                            Title = Title.Trim(),
                            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                            DueDate = due,
                            EstimatedMinutes = minutes,
                            PriorityScore = priority,
                            Status = SelectedStatus.Value,
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                result = await apiClient.CreateTaskAsync(
                        new CreateTaskRequestDto
                        {
                            CourseId = courseId,
                            Title = Title.Trim(),
                            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                            DueDate = due,
                            EstimatedMinutes = minutes,
                            PriorityScore = priority,
                            Status = SelectedStatus.Value,
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
    private async Task DeleteAsync(CancellationToken cancellationToken)
    {
        if (!IsEditMode)
        {
            return;
        }

        var confirm = await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.DisplayAlert("Görevi sil", "Bu görev silinsin mi?", "Sil", "İptal")).ConfigureAwait(false);
        if (!confirm)
        {
            return;
        }

        var result = await apiClient.DeleteTaskAsync(_taskId, cancellationToken).ConfigureAwait(false);
        if (await HandleAuthAsync(result.Error?.Code, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.Error?.Message ?? "Görev silinemedi.";
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync("..")).ConfigureAwait(false);
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

public sealed record TaskStatusOption(string Label, TaskItemStatus Value)
{
    public override string ToString() => Label;
}
