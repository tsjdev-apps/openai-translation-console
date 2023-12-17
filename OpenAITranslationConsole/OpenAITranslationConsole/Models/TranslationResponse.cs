using System.Text.Json.Serialization;

namespace OpenAITranslationConsole.Models;

/// <summary>
///     Represents a response of the translation
///     request.
/// </summary>
/// <param name="Text"> The translated text of the audio file. </param>
internal record TranslationResponse(
    [property: JsonPropertyName("text")] string? Text);
