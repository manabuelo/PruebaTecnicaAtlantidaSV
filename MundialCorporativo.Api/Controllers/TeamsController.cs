using Microsoft.AspNetCore.Mvc;
using MundialCorporativo.Api.Common;
using MundialCorporativo.Api.Contracts;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Idempotency;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;
using MundialCorporativo.Application.Teams;
using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Api.Controllers;

[Route("api/teams")]
public class TeamsController : IdempotentControllerBase
{
    private readonly ICommandHandler<CreateTeamCommand, Result<Guid>> _createHandler;
    private readonly ICommandHandler<UpdateTeamCommand, Result> _updateHandler;
    private readonly ICommandHandler<PatchTeamCommand, Result> _patchHandler;
    private readonly ICommandHandler<DeleteTeamCommand, Result> _deleteHandler;
    private readonly IQueryHandler<GetTeamByIdQuery, TeamDto?> _getByIdHandler;
    private readonly IQueryHandler<ListTeamsQuery, PagedResult<TeamDto>> _listHandler;
    private readonly IIdempotencyService _idempotencyService;

    public TeamsController(
        ICommandHandler<CreateTeamCommand, Result<Guid>> createHandler,
        ICommandHandler<UpdateTeamCommand, Result> updateHandler,
        ICommandHandler<PatchTeamCommand, Result> patchHandler,
        ICommandHandler<DeleteTeamCommand, Result> deleteHandler,
        IQueryHandler<GetTeamByIdQuery, TeamDto?> getByIdHandler,
        IQueryHandler<ListTeamsQuery, PagedResult<TeamDto>> listHandler,
        IIdempotencyService idempotencyService)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _patchHandler = patchHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _listHandler = listHandler;
        _idempotencyService = idempotencyService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TeamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? name, [FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        var result = await _listHandler.Handle(new ListTeamsQuery(name, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var team = await _getByIdHandler.Handle(new GetTeamByIdQuery(id), cancellationToken);
        return team is null ? NotFound() : Ok(team);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Create([FromBody] TeamCreateRequest request, CancellationToken cancellationToken)
    {
        return ExecuteIdempotentPostAsync(
            _idempotencyService,
            async ct =>
            {
                var result = await _createHandler.Handle(new CreateTeamCommand(request.Name), ct);
                if (result.IsFailure)
                {
                    var payload = new { error = result.ErrorMessage, code = result.ErrorCode };
                    var statusCode = result.ErrorCode == "Conflict" ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
                    return (statusCode, (object)payload);
                }

                return (StatusCodes.Status201Created, (object)new { id = result.Value });
            },
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, [FromBody] TeamUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateHandler.Handle(new UpdateTeamCommand(id, request.Name), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(Guid id, [FromBody] TeamPatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _patchHandler.Handle(new PatchTeamCommand(id, request.Name), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deleteHandler.Handle(new DeleteTeamCommand(id), cancellationToken);
        return NoContent();
    }
}
