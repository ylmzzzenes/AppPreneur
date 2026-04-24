using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface IChatService
{
    Task<Result<string>> ReplyAsync(string userMessage, CancellationToken cancellationToken = default);
}
