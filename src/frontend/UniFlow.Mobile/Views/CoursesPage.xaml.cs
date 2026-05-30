using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class CoursesPage : ContentPage
{
    public CoursesPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<CoursesViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CoursesViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }
}
