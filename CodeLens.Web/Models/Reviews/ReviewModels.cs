using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CodeLens.Web.Models.Reviews;

// ── Enums ────────────────────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Severity { Info = 0, Minor = 1, Major = 2, Critical = 3 }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewStatus { Pending = 0, Processing = 1, Completed = 2, Failed = 3 }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProgrammingLanguage
{
    CSharp = 0, JavaScript = 1, TypeScript = 2, Python = 3,
    Java = 4, Go = 5, Rust = 6, Cpp = 7, Ruby = 8,
    Php = 9, Swift = 10, Kotlin = 11, Sql = 12, Other = 99
}

// ── Extension methods ─────────────────────────────────────────────────────────

public static class EnumExtensions
{
    public static string ToDisplayName(this ProgrammingLanguage lang) => lang switch
    {
        ProgrammingLanguage.CSharp     => "C#",
        ProgrammingLanguage.Cpp        => "C++",
        ProgrammingLanguage.JavaScript => "JavaScript",
        ProgrammingLanguage.TypeScript => "TypeScript",
        ProgrammingLanguage.Php        => "PHP",
        ProgrammingLanguage.Sql        => "SQL",
        _                              => lang.ToString()
    };

    public static string ToDisplayName(this ReviewStatus status) => status switch
    {
        ReviewStatus.Pending    => "Pending",
        ReviewStatus.Processing => "Processing…",
        ReviewStatus.Completed  => "Completed",
        ReviewStatus.Failed     => "Failed",
        _                       => status.ToString()
    };
}

// ── Request ───────────────────────────────────────────────────────────────────

public sealed class SubmitReviewRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(100, ErrorMessage = "Title must not exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Source code is required.")]
    [MinLength(10, ErrorMessage = "Code is too short to review meaningfully.")]
    [MaxLength(50_000, ErrorMessage = "Code must not exceed 50,000 characters.")]
    public string SourceCode { get; set; } = string.Empty;

    public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.CSharp;
}
