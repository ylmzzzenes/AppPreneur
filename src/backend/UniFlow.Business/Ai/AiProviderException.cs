namespace UniFlow.Business.Ai;

public sealed class AiProviderException : Exception
{
    public AiProviderException(string code, string message, string provider)
        : base(message)
    {
        Code = code;
        Provider = provider;
    }

    public string Code { get; }

    public string Provider { get; }
}
