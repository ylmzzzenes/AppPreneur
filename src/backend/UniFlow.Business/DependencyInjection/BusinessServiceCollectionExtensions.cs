using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Services;
using UniFlow.Business.Services.SyllabusTextStorage;
using UniFlow.Business.Validation;

namespace UniFlow.Business.DependencyInjection;

public static class BusinessServiceCollectionExtensions
{
    public static IServiceCollection AddUniFlowBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ISyllabusFileValidationService, SyllabusFileValidationService>();
        services.AddSingleton<ISyllabusTextStoragePolicy, SyllabusTextStoragePolicy>();
        services.AddScoped<ISyllabusIngestionService, SyllabusIngestionService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddSingleton<IDailyMessageService, DailyMessageService>();
        services.AddScoped<IPersonalizedDailyMessageService, PersonalizedDailyMessageService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStudyPlanService, StudyPlanService>();
        services.AddScoped<ITaskFeedbackService, TaskFeedbackService>();
        services.AddScoped<IWeeklySummaryService, WeeklySummaryService>();

        services.AddUniFlowAi(configuration);

        return services;
    }
}
