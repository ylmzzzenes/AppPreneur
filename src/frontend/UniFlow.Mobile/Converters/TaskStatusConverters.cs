using System.Globalization;
using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Converters;

public sealed class TaskStatusToLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TaskItemStatus status
            ? status switch
            {
                TaskItemStatus.Pending => "Bekliyor",
                TaskItemStatus.Done => "Tamamlandı",
                TaskItemStatus.Missed => "Kaçırıldı",
                _ => status.ToString(),
            }
            : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class TaskStatusToBadgeBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TaskItemStatus status
            ? status switch
            {
                TaskItemStatus.Pending => Color.FromArgb("#EEF2FF"),
                TaskItemStatus.Done => Color.FromArgb("#DCFCE7"),
                TaskItemStatus.Missed => Color.FromArgb("#FFEDD5"),
                _ => Color.FromArgb("#F1F5F9"),
            }
            : Color.FromArgb("#F1F5F9");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class TaskStatusToBadgeTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TaskItemStatus status
            ? status switch
            {
                TaskItemStatus.Pending => Color.FromArgb("#4338CA"),
                TaskItemStatus.Done => Color.FromArgb("#15803D"),
                TaskItemStatus.Missed => Color.FromArgb("#C2410C"),
                _ => Color.FromArgb("#475569"),
            }
            : Color.FromArgb("#475569");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
