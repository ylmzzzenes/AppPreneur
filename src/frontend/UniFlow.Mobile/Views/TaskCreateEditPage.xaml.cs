using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class TaskCreateEditPage : ContentPage
{
    public TaskCreateEditPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<TaskCreateEditViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TaskCreateEditViewModel vm && !vm.IsEditMode && vm.InitializeCommand.CanExecute(null))
        {
            vm.InitializeCommand.Execute(null);
        }
    }
}
