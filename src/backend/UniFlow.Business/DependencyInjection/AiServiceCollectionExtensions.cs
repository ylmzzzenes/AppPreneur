using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Configuration;
using UniFlow.Business.Scheduling;
using UniFlow.Business.Services.Gemini;
using UniFlow.Business.Services.Ocr;
using UniFlow.Business.Syllabus;

namespace UniFlow.Business.DependencyInjection;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddUniFlowAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<UniFlowOcrOptions>(configuration.GetSection(UniFlowOcrOptions.SectionName));
        services.Configure<UniFlowGeminiOptions>(configuration.GetSection(UniFlowGeminiOptions.SectionName));

        services.AddKeyedTransient<IOcrService, StubOcrService>(OcrProvider.Stub);
        services.AddKeyedTransient<IOcrService, AzureDocumentIntelligenceOcrService>(OcrProvider.Azure);
        services.AddKeyedTransient<IOcrService, TesseractOcrService>(OcrProvider.Tesseract);
        services.AddKeyedTransient<IOcrService, AwsTextractOcrService>(OcrProvider.Aws);

        services.AddTransient<IOcrService>(sp =>
        {
            var provider = sp.GetRequiredService<IOptions<UniFlowOcrOptions>>().Value.Provider;
            return sp.GetRequiredKeyedService<IOcrService>(provider);
        });

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        services
            .AddHttpClient<IGeminiService, GeminiService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(2);
            })
            .AddPolicyHandler(retryPolicy);

        services.AddScoped<SyllabusParsingService>();
        services.AddScoped<HeuristicSyllabusParsingService>();
        services.AddScoped<ISyllabusParsingService, SyllabusParsingServiceResolver>();
        services.AddSingleton<ITaskPriorityCalculator, AdaptiveTaskPriorityCalculator>();

        return services;
    }
}
