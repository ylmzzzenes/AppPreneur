using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.AiProduct;
using UniFlow.Business.Configuration;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class ChatService(
    IAiProvider aiProvider,
    IOptions<AiOptions> aiOptions,
    IUserQueries userQueries,
    ICourseQueries courseQueries,
    ITaskQueries taskQueries,
    ILogger<ChatService> logger) : IChatService
{
    private const string DefaultChatSystemPromptResource = "chat-sarkastik-dahi.md";

    private readonly Lazy<string> _defaultSystemPrompt = new(
        () => AiPromptLoader.Load(DefaultChatSystemPromptResource),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public async Task<Result<string>> ReplyAsync(
        long userId,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var trimmed = userMessage.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Result<string>.Fail("VALIDATION", "Mesaj boş olamaz.");
        }

        var options = aiOptions.Value;
        var systemPrompt = ResolveChatSystemPrompt(options);
        var contextBlock = await ChatUserContextBuilder.BuildAsync(
                userId,
                userQueries,
                courseQueries,
                taskQueries,
                cancellationToken)
            .ConfigureAwait(false);

        var userPrompt = $"""
            {contextBlock}

            Öğrenci mesajı:
            {trimmed}
            """;

        try
        {
            var response = await aiProvider.GenerateTextAsync(
                    new AiTextRequest
                    {
                        SystemPrompt = systemPrompt,
                        UserPrompt = userPrompt,
                        PromptVersion = options.PromptVersion,
                        Model = options.Model,
                        Temperature = 0.7,
                        Metadata = new Dictionary<string, string> { ["kind"] = "chat" },
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            AiRequestLogger.LogCompleted(logger, response, userPrompt.Length + systemPrompt.Length);
            return Result<string>.Success(response.Content);
        }
        catch (AiProviderException ex)
        {
            AiRequestLogger.LogFailed(logger, ex.Provider, ex.Code);
            return Result<string>.Fail(ex.Code, ex.Message);
        }
    }

    private string ResolveChatSystemPrompt(AiOptions options) =>
        string.IsNullOrWhiteSpace(options.ChatSystemPrompt)
            ? _defaultSystemPrompt.Value
            : options.ChatSystemPrompt.Trim();
}
