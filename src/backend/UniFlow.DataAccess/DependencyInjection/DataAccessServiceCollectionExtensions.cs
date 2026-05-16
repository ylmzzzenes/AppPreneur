using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniFlow.DataAccess.Interceptors;
using UniFlow.DataAccess.Persistence;
using UniFlow.DataAccess.Queries;
using UniFlow.DataAccess.UnitOfWork;

namespace UniFlow.DataAccess.DependencyInjection;

public static class DataAccessServiceCollectionExtensions
{
    public static IServiceCollection AddUniFlowDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<UniFlowDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.AddScoped<IUserQueries, UserQueries>();
        services.AddScoped<ICourseQueries, CourseQueries>();
        services.AddScoped<ITaskQueries, TaskQueries>();
        services.AddScoped<IDashboardQueries, DashboardQueries>();
        services.AddScoped<ISyllabusScanSessionQueries, SyllabusScanSessionQueries>();

        return services;
    }
}
