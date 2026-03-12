using Microsoft.AspNetCore.Mvc;
using MundialCorporativo.Api.Common;
using MundialCorporativo.Api.Contracts;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Idempotency;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;
using MundialCorporativo.Application.Matches;
using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Api.Controllers;

[Route("api/matches")]
public class MatchesController : IdempotentControllerBase
{
    private readonly ICommandHandler<CreateMatchCommand, Result<Guid>> _createHandler;
    private readonly ICommandHandler<RegisterMatchResultCommand, Result> _registerResultHandler;
    private readonly ICommandHandler<PatchMatchCommand, Result> _patchHandler;
    private readonly ICommandHandler<DeleteMatchCommand, Result> _deleteHandler;
    private readonly IQueryHandler<GetMatchByIdQuery, MatchDto?> _getByIdHandler;
    private readonly IQueryHandler<ListMatchesQuery, PagedResult<MatchDto>> _listHandler;
    private readonly IIdempotencyService _idempotencyService;

    public MatchesController(
        ICommandHandler<CreateMatchCommand, Result<Guid>> createHandler,
        ICommandHandler<RegisterMatchResultCommand, Result> registerResultHandler,
        ICommandHandler<PatchMatchCommand, Result> patchHandler,
        ICommandHandler<DeleteMatchCommand, Result> deleteHandler,
        IQueryHandler<GetMatchByIdQuery, MatchDto?> getByIdHandler,
        IQueryHandler<ListMatchesQuery, PagedResult<MatchDto>> listHandler,
        IIdempotencyService idempotencyService)
    {
        _createHandler = createHandler;
        _registerResultHandler = registerResultHandler;
        _patchHandler = patchHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _listHandler = listHandler;
        _idempotencyService = idempotencyService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Guid? teamId,
        [FromQuery] DateTime? dateFromUtc,
        [FromQuery] DateTime? dateToUtc,
        [FromQuery] string? status,
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _listHandler.Handle(
            new ListMatchesQuery(teamId, dateFromUtc, dateToUtc, status, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var match = await _getByIdHandler.Handle(new GetMatchByIdQuery(id), cancellationToken);
        return match is null ? NotFound() : Ok(match);
    }

    [HttpPost]
    public Task<IActionResult> Create([FromBody] MatchCreateRequest request, CancellationToken cancellationToken)
    {
        return ExecuteIdempotentPostAsync(
            _idempotencyService,
            async ct =>
            {
                var result = await _createHandler.Handle(new CreateMatchCommand(request.HomeTeamId, request.AwayTeamId, request.MatchDateUtc), ct);
                if (result.IsFailure)
                {
                    var statusCode = result.ErrorCode == "NotFound" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
                    return (statusCode, (object)new { error = result.ErrorMessage, code = result.ErrorCode });
                }

                return (StatusCodes.Status201Created, (object)new { id = result.Value });
            },
            cancellationToken);
    }

    [HttpPost("{id:guid}/result")]
    public Task<IActionResult> RegisterResult(Guid id, [FromBody] RegisterMatchResultRequest request, CancellationToken cancellationToken)
    {
        return ExecuteIdempotentPostAsync(
            _idempotencyService,
            async ct =>
            {
                var goals = request.Goals?.Select(x => new RegisterGoalCommand(x.PlayerId, x.Goals)).ToList() ?? new List<RegisterGoalCommand>();
                var result = await _registerResultHandler.Handle(new RegisterMatchResultCommand(id, request.HomeScore, request.AwayScore, goals), ct);
                if (result.IsFailure)
                {
                    var statusCode = result.ErrorCode == "NotFound" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
                    return (statusCode, (object)new { error = result.ErrorMessage, code = result.ErrorCode });
                }

                return (StatusCodes.Status200OK, (object)new { message = "Resultado registrado." });
            },
            cancellationToken);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] MatchPatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _patchHandler.Handle(new PatchMatchCommand(id, request.MatchDateUtc, request.Cancelled), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deleteHandler.Handle(new DeleteMatchCommand(id), cancellationToken);
        return NoContent();
    }
}
