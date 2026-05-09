using Microsoft.Extensions.Logging;
using UniFlow.Mobile.Services;
using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IAuthTokenStore, SecureAuthTokenStore>();
        builder.Services.AddSingleton<IUserSessionInfo, UserSessionInfo>();
        builder.Services.AddTransient<AuthHeaderHandler>();
        builder.Services
            .AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(ApiConstants.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(120);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<SyllabusViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        ServiceHelper.Init(app.Services);
        return app;
    }
}
