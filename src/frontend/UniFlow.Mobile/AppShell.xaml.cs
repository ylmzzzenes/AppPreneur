using UniFlow.Mobile.Services;
using UniFlow.Mobile.Views;

namespace UniFlow.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));
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
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await GoToAsync($"//{Routes.MainTabs}/{Routes.Dashboard}"));
            }
        }
        catch
        {
            // Startup yönlendirmesi başarısız olursa giriş ekranında kalınır.
        }
    }
}
