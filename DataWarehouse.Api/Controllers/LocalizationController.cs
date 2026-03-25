using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class LocalizationController : ControllerBase
{
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en",
        "ar"
    };

    private readonly IWebHostEnvironment environment;

    public LocalizationController(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    [HttpGet("{language}")]
    public async Task<IActionResult> GetTranslations(string language, CancellationToken cancellationToken)
    {
        var normalizedLanguage = NormalizeLanguage(language);
        var translationFilePath = Path.Combine(
            environment.ContentRootPath,
            "Translations",
            $"{normalizedLanguage}.json");
        var literalTranslationFilePath = Path.Combine(
            environment.ContentRootPath,
            "Translations",
            $"literal-{normalizedLanguage}.json");

        if (!System.IO.File.Exists(translationFilePath))
        {
            return NotFound(new
            {
                message = $"Translation file was not found for language '{normalizedLanguage}'."
            });
        }

        await using var stream = System.IO.File.OpenRead(translationFilePath);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var data = document.RootElement.Clone();
        var literalTranslations = await LoadLiteralTranslationsAsync(literalTranslationFilePath, cancellationToken);

        return Ok(new LocalizationResponse(
            normalizedLanguage,
            data,
            CountLeafNodes(data),
            literalTranslations));
    }

    private static string NormalizeLanguage(string? language)
    {
        var normalized = string.IsNullOrWhiteSpace(language)
            ? "en"
            : language.Trim().ToLowerInvariant().Split('-')[0];

        return SupportedLanguages.Contains(normalized) ? normalized : "en";
    }

    private static int CountLeafNodes(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Sum(property => CountLeafNodes(property.Value)),
            JsonValueKind.Array => element.EnumerateArray().Sum(CountLeafNodes),
            _ => 1
        };
    }

    private static async Task<Dictionary<string, string>?> LoadLiteralTranslationsAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        if (!System.IO.File.Exists(filePath))
        {
            return null;
        }

        await using var stream = System.IO.File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
            stream,
            cancellationToken: cancellationToken);
    }

    public sealed record LocalizationResponse(
        string Language,
        JsonElement Data,
        int Count,
        Dictionary<string, string>? LiteralTranslations);
}
