using CodeLens.Application.Features.Reviews.Commands;
using CodeLens.Application.Features.Reviews.Queries;

namespace CodeLens.Api.Controllers;

/// <summary>
/// Manages code review submissions, history, detail retrieval, and re-runs.
/// All endpoints require a valid JWT (<c>[Authorize]</c> applied at controller level).
/// </summary>
[ApiController]
[Route("api/reviews")]
[Authorize]
[Produces("application/json")]
public sealed class ReviewsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises the controller with the MediatR sender.</summary>
    public ReviewsController(ISender sender) => _sender = sender;

    /// <summary>
    /// Submits source code for AI-powered review.
    /// The call is synchronous — it waits for the OpenAI response before returning.
    /// Typical response time: 5–30 seconds depending on code length and AI model.
    /// </summary>
    /// <param name="command">Title, source code, and programming language.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="201">Review created and AI analysis complete (or failed gracefully).</response>
    /// <response code="400">Validation failure (code too short/long, missing title, etc.).</response>
    /// <response code="401">Missing or invalid JWT.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CodeReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitReviewCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Returns a paginated list of the authenticated user's review history, newest first.
    /// </summary>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <param name="pageSize">Items per page, 1–50 (default: 10).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Paged review list returned.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CodeReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetReviewHistoryQuery(page, pageSize), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves full review details including all issues.
    /// Users can only access their own reviews.
    /// </summary>
    /// <param name="id">The review's GUID.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Review detail returned.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    /// <response code="403">The review belongs to a different user.</response>
    /// <response code="404">No review found with the given ID.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CodeReviewDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetReviewByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Re-runs the AI analysis on an existing review, replacing all previous results.
    /// Only the review's owner may trigger a re-run.
    /// </summary>
    /// <param name="id">The review's GUID.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Re-run completed — updated detail returned.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    /// <response code="403">The review belongs to a different user.</response>
    /// <response code="404">No review found with the given ID.</response>
    [HttpPost("{id:guid}/rerun")]
    [ProducesResponseType(typeof(CodeReviewDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReRun(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReRunReviewCommand(id), cancellationToken);
        return Ok(result);
    }
}
