using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class TasksViewModel(
    IApiClient apiClient,
    IAuthTokenStore tokenStore) : ObservableObject
{
    public ObservableCollection<TaskItemResponseDto> Tasks { get; } = new();
    public ObservableCollection<CourseResponseDto> Courses { get; } = new();

    public IReadOnlyList<TaskListFilterOption> RangeFilters { get; } =
    [
        new("Bugün", TaskListRange.Today),
        new("Yaklaşan", TaskListRange.Upcoming),
        new("Tümü", TaskListRange.All),
    ];

    public IReadOnlyList<TaskStatusFilterOption> StatusFilters { get; } =
    [
        new("Tüm durumlar", null),
        new("Bekliyor", TaskItemStatus.Pending),
        new("Tamamlandı", TaskItemStatus.Done),
        new("Kaçırıldı", TaskItemStatus.Missed),
    ];

    [ObservableProperty]
    private TaskListFilterOption selectedRange = new("Tümü", TaskListRange.All);

    [ObservableProperty]
    private TaskStatusFilterOption selectedStatusFilter = new("Tüm durumlar", null);

    [ObservableProperty]
    private CourseResponseDto? selectedCourse;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? statusMessage;

    partial void OnSelectedRangeChanged(TaskListFilterOption value) => _ = LoadAsync(CancellationToken.None);

    partial void OnSelectedStatusFilterChanged(TaskStatusFilterOption value) => ApplyFilters();

    partial void OnSelectedCourseChanged(CourseResponseDto? value) => ApplyFilters();

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        StatusMessage = null;
        Tasks.Clear();

        try
        {
            if (Courses.Count == 0)
            {
                var coursesResult = await apiClient.GetCoursesAsync(cancellationToken).ConfigureAwait(false);
                if (await HandleAuthAsync(coursesResult.Error?.Code, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }

                if (coursesResult.IsSuccess && coursesResult.Data is not null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Courses.Clear();
                        foreach (var course in coursesResult.Data)
                        {
                            Courses.Add(course);
                        }
                    }).ConfigureAwait(false);
                }
            }

            IReadOnlyList<TaskItemResponseDto> items;
            switch (SelectedRange.Value)
            {
                case TaskListRange.Today:
                {
                    var today = await apiClient.GetTodayTasksAsync(cancellationToken).ConfigureAwait(false);
                    if (await HandleAuthAsync(today.Error?.Code, cancellationToken).ConfigureAwait(false))
                    {
                        return;
                    }

                    if (!today.IsSuccess || today.Data is null)
                    {
                        StatusMessage = today.Error?.Message ?? "Görevler yüklenemedi.";
                        return;
                    }

                    items = today.Data.Items;
                    break;
                }
                case TaskListRange.Upcoming:
                {
                    var upcoming = await apiClient.GetUpcomingTasksAsync(14, null, cancellationToken).ConfigureAwait(false);
                    if (await HandleAuthAsync(upcoming.Error?.Code, cancellationToken).ConfigureAwait(false))
                    {
                        return;
                    }

                    if (!upcoming.IsSuccess || upcoming.Data is null)
                    {
                        StatusMessage = upcoming.Error?.Message ?? "Görevler yüklenemedi.";
                        return;
                    }

                    items = upcoming.Data;
                    break;
                }
                default:
                {
                    var all = await apiClient.GetTasksAsync(cancellationToken).ConfigureAwait(false);
                    if (await HandleAuthAsync(all.Error?.Code, cancellationToken).ConfigureAwait(false))
                    {
                        return;
                    }

                    if (!all.IsSuccess || all.Data is null)
                    {
                        StatusMessage = all.Error?.Message ?? "Görevler yüklenemedi.";
                        return;
                    }

                    items = all.Data;
                    break;
                }
            }

            _allTasks = items.ToList();
            ApplyFilters();
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
        }
    }

    private List<TaskItemResponseDto> _allTasks = [];

    private void ApplyFilters()
    {
        var filtered = _allTasks.AsEnumerable();

        if (SelectedCourse is not null)
        {
            filtered = filtered.Where(t => t.CourseId == SelectedCourse.Id);
        }

        if (SelectedStatusFilter.Value.HasValue)
        {
            filtered = filtered.Where(t => t.Status == SelectedStatusFilter.Value.Value);
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Tasks.Clear();
            foreach (var task in filtered.OrderByDescending(t => t.PriorityScore).ThenBy(t => t.DueDate))
            {
                Tasks.Add(task);
            }
        });
    }

    [RelayCommand]
    private Task AddTaskAsync() => Shell.Current.GoToAsync(Routes.TaskCreateEdit);

    [RelayCommand]
    private Task EditTaskAsync(TaskItemResponseDto task) =>
        Shell.Current.GoToAsync($"{Routes.TaskCreateEdit}?taskId={task.Id}");

    [RelayCommand]
    private async Task ChangeStatusAsync(TaskItemResponseDto task, CancellationToken cancellationToken)
    {
        var choice = await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            return await Shell.Current.DisplayActionSheet(
                "Durum seç",
                "İptal",
                null,
                "Bekliyor",
                "Tamamlandı",
                "Kaçırıldı").ConfigureAwait(true);
        }).ConfigureAwait(false);

        var newStatus = choice switch
        {
            "Bekliyor" => TaskItemStatus.Pending,
            "Tamamlandı" => TaskItemStatus.Done,
            "Kaçırıldı" => TaskItemStatus.Missed,
            _ => (TaskItemStatus?)null,
        };

        if (newStatus is null || newStatus == task.Status)
        {
            return;
        }

        var previous = task.Status;
        task.Status = newStatus.Value;

        var result = await apiClient.UpdateTaskStatusAsync(task.Id, newStatus.Value, cancellationToken).ConfigureAwait(false);
        if (await HandleAuthAsync(result.Error?.Code, cancellationToken).ConfigureAwait(false))
        {
            task.Status = previous;
            return;
        }

        if (!result.IsSuccess)
        {
            task.Status = previous;
            StatusMessage = result.Error?.Message ?? "Durum güncellenemedi.";
            return;
        }

        _ = TaskFeedbackHelper.TryShowFeedbackAsync(apiClient, task.Id, newStatus.Value, cancellationToken);
        ApplyFilters();
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(TaskItemResponseDto task, CancellationToken cancellationToken)
    {
        var confirm = await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.DisplayAlert("Görevi sil", $"{task.Title} silinsin mi?", "Sil", "İptal")).ConfigureAwait(false);
        if (!confirm)
        {
            return;
        }

        var result = await apiClient.DeleteTaskAsync(task.Id, cancellationToken).ConfigureAwait(false);
        if (await HandleAuthAsync(result.Error?.Code, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.Error?.Message ?? "Görev silinemedi.";
            return;
        }

        _allTasks.RemoveAll(t => t.Id == task.Id);
        ApplyFilters();
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

public enum TaskListRange
{
    Today,
    Upcoming,
    All,
}

public sealed record TaskListFilterOption(string Label, TaskListRange Value)
{
    public override string ToString() => Label;
}

public sealed record TaskStatusFilterOption(string Label, TaskItemStatus? Value)
{
    public override string ToString() => Label;
}
