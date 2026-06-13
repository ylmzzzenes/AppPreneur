namespace UniFlow.API.Configuration;

public static class CorsExtensions
{
    public const string PolicyName = "UniFlowCors";

    public static IServiceCollection AddUniFlowCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var section = configuration.GetSection(CorsOptions.SectionName);
        var configuredOrigins = (section.GetSection("AllowedOrigins").Get<string[]>() ?? [])
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();

        services.Configure<CorsOptions>(section);

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (configuredOrigins.Length > 0)
                {
                    policy.WithOrigins(configuredOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    return;
                }

                if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
                {
                    policy.SetIsOriginAllowed(origin =>
                        {
                            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            {
                                return false;
                            }

                            return uri.Host is "localhost" or "127.0.0.1" or "10.0.2.2"
                                   || uri.Host.StartsWith("192.168.", StringComparison.Ordinal);
                        })
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }
}
