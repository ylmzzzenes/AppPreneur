using UniFlow.Mobile.Services;
using UniFlow.Mobile.Views;

namespace UniFlow.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));
        Routing.RegisterRoute(Routes.SyllabusPreview, typeof(SyllabusPreviewPage));
        Routing.RegisterRoute(Routes.Onboarding, typeof(OnboardingPage));
        Routing.RegisterRoute(Routes.Profile, typeof(ProfilePage));
        Routing.RegisterRoute(Routes.CourseEdit, typeof(CourseEditPage));
        Routing.RegisterRoute(Routes.TaskCreateEdit, typeof(TaskCreateEditPage));
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;
        try
        {
            var store = ServiceHelper.GetRequiredService<IAuthTokenStore>();
            if (await store.HasTokenAsync().ConfigureAwait(false))
            {
                var apiClient = ServiceHelper.GetRequiredService<IApiClient>();
                var userSession = ServiceHelper.GetRequiredService<IUserSessionInfo>();
                await AuthNavigation.NavigateAfterAuthenticationAsync(apiClient, store, userSession)
                    .ConfigureAwait(false);
            }
        }
        catch
        {
            // Startup yönlendirmesi başarısız olursa giriş ekranında kalınır.
        }
    }
}
