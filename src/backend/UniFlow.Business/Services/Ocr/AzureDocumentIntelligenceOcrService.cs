using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Core;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Configuration;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services.Ocr;

public sealed class AzureDocumentIntelligenceOcrService : IOcrService
{
    private readonly UniFlowOcrOptions _options;

    public AzureDocumentIntelligenceOcrService(IOptions<UniFlowOcrOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Result<string>> ExtractTextAsync(byte[] content, string? contentType, CancellationToken cancellationToken = default)
    {
        if (content.Length == 0)
        {
            return Result<string>.Fail("OCR_EMPTY", "Document content is empty.");
        }

        var azure = _options.Azure;
        if (string.IsNullOrWhiteSpace(azure.Endpoint) || string.IsNullOrWhiteSpace(azure.ApiKey))
        {
            return Result<string>.Fail("OCR_AZURE_CONFIG", "Azure Document Intelligence endpoint or API key is missing.");
        }

        try
        {
            var client = new DocumentIntelligenceClient(new Uri(azure.Endpoint), new AzureKeyCredential(azure.ApiKey));
            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                BinaryData.FromBytes(content),
                cancellationToken).ConfigureAwait(false);

            var text = operation.Value.Content ?? string.Empty;
            return Result<string>.Success(text);
        }
        catch (RequestFailedException ex)
        {
            return Result<string>.Fail("OCR_AZURE_REQUEST", ex.Message);
        }
    }
}
