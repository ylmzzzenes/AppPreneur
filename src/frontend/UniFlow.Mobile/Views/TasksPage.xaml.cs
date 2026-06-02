using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class TasksPage : ContentPage
{
    public TasksPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<TasksViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TasksViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }
}
