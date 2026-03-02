using CodeLens.Application.Features.Reviews.Commands;
using CodeLens.Application.Features.Reviews.Queries;

namespace CodeLens.Api.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
[Produces("application/json")]
public sealed class ReviewsController : ControllerBase
{
    private readonly ISender _sender;

    public ReviewsController(ISender sender) => _sender = sender;

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
