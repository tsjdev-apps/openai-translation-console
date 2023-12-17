using OpenAITranslationConsole.Models;
using Spectre.Console;
using System.Net.Http.Headers;
using System.Net.Http.Json;

string[] fileExtensions = ["mp3", "mp4", "mpeg", "mpga", "m4a", "ogg", "wav", "webm"];

bool isRunning = true;

while (isRunning)
{
    AnsiConsole.Clear();

    // Create header
    CreateHeader();

    // Get OpenAI Api Key
    string openAIApiKey = GetOpenAIApiKey();

    AnsiConsole.Clear();
    CreateHeader();

    // Get file path
    string filePath = GetFilePath();

    AnsiConsole.Clear();
    CreateHeader();

    // LOGIC
    await AnsiConsole.Status()
        .StartAsync("Translating your audio file...", async ctx =>
        {
            string translationEndpoint = "https://api.openai.com/v1/audio/translations";

            try
            {
                byte[] soundBytes = await File.ReadAllBytesAsync(filePath);

                HttpClient client = new();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", openAIApiKey);

                MultipartFormDataContent content = new()
                {
                    { new ByteArrayContent(soundBytes), "file", Path.GetFileName(filePath) },
                    { new StringContent("whisper-1"), "model" }
                };

                HttpResponseMessage response = 
                    await client.PostAsync(translationEndpoint, content);

                TranslationResponse? translationResponse =
                    await response.Content.ReadFromJsonAsync<TranslationResponse>();

                if (!string.IsNullOrEmpty(translationResponse?.Text))
                {
                    AnsiConsole.MarkupLine($"This is your translated text: [yellow]{translationResponse?.Text}[/]");
                    AnsiConsole.WriteLine();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"Something went wrong: [red]{ex.Message}[/]");
            }
        });

    AnsiConsole.WriteLine();
    isRunning = AnsiConsole.Confirm("Do you want to translate another audio file?", false);
}


/// <summary>
///     Creates the header for the console application.
/// </summary>
static void CreateHeader()
{
    // Create a grid for the header text
    Grid grid = new();
    grid.AddColumn();
    grid.AddRow(new FigletText("Translation").Centered().Color(Color.Red));
    grid.AddRow(Align.Center(new Panel("[red]Sample by Thomas Sebastian Jensen ([link]https://www.tsjdev-apps.de[/])[/]")));

    // Write the grid to the console
    AnsiConsole.Write(grid);
    AnsiConsole.WriteLine();
}

/// <summary>
///     Prompts the user for their OpenAI API key.
/// </summary>
/// <returns>The user's OpenAI API key.</returns>
static string GetOpenAIApiKey()
    => AnsiConsole.Prompt(
        new TextPrompt<string>("Please insert your [yellow]OpenAI API key[/]:")
        .PromptStyle("white")
        .ValidationErrorMessage("[red]Invalid prompt[/]")
        .Validate(prompt =>
        {
            if (prompt.Length < 3)
            {
                return ValidationResult.Error("[red]API key too short[/]");
            }

            if (prompt.Length > 200)
            {
                return ValidationResult.Error("[red]API key too long[/]");
            }

            return ValidationResult.Success();
        }));

/// <summary>
///     Prompts the user for the file path represents the audio file.
/// </summary>
/// <returns>The user's selected file path for the audio file.</returns>
string GetFilePath()
    => AnsiConsole.Prompt(
        new TextPrompt<string>("Please insert the [yellow]path[/] for the audio file:")
        .PromptStyle("white")
        .ValidationErrorMessage("[red]Invalid input[/]")
        .Validate(prompt =>
        {
            if (prompt.Length < 3)
            {
                return ValidationResult.Error("[red]File path too short[/]");
            }

            if (prompt.Length > 256)
            {
                return ValidationResult.Error("[red]File path too long[/]");
            }

            bool isValidExtension = false;
            foreach (string extension in fileExtensions)
            {
                if (prompt.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    isValidExtension = true;
                    break;
                }
            }

            if (!isValidExtension)
            {
                return ValidationResult.Error("[red]File path has wrong file extension.[/]");
            }

            return ValidationResult.Success();
        }));