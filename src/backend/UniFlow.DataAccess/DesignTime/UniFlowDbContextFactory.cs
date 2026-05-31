using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using UniFlow.DataAccess.DependencyInjection;
using UniFlow.DataAccess.Persistence;

namespace UniFlow.DataAccess.DesignTime;

/// <summary>
/// Design-time factory for EF Core CLI (migrations).
/// Reads Database:Provider and ConnectionStrings:DefaultConnection from environment variables.
/// </summary>
public sealed class UniFlowDbContextFactory : IDesignTimeDbContextFactory<UniFlowDbContext>
{
    public UniFlowDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<UniFlowDbContext>();
        optionsBuilder.ConfigureUniFlowProvider(configuration);
        return new UniFlowDbContext(optionsBuilder.Options);
    }
}
