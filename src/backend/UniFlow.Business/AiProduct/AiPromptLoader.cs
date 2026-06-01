using System.Reflection;

namespace UniFlow.Business.AiProduct;

internal static class AiPromptLoader
{
    internal static string Load(string resourceSuffix)
    {
        var assembly = typeof(AiPromptLoader).Assembly;
        var name = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded resource {resourceSuffix} was not found.");

        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Could not open embedded resource {name}.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
