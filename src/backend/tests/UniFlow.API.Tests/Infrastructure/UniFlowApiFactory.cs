using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UniFlow.DataAccess.Persistence;

namespace UniFlow.API.Tests.Infrastructure;

/// <summary>
/// Boots the API with environment <c>Testing</c>, SQLite in-memory database, and test configuration.
/// Never uses production SQL Server connection strings.
/// </summary>
public sealed class UniFlowApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=UniFlowIntegrationTests;Mode=Memory;Cache=Shared");

    public UniFlowApiFactory()
    {
        _connection.Open();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    public HttpClient CreateAuthenticatedClient() => CreateClient();

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UniFlowDbContext>();
        await db.Database.EnsureDeletedAsync().ConfigureAwait(false);
        await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<UniFlowDbContext>>();
            services.RemoveAll<UniFlowDbContext>();

            services.AddDbContext<UniFlowDbContext>((sp, options) =>
            {
                options.UseSqlite(_connection);
                options.AddInterceptors(sp.GetRequiredService<UniFlow.DataAccess.Interceptors.AuditInterceptor>());
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
