using FluentAssertions;
using Microsoft.Extensions.Options;
using UniFlow.Business.Options;
using UniFlow.Business.Services.SyllabusTextStorage;
using Xunit;

namespace UniFlow.Business.Tests.Syllabus;

public sealed class SyllabusTextStoragePolicyTests
{
    private static SyllabusTextStoragePolicy CreatePolicy(
        int maxSourceLength = 4000,
        int maxPreviewJsonLength = 20_000,
        bool storeRawSourceText = false,
        bool storeAiRawResponse = false,
        bool storePreviewJson = true,
        bool normalizeBeforeHashing = true) =>
        new(Microsoft.Extensions.Options.Options.Create(new SyllabusTextStorageOptions
        {
            MaxStoredSourceTextLength = maxSourceLength,
            MaxStoredPreviewJsonLength = maxPreviewJsonLength,
            StoreRawSourceText = storeRawSourceText,
            StoreAiRawResponse = storeAiRawResponse,
            StorePreviewJson = storePreviewJson,
            NormalizeBeforeHashing = normalizeBeforeHashing,
        }));

    [Fact]
    public void Same_input_should_generate_same_sha256_hash()
    {
        var policy = CreatePolicy();
        const string input = "Fall 2026 CS101 syllabus content";

        policy.ComputeSha256Hash(input).Should().Be(policy.ComputeSha256Hash(input));
    }

    [Fact]
    public void Different_input_should_generate_different_sha256_hash()
    {
        var policy = CreatePolicy();

        policy.ComputeSha256Hash("input-a").Should().NotBe(policy.ComputeSha256Hash("input-b"));
    }

    [Fact]
    public void Empty_input_should_generate_deterministic_hash()
    {
        var policy = CreatePolicy();

        policy.ComputeSha256Hash(null).Should().Be(policy.ComputeSha256Hash(string.Empty));
        policy.ComputeSha256Hash(null).Should().HaveLength(64);
        policy.ComputeSha256Hash(null).Should().MatchRegex("^[a-f0-9]+$");
    }

    [Fact]
    public void NormalizeBeforeHashing_line_endings_produce_same_hash()
    {
        var policy = CreatePolicy(normalizeBeforeHashing: true);

        policy.ComputeSha256Hash("line1\r\nline2").Should().Be(policy.ComputeSha256Hash("line1\nline2"));
    }

    [Fact]
    public void Long_source_text_should_be_truncated_when_raw_storage_enabled()
    {
        var policy = CreatePolicy(maxSourceLength: 50, storeRawSourceText: true);
        var longText = new string('B', 200);

        var stored = policy.PrepareSourceTextForStorage(longText);

        stored.Should().HaveLength(50);
        stored.Should().Be(longText[..50]);
    }

    [Fact]
    public void Source_text_should_not_be_stored_when_raw_storage_disabled()
    {
        var policy = CreatePolicy(storeRawSourceText: false);

        policy.PrepareSourceTextForStorage("sensitive full syllabus body").Should().BeNull();
    }

    [Fact]
    public void Preview_json_should_be_truncated()
    {
        var policy = CreatePolicy(maxPreviewJsonLength: 100, storePreviewJson: true);
        var json = new string('J', 500);

        var stored = policy.PreparePreviewJsonForStorage(json);

        stored.Should().HaveLength(100);
    }

    [Fact]
    public void Ai_raw_response_should_not_be_stored_by_default()
    {
        var policy = CreatePolicy(storeAiRawResponse: false);

        policy.PrepareAiRawResponseForStorage("{\"tasks\":[]}").Should().BeNull();
    }

    [Fact]
    public void GetTextLength_ReturnsZeroForNull()
    {
        var policy = CreatePolicy();

        policy.GetTextLength(null).Should().Be(0);
        policy.GetTextLength("abc").Should().Be(3);
    }
}
