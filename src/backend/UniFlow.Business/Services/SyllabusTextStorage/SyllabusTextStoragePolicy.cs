using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using UniFlow.Business.Options;

namespace UniFlow.Business.Services.SyllabusTextStorage;

public sealed class SyllabusTextStoragePolicy : ISyllabusTextStoragePolicy
{
    private readonly SyllabusTextStorageOptions _options;

    public SyllabusTextStoragePolicy(IOptions<SyllabusTextStorageOptions> options)
    {
        _options = options.Value;
    }

    public string ComputeSha256Hash(string? input)
    {
        var normalized = NormalizeForHash(input);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public string? PrepareSourceTextForStorage(string? sourceText)
    {
        if (!_options.StoreRawSourceText || string.IsNullOrEmpty(sourceText))
        {
            return null;
        }

        return Truncate(sourceText, _options.MaxStoredSourceTextLength);
    }

    public string? PreparePreviewJsonForStorage(string? previewJson)
    {
        if (!_options.StorePreviewJson || string.IsNullOrEmpty(previewJson))
        {
            return null;
        }

        return Truncate(previewJson, _options.MaxStoredPreviewJsonLength);
    }

    public string? PrepareAiRawResponseForStorage(string? aiRawResponse)
    {
        if (!_options.StoreAiRawResponse || string.IsNullOrEmpty(aiRawResponse))
        {
            return null;
        }

        return Truncate(aiRawResponse, _options.MaxStoredPreviewJsonLength);
    }

    public int GetTextLength(string? input) => input?.Length ?? 0;

    private string NormalizeForHash(string? input)
    {
        var value = input ?? string.Empty;
        if (!_options.NormalizeBeforeHashing)
        {
            return value;
        }

        return value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
    }

    private static string Truncate(string value, int maxLength)
    {
        if (maxLength <= 0 || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
