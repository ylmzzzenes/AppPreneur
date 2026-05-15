using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UniFlow.API.Contracts;

namespace UniFlow.API.Infrastructure;

/// <summary>
/// Ensures multipart/form-data endpoints with <see cref="IFormFile"/> properties generate valid OpenAPI for Swagger UI.
/// </summary>
internal sealed class MultipartFormDataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var consumesMultipart = context.MethodInfo
            .GetCustomAttributes(typeof(ConsumesAttribute), inherit: true)
            .Cast<ConsumesAttribute>()
            .SelectMany(a => a.ContentTypes)
            .Any(contentType => contentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase));

        if (!consumesMultipart)
        {
            return;
        }

        var formType = context.MethodInfo.GetParameters()
            .FirstOrDefault(p => p.GetCustomAttributes(typeof(FromFormAttribute), inherit: true).Length > 0)
            ?.ParameterType;

        if (formType is null || formType == typeof(string) || !formType.IsClass)
        {
            return;
        }

        var properties = new Dictionary<string, OpenApiSchema>();
        var required = new HashSet<string>();

        foreach (var property in formType.GetProperties())
        {
            var name = JsonNamingPolicy.CamelCase.ConvertName(property.Name);

            if (property.PropertyType == typeof(IFormFile))
            {
                properties[name] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = "Syllabus document (PDF, JPEG, or PNG; max 10 MB).",
                };
                required.Add(name);
                continue;
            }

            properties[name] = new OpenApiSchema
            {
                Type = "string",
                Description = property.Name switch
                {
                    nameof(SyllabusIngestFormRequest.CourseCode) => "Course code (e.g. CS101).",
                    nameof(SyllabusIngestFormRequest.CourseTitle) => "Course title (e.g. Calculus).",
                    _ => null,
                },
            };

            required.Add(name);
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties,
                        Required = required,
                    },
                },
            },
        };

        operation.Parameters.Clear();
    }
}
