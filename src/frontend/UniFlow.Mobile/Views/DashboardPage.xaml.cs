using System.ComponentModel;
using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private DashboardViewModel? _vm;

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<DashboardViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is not DashboardViewModel vm)
            return;

        if (!ReferenceEquals(_vm, vm))
        {
            if (_vm != null)
                _vm.PropertyChanged -= OnVmPropertyChanged;

            _vm = vm;
            _vm.PropertyChanged += OnVmPropertyChanged;
        }

        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_vm != null)
        {
            _vm.PropertyChanged -= OnVmPropertyChanged;
            _vm = null;
        }
    }

    private async void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not DashboardViewModel vm || e.PropertyName != nameof(DashboardViewModel.SnackMessage))
            return;

        await UpdateSnackbarAsync(vm.SnackMessage).ConfigureAwait(false);
    }

    private async Task UpdateSnackbarAsync(string? message)
    {
        await Dispatcher.DispatchAsync(async () =>
        {
            if (string.IsNullOrEmpty(message))
            {
                if (SnackbarHost.IsVisible)
                {
                    await SnackbarHost.TranslateTo(0, -180, 180, Easing.CubicIn).ConfigureAwait(true);
                    SnackbarHost.IsVisible = false;
                }

                return;
            }

            SnackbarLabel.Text = message;
            if (!SnackbarHost.IsVisible)
            {
                SnackbarHost.IsVisible = true;
                SnackbarHost.TranslationY = -180;
                await SnackbarHost.TranslateTo(0, 0, 220, Easing.CubicOut).ConfigureAwait(true);
            }
        }).ConfigureAwait(false);
    }
}
