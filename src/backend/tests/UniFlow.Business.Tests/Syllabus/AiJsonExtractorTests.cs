using FluentAssertions;
using UniFlow.Business.Syllabus;
using Xunit;

namespace UniFlow.Business.Tests.Syllabus;

public sealed class AiJsonExtractorTests
{
    private const string ValidArrayJson =
        """
        [
          {
            "title": "Midterm Exam",
            "description": "Chapters 1-5",
            "dueDate": "2026-05-20T00:00:00Z",
            "category": "Exam"
          }
        ]
        """;

    [Fact]
    public void ExtractJson_PlainJson_ReturnsSameArrayPayload()
    {
        var result = AiJsonExtractor.ExtractJson(ValidArrayJson);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrWhiteSpace();
        result.Data!.TrimStart().Should().StartWith("[");
    }

    [Fact]
    public void ExtractJson_JsonLanguageFence_StripsAndExtractsArray()
    {
        var input = $"Here you go:\n```json\n{ValidArrayJson}\n```\nThanks!";

        var result = AiJsonExtractor.ExtractJson(input);

        result.IsSuccess.Should().BeTrue();
        var parse = AiJsonExtractor.ParseTaskArray(result.Data!);
        parse.IsSuccess.Should().BeTrue();
        parse.Data.Should().ContainSingle(t => t.Title == "Midterm Exam");
    }

    [Fact]
    public void ExtractJson_PlainFence_StripsAndExtractsArray()
    {
        var input = $"```\n{ValidArrayJson}\n```";

        var result = AiJsonExtractor.ExtractJson(input);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ExtractJson_PreambleBeforeArray_StillFindsJson()
    {
        var input = $"The syllabus tasks are:\n{ValidArrayJson}";

        var result = AiJsonExtractor.ExtractJson(input);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ExtractJson_PostambleAfterArray_StillFindsJson()
    {
        var input = $"{ValidArrayJson}\nLet me know if you need more.";

        var result = AiJsonExtractor.ExtractJson(input);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ExtractJson_EmptyInput_ReturnsFailureWithoutSensitiveDetails()
    {
        var result = AiJsonExtractor.ExtractJson("   ");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("SYLLABUS_AI_EMPTY");
        result.Error.Message.ToUpperInvariant().Should().NotContain("API");
        result.Error.Message.ToUpperInvariant().Should().NotContain("KEY");
    }

    [Fact]
    public void ExtractJson_NoJson_ReturnsNotFoundError()
    {
        var result = AiJsonExtractor.ExtractJson("This is only plain commentary with no structured data.");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("SYLLABUS_JSON_NOT_FOUND");
    }

    [Fact]
    public void ParseTaskArray_MissingRequiredTitle_ReturnsValidationError()
    {
        const string json =
            """
            [
              { "title": "", "category": "Exam" }
            ]
            """;

        var extract = AiJsonExtractor.ExtractJson(json);
        var parse = AiJsonExtractor.ParseTaskArray(extract.Data!);

        parse.IsSuccess.Should().BeFalse();
        parse.Error!.Code.Should().Be("SYLLABUS_JSON_VALIDATION");
    }

    [Fact]
    public void ParseTaskArray_ExtraFields_DoesNotThrowAndParsesKnownFields()
    {
        const string json =
            """
            [
              {
                "title": "Quiz 1",
                "category": "Quiz",
                "unexpectedField": "ignored",
                "nested": { "a": 1 }
              }
            ]
            """;

        var extract = AiJsonExtractor.ExtractJson(json);
        var parse = AiJsonExtractor.ParseTaskArray(extract.Data!);

        parse.IsSuccess.Should().BeTrue();
        parse.Data!.Single().Title.Should().Be("Quiz 1");
    }

    [Fact]
    public void ParseTaskArray_InvalidJson_ReturnsParseErrorWithoutRawPayloadInMessage()
    {
        var parse = AiJsonExtractor.ParseTaskArray("[{ not valid json }]");

        parse.IsSuccess.Should().BeFalse();
        parse.Error!.Code.Should().Be("SYLLABUS_JSON");
        parse.Error.Message.ToUpperInvariant().Should().NotContain("APIKEY");
    }

    [Fact]
    public void ExtractJson_SameInput_IsDeterministic()
    {
        var input = $"```json\n{ValidArrayJson}\n```";

        var first = AiJsonExtractor.ExtractJson(input).Data;
        var second = AiJsonExtractor.ExtractJson(input).Data;

        first.Should().Be(second);
    }
}
