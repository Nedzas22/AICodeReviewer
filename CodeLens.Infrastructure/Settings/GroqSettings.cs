namespace CodeLens.Infrastructure.Settings;

public sealed class GroqSettings
{
    public const string SectionName = "Groq";

    public string ApiKey { get; init; } = default!;
    public string Model { get; init; } = "llama-3.3-70b-versatile";
    public int MaxOutputTokens { get; init; } = 4096;
}
