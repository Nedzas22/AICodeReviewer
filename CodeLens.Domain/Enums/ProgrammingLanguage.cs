namespace CodeLens.Domain.Enums;

/// <summary>
/// Supported programming languages for code review submission.
/// Values are stable integers used for database persistence.
/// </summary>
public enum ProgrammingLanguage
{
    /// <summary>C# (.cs files)</summary>
    CSharp = 0,

    /// <summary>JavaScript (.js files)</summary>
    JavaScript = 1,

    /// <summary>TypeScript (.ts files)</summary>
    TypeScript = 2,

    /// <summary>Python (.py files)</summary>
    Python = 3,

    /// <summary>Java (.java files)</summary>
    Java = 4,

    /// <summary>Go (.go files)</summary>
    Go = 5,

    /// <summary>Rust (.rs files)</summary>
    Rust = 6,

    /// <summary>C++ (.cpp / .h files)</summary>
    Cpp = 7,

    /// <summary>Ruby (.rb files)</summary>
    Ruby = 8,

    /// <summary>PHP (.php files)</summary>
    Php = 9,

    /// <summary>Swift (.swift files)</summary>
    Swift = 10,

    /// <summary>Kotlin (.kt files)</summary>
    Kotlin = 11,

    /// <summary>SQL (.sql files)</summary>
    Sql = 12,

    /// <summary>Any language not explicitly listed above.</summary>
    Other = 99
}
