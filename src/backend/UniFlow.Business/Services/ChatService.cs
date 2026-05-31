using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.Configuration;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class ChatService(
    IAiProvider aiProvider,
    IOptions<AiOptions> aiOptions,
    ILogger<ChatService> logger) : IChatService
{
    private readonly Lazy<string> _promptTemplate = new(LoadPrompt, LazyThreadSafetyMode.ExecutionAndPublication);

    public async Task<Result<string>> ReplyAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var prompt = _promptTemplate.Value.Replace("{{USER_MESSAGE}}", userMessage.Trim(), StringComparison.Ordinal);
        var options = aiOptions.Value;

        try
        {
            var response = await aiProvider.GenerateTextAsync(
                    new AiTextRequest
                    {
                        UserPrompt = prompt,
                        PromptVersion = options.PromptVersion,
                        Model = options.Model,
                        Metadata = new Dictionary<string, string> { ["kind"] = "chat" },
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            AiRequestLogger.LogCompleted(logger, response, prompt.Length);
            return Result<string>.Success(response.Content);
        }
        catch (AiProviderException ex)
        {
            AiRequestLogger.LogFailed(logger, ex.Provider, ex.Code);
            return Result<string>.Fail(ex.Code, ex.Message);
        }
    }

    private static string LoadPrompt()
    {
        var assembly = typeof(ChatService).Assembly;
        var name = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("chat-sarkastik-dahi.md", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Embedded resource chat-sarkastik-dahi.md was not found.");

        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Could not open embedded resource {name}.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
