namespace CodeLens.Domain.Enums;

/// <summary>
/// Lifecycle status of a <see cref="Entities.CodeReview"/>.
/// Transitions: Pending → Processing → Completed | Failed.
/// </summary>
public enum ReviewStatus
{
    /// <summary>Review has been submitted and is waiting to be picked up by the AI engine.</summary>
    Pending = 0,

    /// <summary>The AI engine is currently analysing the code.</summary>
    Processing = 1,

    /// <summary>Review completed successfully; results are available.</summary>
    Completed = 2,

    /// <summary>Review failed due to an AI service error or timeout.</summary>
    Failed = 3
}
