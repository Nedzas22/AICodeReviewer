using CodeLens.Application.Common.Models;
using CodeLens.Domain.Enums;

namespace CodeLens.Application.Common.Interfaces;

public interface IAiReviewService
{
    Task<AiReviewResult> AnalyseAsync(
        string sourceCode,
        ProgrammingLanguage language,
        CancellationToken cancellationToken = default);
}
