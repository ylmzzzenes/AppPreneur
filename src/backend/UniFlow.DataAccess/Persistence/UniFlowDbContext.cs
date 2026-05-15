using Microsoft.EntityFrameworkCore;
using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Persistence;

public sealed class UniFlowDbContext : DbContext
{
    public UniFlowDbContext(DbContextOptions<UniFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Syllabus> Syllabi => Set<Syllabus>();

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    public DbSet<SyllabusScanSession> SyllabusScanSessions => Set<SyllabusScanSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UniFlowDbContext).Assembly);
    }
}
