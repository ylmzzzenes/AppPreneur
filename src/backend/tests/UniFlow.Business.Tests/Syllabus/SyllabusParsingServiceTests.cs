using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using UniFlow.Business.Ai;
using UniFlow.Business.Configuration;
using UniFlow.Business.Syllabus;
using Xunit;

namespace UniFlow.Business.Tests.Syllabus;

public sealed class SyllabusParsingServiceTests
{
    [Fact]
    public async Task ParseTasksFromSyllabusTextAsync_InvalidAiJson_ReturnsJsonNotFound()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResponse
            {
                Content = "This is not JSON at all.",
                Provider = AiProviders.Gemini,
                Model = "gemini-2.0-flash",
            });

        var service = new SyllabusParsingService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions { Model = "gemini-2.0-flash", PromptVersion = "v1" }),
            NullLogger<SyllabusParsingService>.Instance);

        var result = await service.ParseTasksFromSyllabusTextAsync("CS101 syllabus with Final 2026-12-01");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("SYLLABUS_JSON_NOT_FOUND");
    }

    [Fact]
    public async Task ParseTasksFromSyllabusTextAsync_ValidAiJson_ReturnsTasks()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResponse
            {
                Content = """
                          ```json
                          [{"title":"Final Exam","dueDate":"2026-12-01","category":"Final"}]
                          ```
                          """,
                Provider = AiProviders.Gemini,
                Model = "gemini-2.0-flash",
            });

        var service = new SyllabusParsingService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions()),
            NullLogger<SyllabusParsingService>.Instance);

        var result = await service.ParseTasksFromSyllabusTextAsync("syllabus body");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle(t => t.Title == "Final Exam");
    }

    [Fact]
    public async Task ParseTasksFromSyllabusTextAsync_EmptyAiArray_FallsBackToContentExtractor()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResponse
            {
                Content = "[]",
                Provider = AiProviders.Gemini,
                Model = "gemini-2.5-flash",
            });

        var service = new SyllabusParsingService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions()),
            NullLogger<SyllabusParsingService>.Instance);

        const string syllabus = """
            Dersin Amacı
            Öğrencilere matematiksel becerilerin kazandırılması.
            Dersin İçeriği
            Sayılar sınıflandırmasını yapabilmek
            """;

        var result = await service.ParseTasksFromSyllabusTextAsync(syllabus);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        result.Data!.Should().Contain(t => t.Title.Contains("Sayılar", StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class SyllabusParsingServiceResolverTests
{
    [Fact]
    public async Task FakeProvider_UsesHeuristicParser()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        var aiParsing = new SyllabusParsingService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions { Model = "gemini-2.0-flash" }),
            NullLogger<SyllabusParsingService>.Instance);

        var resolver = CreateResolver(
            aiParsing,
            new AiOptions { Provider = AiProviders.Fake, Model = "fake", EnableFallback = true },
            environment: "Testing");

        var result = await resolver.ParseTasksFromSyllabusTextAsync("Final sinav 2026-12-15");

        result.IsSuccess.Should().BeTrue();
        await aiProvider.DidNotReceive().GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GeminiWithoutApiKey_InTesting_UsesHeuristicParser()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        var aiParsing = new SyllabusParsingService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions { Model = "gemini-2.0-flash" }),
            NullLogger<SyllabusParsingService>.Instance);

        var resolver = CreateResolver(
            aiParsing,
            new AiOptions
            {
                Provider = AiProviders.Gemini,
                ApiKey = "",
                Model = "gemini-2.0-flash",
                EnableFallback = true,
            },
            environment: "Testing");

        var result = await resolver.ParseTasksFromSyllabusTextAsync("Quiz 2026-05-20");

        result.IsSuccess.Should().BeTrue();
        await aiProvider.DidNotReceive().GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GeminiWithApiKey_UsesAiParser()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResponse
            {
                Content = """[{"title":"AI Task","dueDate":"2026-06-01","category":"Homework"}]""",
                Provider = AiProviders.Gemini,
                Model = "gemini-2.0-flash",
            });

        var aiParsing = new SyllabusParsingService(
            aiProvider,
            Microsoft.Extensions.Options.Options.Create(new AiOptions { Model = "gemini-2.0-flash" }),
            NullLogger<SyllabusParsingService>.Instance);

        var resolver = CreateResolver(
            aiParsing,
            new AiOptions
            {
                Provider = AiProviders.Gemini,
                ApiKey = "test-key",
                Model = "gemini-2.0-flash",
                EnableFallback = true,
            },
            environment: "Testing");

        var result = await resolver.ParseTasksFromSyllabusTextAsync("syllabus");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle(t => t.Title == "AI Task");
        await aiProvider.Received(1).GenerateTextAsync(Arg.Any<AiTextRequest>(), Arg.Any<CancellationToken>());
    }

    private static SyllabusParsingServiceResolver CreateResolver(
        SyllabusParsingService aiParsing,
        AiOptions aiOptions,
        string environment)
    {
        return new SyllabusParsingServiceResolver(
            Microsoft.Extensions.Options.Options.Create(aiOptions),
            new FakeHostEnvironment(environment),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<SyllabusParsingServiceResolver>.Instance,
            aiParsing,
            new HeuristicSyllabusParsingService());
    }

    private sealed class FakeHostEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
