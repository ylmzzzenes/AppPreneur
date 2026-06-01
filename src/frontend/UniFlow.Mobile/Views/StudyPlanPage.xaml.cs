using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class StudyPlanPage : ContentPage
{
    public StudyPlanPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<StudyPlanViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StudyPlanViewModel vm)
        {
            if (vm.LoadCoursesCommand.CanExecute(null))
            {
                vm.LoadCoursesCommand.Execute(null);
            }
        }
    }
}
