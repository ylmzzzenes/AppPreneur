using FluentAssertions;
using Microsoft.Extensions.Options;
using UniFlow.Business.Configuration;
using UniFlow.Business.Contracts.Syllabus;
using UniFlow.Business.Services;
using Xunit;

namespace UniFlow.Business.Tests.Syllabus;

public sealed class SyllabusTextStoragePolicyTests
{
    private static SyllabusTextStoragePolicy CreatePolicy(
        int maxSourceLength = 4000,
        int maxPreviewJsonLength = 20_000,
        bool storeRawSourceText = false) =>
        new(Options.Create(new SyllabusTextStorageOptions
        {
            MaxStoredSourceTextLength = maxSourceLength,
            MaxStoredPreviewJsonLength = maxPreviewJsonLength,
            StoreRawSourceText = storeRawSourceText,
        }));

    [Fact]
    public void ComputeSourceTextHash_SameInput_ProducesSameSha256Hex()
    {
        var policy = CreatePolicy();
        const string input = "  Fall 2026 CS101 syllabus content  ";

        var first = policy.ComputeSourceTextHash(input);
        var second = policy.ComputeSourceTextHash(input);

        first.Should().Be(second);
        first.Should().HaveLength(64);
        first.Should().MatchRegex("^[0-9A-F]+$");
    }

    [Fact]
    public void ComputeSourceTextHash_EmptyOrNullInput_IsSafeAndDeterministic()
    {
        var policy = CreatePolicy();

        policy.ComputeSourceTextHash(null).Should().Be(policy.ComputeSourceTextHash(string.Empty));
        policy.ComputeSourceTextHash("   ").Should().Be(policy.ComputeSourceTextHash(string.Empty));
    }

    [Fact]
    public void BuildSourcePreview_LongText_IsTruncatedToConfiguredLength()
    {
        var policy = CreatePolicy(maxSourceLength: 100);
        var longText = new string('A', 500);

        var preview = policy.BuildSourcePreview(longText);

        preview.Should().HaveLength(100);
        preview.Should().Be(longText[..100]);
    }

    [Fact]
    public void PrepareStoredSourceText_WhenDisabled_ReturnsNull()
    {
        var policy = CreatePolicy(storeRawSourceText: false);

        policy.PrepareStoredSourceText("sensitive full syllabus body").Should().BeNull();
    }

    [Fact]
    public void PrepareStoredSourceText_WhenEnabled_ReturnsTruncatedText()
    {
        var policy = CreatePolicy(maxSourceLength: 50, storeRawSourceText: true);
        var longText = new string('B', 200);

        var stored = policy.PrepareStoredSourceText(longText);

        stored.Should().HaveLength(50);
        stored.Should().Be(longText[..50]);
    }

    [Fact]
    public void SerializePreview_ExceedsMaxLength_TruncatesDetectedItems()
    {
        var policy = CreatePolicy(maxPreviewJsonLength: 600);
        var items = Enumerable.Range(0, 40)
            .Select(i => new SyllabusDetectedItemDto
            {
                Title = $"Assignment {i} with a long title for payload size",
                Description = "Weekly reading and problem set details",
                Type = "Homework",
            })
            .ToList();

        var json = policy.SerializePreview("summary", items);

        json.Length.Should().BeLessOrEqualTo(600);
        json.Should().Contain("Assignment");
    }
}
