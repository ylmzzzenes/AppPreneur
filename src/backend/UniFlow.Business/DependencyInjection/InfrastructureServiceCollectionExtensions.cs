using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Configuration;
using UniFlow.Business.Services;
using UniFlow.Business.Validation;
using UniFlow.DataAccess.Interceptors;
using UniFlow.DataAccess.Persistence;
using UniFlow.DataAccess.Queries;
using UniFlow.DataAccess.UnitOfWork;

namespace UniFlow.Business.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddUniFlowInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<UniFlowDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserQueries, UserQueries>();
        services.AddScoped<ICourseQueries, CourseQueries>();
        services.AddScoped<ITaskQueries, TaskQueries>();
        services.AddScoped<IDashboardQueries, DashboardQueries>();
        services.AddScoped<ISyllabusScanSessionQueries, SyllabusScanSessionQueries>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ISyllabusFileValidationService, SyllabusFileValidationService>();
        services.AddScoped<ISyllabusIngestionService, SyllabusIngestionService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddSingleton<IDailyMessageService, DailyMessageService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
