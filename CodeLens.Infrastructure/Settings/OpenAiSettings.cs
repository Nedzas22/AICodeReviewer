namespace CodeLens.Infrastructure.Settings;

/// <summary>
/// Strongly-typed configuration for the OpenAI API client.
/// Bind to <c>appsettings.json</c> section <c>"OpenAI"</c>.
/// </summary>
public sealed class OpenAiSettings
{
    /// <summary>The <c>appsettings.json</c> section key for this settings class.</summary>
    public const string SectionName = "OpenAI";

    /// <summary>Gets the OpenAI API key. Keep this in user-secrets or environment variables — never in source control.</summary>
    public string ApiKey { get; init; } = default!;

    /// <summary>Gets the model to use for reviews (e.g., "gpt-4o", "gpt-4-turbo").</summary>
    public string Model { get; init; } = "gpt-4o";

    /// <summary>Gets the maximum number of output tokens per review call. Defaults to 4096.</summary>
    public int MaxOutputTokens { get; init; } = 4096;
}
