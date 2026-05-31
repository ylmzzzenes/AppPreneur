using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UniFlow.DataAccess.Configuration;

namespace UniFlow.DataAccess.DependencyInjection;

internal static class DbContextConfigurationExtensions
{
    public static DbContextOptionsBuilder ConfigureUniFlowProvider(
        this DbContextOptionsBuilder options,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var provider = configuration.GetSection(DatabaseOptions.SectionName).GetValue<string>("Provider")
            ?? DatabaseProviders.SqlServer;

        return provider switch
        {
            DatabaseProviders.SqlServer => options.UseSqlServer(connectionString),
            DatabaseProviders.PostgreSql => options.UseNpgsql(connectionString),
            _ => throw new InvalidOperationException(
                $"Database:Provider '{provider}' is not supported. Use '{DatabaseProviders.SqlServer}' or '{DatabaseProviders.PostgreSql}'."),
        };
    }
}
