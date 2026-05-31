using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;

namespace UniFlow.API.Infrastructure;

internal static class DatabaseMigrationExtensions
{
    public static async Task ApplyDevelopmentMigrationsAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UniFlowDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("UniFlow.Database");

        logger.LogInformation("Applying EF Core migrations (Development)...");
        await db.Database.MigrateAsync().ConfigureAwait(false);
        logger.LogInformation("Database migrations applied.");
    }
}
