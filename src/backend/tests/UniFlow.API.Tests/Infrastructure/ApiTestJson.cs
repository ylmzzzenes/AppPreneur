using System.Text.Json;
using System.Text.Json.Serialization;

namespace UniFlow.API.Tests.Infrastructure;

internal static class ApiTestJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };
}
