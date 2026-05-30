using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<ProfileViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }
}
