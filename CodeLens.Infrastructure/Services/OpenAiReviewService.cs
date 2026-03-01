using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.Common.Models;
using CodeLens.Domain.Enums;
using CodeLens.Infrastructure.Settings;

namespace CodeLens.Infrastructure.Services;

/// <summary>
/// Calls the OpenAI Chat Completions API and parses the structured JSON response
/// into an <see cref="AiReviewResult"/>. Uses JSON mode to guarantee valid JSON output.
/// </summary>
internal sealed class OpenAiReviewService : IAiReviewService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly OpenAiSettings _settings;
    private readonly ILogger<OpenAiReviewService> _logger;

    /// <summary>Initialises the service with OpenAI settings and a logger.</summary>
    public OpenAiReviewService(
        IOptions<OpenAiSettings> options,
        ILogger<OpenAiReviewService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AiReviewResult> AnalyseAsync(
        string sourceCode,
        ProgrammingLanguage language,
        CancellationToken cancellationToken = default)
    {
        var client = new ChatClient(_settings.Model, _settings.ApiKey);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(BuildSystemPrompt()),
            new UserChatMessage(BuildUserPrompt(sourceCode, language))
        };

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            MaxOutputTokenCount = _settings.MaxOutputTokens
        };

        _logger.LogInformation(
            "Calling OpenAI model {Model} for {Language} code review ({Chars} chars)",
            _settings.Model, language, sourceCode.Length);

        var completion = await client.CompleteChatAsync(messages, options, cancellationToken);
        var json = completion.Value.Content[0].Text;

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("OpenAI returned an empty response.");

        var parsed = JsonSerializer.Deserialize<AiResponseJson>(json, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI returned an unparseable JSON response.");

        return MapToResult(parsed);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static string BuildSystemPrompt() => """
        You are an expert code reviewer. When given code to review, respond with ONLY a valid
        JSON object — no markdown, no explanations outside the JSON structure.

        The JSON must follow this exact schema:
        {
          "summary": "<2-3 sentence overall assessment of code quality>",
          "overall_score": <integer 0-100, where 100 is perfect>,
          "issues": [
            {
              "severity": "<Info | Minor | Major | Critical>",
              "title": "<5-10 word headline>",
              "description": "<detailed explanation of the issue and its impact>",
              "line_start": <1-based line number or null>,
              "line_end": <1-based line number or null>,
              "suggested_fix": "<corrected code snippet or null>",
              "category": "<Security | Performance | Maintainability | Style | Bug | null>"
            }
          ]
        }

        Severity definitions:
        - Critical: Security vulnerabilities, data loss risk, crashes, injection attacks
        - Major: Logic bugs, significant performance problems, poor design choices
        - Minor: Code smells, naming issues, minor inefficiencies, missing error handling
        - Info: Best practice suggestions, style notes, educational observations
        """;

    private static string BuildUserPrompt(string sourceCode, ProgrammingLanguage language)
    {
        var langName = language.ToString();
        return $"Please review the following {langName} code:\n\n```{langName.ToLower()}\n{sourceCode}\n```";
    }

    private static AiReviewResult MapToResult(AiResponseJson json) =>
        new()
        {
            Summary = json.Summary ?? "No summary provided.",
            OverallScore = Math.Clamp(json.OverallScore, 0, 100),
            Model = "gpt-4o",
            Issues = json.Issues
                .Select(i => new AiReviewIssue
                {
                    Severity = SeverityParser.Parse(i.Severity),
                    Title = i.Title ?? "Untitled issue",
                    Description = i.Description ?? string.Empty,
                    LineStart = i.LineStart,
                    LineEnd = i.LineEnd,
                    SuggestedFix = i.SuggestedFix,
                    Category = i.Category
                })
                .ToList()
        };

    // ──────────────────────────────────────────────────────────────────────────
    // Private JSON deserialization models (internal to this service)
    // ──────────────────────────────────────────────────────────────────────────

    private sealed class AiResponseJson
    {
        [JsonPropertyName("summary")]       public string? Summary { get; init; }
        [JsonPropertyName("overall_score")] public int OverallScore { get; init; }
        [JsonPropertyName("issues")]        public List<AiIssueJson> Issues { get; init; } = [];
    }

    private sealed class AiIssueJson
    {
        [JsonPropertyName("severity")]      public string? Severity { get; init; }
        [JsonPropertyName("title")]         public string? Title { get; init; }
        [JsonPropertyName("description")]   public string? Description { get; init; }
        [JsonPropertyName("line_start")]    public int? LineStart { get; init; }
        [JsonPropertyName("line_end")]      public int? LineEnd { get; init; }
        [JsonPropertyName("suggested_fix")] public string? SuggestedFix { get; init; }
        [JsonPropertyName("category")]      public string? Category { get; init; }
    }
}
