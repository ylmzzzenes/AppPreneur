using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using UniFlow.DataAccess.Persistence;

namespace UniFlow.DataAccess.DesignTime;

/// <summary>
/// Design-time factory for EF Core CLI (migrations) when DataAccess is the migrations project.
/// </summary>
public sealed class UniFlowDbContextFactory : IDesignTimeDbContextFactory<UniFlowDbContext>
{
    public UniFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UniFlowDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=UniFlowDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        return new UniFlowDbContext(optionsBuilder.Options);
    }
}
