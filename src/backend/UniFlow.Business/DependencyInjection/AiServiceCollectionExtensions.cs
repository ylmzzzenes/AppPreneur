using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.Ai.Providers;
using UniFlow.Business.Configuration;
using UniFlow.Business.Scheduling;
using UniFlow.Business.Services.Ocr;
using UniFlow.Business.Syllabus;

namespace UniFlow.Business.DependencyInjection;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddUniFlowAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedTransient<IOcrService, StubOcrService>(OcrProvider.Stub);
        services.AddKeyedTransient<IOcrService, AzureDocumentIntelligenceOcrService>(OcrProvider.Azure);
        services.AddKeyedTransient<IOcrService, TesseractOcrService>(OcrProvider.Tesseract);
        services.AddKeyedTransient<IOcrService, AwsTextractOcrService>(OcrProvider.Aws);
        services.AddKeyedTransient<IOcrService, GeminiOcrService>(OcrProvider.Gemini);

        services.AddTransient<IOcrService>(sp =>
        {
            var provider = sp.GetRequiredService<IOptions<UniFlowOcrOptions>>().Value.Provider;
            return sp.GetRequiredKeyedService<IOcrService>(provider);
        });

        services.AddSingleton<GeminiAiProvider>();
        services.AddSingleton<OpenAiCompatibleProvider>();
        services.AddSingleton<FakeAiProvider>();
        services.AddSingleton<IAiProvider, AiProviderRouter>();

        services
            .AddHttpClient(GeminiAiProvider.HttpClientName, (sp, client) =>
            {
                var ai = sp.GetRequiredService<IOptions<AiOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(ai.TimeoutSeconds);
            })
            .AddPolicyHandler((services, _) => CreateRetryPolicy(services));

        services
            .AddHttpClient(OpenAiCompatibleProvider.HttpClientName, (sp, client) =>
            {
                var ai = sp.GetRequiredService<IOptions<AiOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(ai.TimeoutSeconds);
            })
            .AddPolicyHandler((services, _) => CreateRetryPolicy(services));

        services.AddScoped<SyllabusParsingService>();
        services.AddScoped<HeuristicSyllabusParsingService>();
        services.AddScoped<ISyllabusParsingService, SyllabusParsingServiceResolver>();
        services.AddSingleton<ITaskPriorityCalculator, AdaptiveTaskPriorityCalculator>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(IServiceProvider services)
    {
        var retryCount = services.GetRequiredService<IOptions<AiOptions>>().Value.RetryCount;
        if (retryCount <= 0)
        {
            return Policy.NoOpAsync<HttpResponseMessage>();
        }

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }
}
