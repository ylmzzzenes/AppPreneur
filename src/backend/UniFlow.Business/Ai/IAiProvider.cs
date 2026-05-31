namespace UniFlow.Business.Ai;

public interface IAiProvider
{
    Task<AiTextResponse> GenerateTextAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default);
}
