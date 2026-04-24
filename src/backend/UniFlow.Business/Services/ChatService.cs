using Microsoft.Extensions.Logging;
using UniFlow.Business.Abstractions;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class ChatService(IGeminiService geminiService, ILogger<ChatService> logger) : IChatService
{
    private readonly Lazy<string> _promptTemplate = new(LoadPrompt, LazyThreadSafetyMode.ExecutionAndPublication);

    public async Task<Result<string>> ReplyAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var prompt = _promptTemplate.Value.Replace("{{USER_MESSAGE}}", userMessage.Trim(), StringComparison.Ordinal);
        var result = await geminiService.GenerateTextAsync(prompt, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Chat Gemini error: {Code} {Message}", result.Error?.Code, result.Error?.Message);
        }

        return result;
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
