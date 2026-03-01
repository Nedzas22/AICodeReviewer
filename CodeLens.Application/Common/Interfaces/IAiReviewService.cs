using CodeLens.Application.Common.Models;
using CodeLens.Domain.Enums;

namespace CodeLens.Application.Common.Interfaces;

/// <summary>
/// Contract for the AI-powered code analysis engine.
/// The implementation (OpenAI, Claude, etc.) lives in the Infrastructure layer.
/// </summary>
public interface IAiReviewService
{
    /// <summary>
    /// Submits code to the configured AI model and returns a structured review result.
    /// </summary>
    /// <param name="sourceCode">The full source code to be analysed.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>
    /// A fully parsed <see cref="AiReviewResult"/> containing the summary, score, and all issues.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the AI service returns an unparseable or empty response.
    /// </exception>
    Task<AiReviewResult> AnalyseAsync(
        string sourceCode,
        ProgrammingLanguage language,
        CancellationToken cancellationToken = default);
}
