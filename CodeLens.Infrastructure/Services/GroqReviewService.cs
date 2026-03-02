using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Application.Common.Models;
using CodeLens.Domain.Enums;
using CodeLens.Infrastructure.Settings;

namespace CodeLens.Infrastructure.Services;

/// <summary>
/// Calls the Groq Chat Completions API (OpenAI-compatible) and parses the structured
/// JSON response into an <see cref="AiReviewResult"/>.
/// </summary>
internal sealed class GroqReviewService : IAiReviewService
{
    private const string BaseUrl = "https://api.groq.com/openai/v1/chat/completions";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly GroqSettings _settings;
    private readonly HttpClient _http;
    private readonly ILogger<GroqReviewService> _logger;

    public GroqReviewService(
        IOptions<GroqSettings> options,
        HttpClient http,
        ILogger<GroqReviewService> logger)
    {
        _settings = options.Value;
        _http = http;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AiReviewResult> AnalyseAsync(
        string sourceCode,
        ProgrammingLanguage language,
        CancellationToken cancellationToken = default)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var requestBody = new GroqRequest
        {
            Model = _settings.Model,
            Messages =
            [
                new GroqMessage { Role = "system", Content = BuildSystemPrompt() },
                new GroqMessage { Role = "user",   Content = BuildUserPrompt(sourceCode, language) }
            ],
            ResponseFormat = new GroqResponseFormat { Type = "json_object" },
            MaxTokens = _settings.MaxOutputTokens
        };

        _logger.LogInformation(
            "Calling Groq model {Model} for {Language} code review ({Chars} chars)",
            _settings.Model, language, sourceCode.Length);

        var response = await _http.PostAsJsonAsync(BaseUrl, requestBody, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Groq API error {Status}: {Body}", (int)response.StatusCode, errorBody);

            var message = (int)response.StatusCode switch
            {
                429 => "Groq API rate limit reached. Please wait a moment and try again.",
                401 => "Groq API key is invalid. Set a new key via: dotnet user-secrets set \"Groq:ApiKey\" \"YOUR_KEY\"",
                _   => $"Groq API returned {(int)response.StatusCode}: {errorBody}"
            };
            throw new InvalidOperationException(message);
        }

        var groqResponse = await response.Content.ReadFromJsonAsync<GroqResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Groq returned an empty response.");

        var json = groqResponse.Choices?[0]?.Message?.Content
            ?? throw new InvalidOperationException("Groq returned no content.");

        var parsed = JsonSerializer.Deserialize<AiResponseJson>(json, JsonOptions)
            ?? throw new InvalidOperationException("Groq returned an unparseable JSON response.");

        return MapToResult(parsed);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Prompt builders
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

    private AiReviewResult MapToResult(AiResponseJson json) =>
        new()
        {
            Summary = json.Summary ?? "No summary provided.",
            OverallScore = Math.Clamp(json.OverallScore, 0, 100),
            Model = _settings.Model,
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
    // Groq API request / response models (OpenAI-compatible)
    // ──────────────────────────────────────────────────────────────────────────

    private sealed class GroqRequest
    {
        [JsonPropertyName("model")]           public string Model { get; init; } = default!;
        [JsonPropertyName("messages")]        public List<GroqMessage> Messages { get; init; } = [];
        [JsonPropertyName("response_format")] public GroqResponseFormat? ResponseFormat { get; init; }
        [JsonPropertyName("max_tokens")]      public int MaxTokens { get; init; }
    }

    private sealed class GroqMessage
    {
        [JsonPropertyName("role")]    public string Role { get; init; } = default!;
        [JsonPropertyName("content")] public string Content { get; init; } = default!;
    }

    private sealed class GroqResponseFormat
    {
        [JsonPropertyName("type")] public string Type { get; init; } = default!;
    }

    private sealed class GroqResponse
    {
        [JsonPropertyName("choices")] public List<GroqChoice>? Choices { get; init; }
    }

    private sealed class GroqChoice
    {
        [JsonPropertyName("message")] public GroqMessage? Message { get; init; }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Review JSON deserialization models
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
