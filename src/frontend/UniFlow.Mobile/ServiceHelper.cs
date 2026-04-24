using Microsoft.Extensions.DependencyInjection;

namespace UniFlow.Mobile;

internal static class ServiceHelper
{
    private static IServiceProvider? _services;

    public static void Init(IServiceProvider services) => _services = services;

    public static T GetRequiredService<T>()
        where T : notnull
        => _services!.GetRequiredService<T>();
}
