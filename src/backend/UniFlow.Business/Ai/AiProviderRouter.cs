using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UniFlow.Business.Ai.Providers;
using UniFlow.Business.Configuration;

namespace UniFlow.Business.Ai;

public sealed class AiProviderRouter : IAiProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AiOptions _options;

    public AiProviderRouter(IServiceProvider serviceProvider, IOptions<AiOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public Task<AiTextResponse> GenerateTextAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default)
    {
        return ResolveProvider().GenerateTextAsync(request, cancellationToken);
    }

    private IAiProvider ResolveProvider()
    {
        if (string.Equals(_options.Provider, AiProviders.Gemini, StringComparison.OrdinalIgnoreCase))
        {
            return new GeminiAiProviderAdapter(_serviceProvider.GetRequiredService<GeminiAiProvider>());
        }

        if (string.Equals(_options.Provider, AiProviders.OpenAiCompatible, StringComparison.OrdinalIgnoreCase))
        {
            return new OpenAiCompatibleProviderAdapter(_serviceProvider.GetRequiredService<OpenAiCompatibleProvider>());
        }

        if (string.Equals(_options.Provider, AiProviders.Fake, StringComparison.OrdinalIgnoreCase))
        {
            return new FakeAiProviderAdapter(_serviceProvider.GetRequiredService<FakeAiProvider>());
        }

        throw new InvalidOperationException(
            $"Ai:Provider '{_options.Provider}' is not supported. Use Gemini, OpenAiCompatible, or Fake.");
    }

    private sealed class GeminiAiProviderAdapter(GeminiAiProvider inner) : IAiProvider
    {
        public Task<AiTextResponse> GenerateTextAsync(AiTextRequest request, CancellationToken cancellationToken = default) =>
            inner.GenerateTextAsync(request, cancellationToken);
    }

    private sealed class OpenAiCompatibleProviderAdapter(OpenAiCompatibleProvider inner) : IAiProvider
    {
        public Task<AiTextResponse> GenerateTextAsync(AiTextRequest request, CancellationToken cancellationToken = default) =>
            inner.GenerateTextAsync(request, cancellationToken);
    }

    private sealed class FakeAiProviderAdapter(FakeAiProvider inner) : IAiProvider
    {
        public Task<AiTextResponse> GenerateTextAsync(AiTextRequest request, CancellationToken cancellationToken = default) =>
            inner.GenerateTextAsync(request, cancellationToken);
    }
}
