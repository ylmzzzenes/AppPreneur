using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UniFlow.Business.Ai;
using UniFlow.Business.Ai.Providers;
using UniFlow.Business.Configuration;
using Xunit;

namespace UniFlow.Business.Tests.Ai;

public sealed class OpenAiCompatibleProviderTests
{
    [Fact]
    public async Task GenerateTextAsync_MapsRequestBody_WithSystemAndUserMessages()
    {
        string? capturedBody = null;
        var handler = new CaptureHttpMessageHandler(request =>
        {
            capturedBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"content":"parsed-content"}}]}""",
                    Encoding.UTF8,
                    "application/json"),
            };
        });

        var provider = CreateProvider(handler, new AiOptions
        {
            Provider = AiProviders.OpenAiCompatible,
            ApiKey = "sk-test",
            BaseUrl = "https://api.openai.com/v1/",
            Model = "gpt-4o-mini",
        });

        var response = await provider.GenerateTextAsync(new AiTextRequest
        {
            SystemPrompt = "You are helpful.",
            UserPrompt = "Summarize syllabus.",
            Temperature = 0.2,
        });

        response.Content.Should().Be("parsed-content");
        response.Provider.Should().Be(AiProviders.OpenAiCompatible);
        response.Model.Should().Be("gpt-4o-mini");

        capturedBody.Should().NotBeNull();
        using var doc = JsonDocument.Parse(capturedBody!);
        doc.RootElement.GetProperty("model").GetString().Should().Be("gpt-4o-mini");
        var messages = doc.RootElement.GetProperty("messages");
        messages.GetArrayLength().Should().Be(2);
        messages[0].GetProperty("role").GetString().Should().Be("system");
        messages[0].GetProperty("content").GetString().Should().Be("You are helpful.");
        messages[1].GetProperty("role").GetString().Should().Be("user");
        messages[1].GetProperty("content").GetString().Should().Be("Summarize syllabus.");
        doc.RootElement.GetProperty("temperature").GetDouble().Should().Be(0.2);
    }

    [Fact]
    public async Task GenerateTextAsync_DoesNotLogRawResponse()
    {
        const string secretResponse = """{"choices":[{"message":{"content":"super-secret-ai-output"}}]}""";
        var handler = new CaptureHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(secretResponse, Encoding.UTF8, "application/json"),
        });

        var logger = new ListLogger<OpenAiCompatibleProvider>();
        var provider = CreateProvider(handler, new AiOptions
        {
            Provider = AiProviders.OpenAiCompatible,
            ApiKey = "sk-test",
            BaseUrl = "https://api.groq.com/openai/v1",
            Model = "openai/gpt-oss-20b",
        }, logger);

        var response = await provider.GenerateTextAsync(new AiTextRequest { UserPrompt = "hello" });

        response.Content.Should().Be("super-secret-ai-output");
        logger.Entries.Should().NotContain(e => e.Message.Contains("super-secret-ai-output", StringComparison.Ordinal));
        logger.Entries.Should().NotContain(e => e.Message.Contains(secretResponse, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("https://api.openai.com/v1/", "https://api.openai.com/v1")]
    [InlineData("https://openrouter.ai/api/v1", "https://openrouter.ai/api/v1")]
    public void NormalizeBaseUrl_TrimsTrailingSlash(string input, string expected)
    {
        OpenAiCompatibleProvider.NormalizeBaseUrl(input).Should().Be(expected);
    }

    private static OpenAiCompatibleProvider CreateProvider(
        HttpMessageHandler handler,
        AiOptions options,
        ILogger<OpenAiCompatibleProvider>? logger = null)
    {
        var factory = new SingleClientHttpClientFactory(
            OpenAiCompatibleProvider.HttpClientName,
            new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });

        return new OpenAiCompatibleProvider(
            factory,
            Microsoft.Extensions.Options.Options.Create(options),
            logger ?? NullLogger<OpenAiCompatibleProvider>.Instance);
    }

    private sealed class CaptureHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }

    private sealed class SingleClientHttpClientFactory(string name, HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string clientName) =>
            string.Equals(clientName, name, StringComparison.Ordinal) ? client : throw new InvalidOperationException();
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
