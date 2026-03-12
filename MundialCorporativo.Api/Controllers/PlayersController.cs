using Microsoft.AspNetCore.Mvc;
using MundialCorporativo.Api.Common;
using MundialCorporativo.Api.Contracts;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Idempotency;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;
using MundialCorporativo.Application.Players;
using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Api.Controllers;

[Route("api/players")]
public class PlayersController : IdempotentControllerBase
{
    private readonly ICommandHandler<CreatePlayerCommand, Result<Guid>> _createHandler;
    private readonly ICommandHandler<UpdatePlayerCommand, Result> _updateHandler;
    private readonly ICommandHandler<PatchPlayerCommand, Result> _patchHandler;
    private readonly ICommandHandler<DeletePlayerCommand, Result> _deleteHandler;
    private readonly IQueryHandler<GetPlayerByIdQuery, PlayerDto?> _getByIdHandler;
    private readonly IQueryHandler<ListPlayersQuery, PagedResult<PlayerDto>> _listHandler;
    private readonly IIdempotencyService _idempotencyService;

    public PlayersController(
        ICommandHandler<CreatePlayerCommand, Result<Guid>> createHandler,
        ICommandHandler<UpdatePlayerCommand, Result> updateHandler,
        ICommandHandler<PatchPlayerCommand, Result> patchHandler,
        ICommandHandler<DeletePlayerCommand, Result> deleteHandler,
        IQueryHandler<GetPlayerByIdQuery, PlayerDto?> getByIdHandler,
        IQueryHandler<ListPlayersQuery, PagedResult<PlayerDto>> listHandler,
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
    public async Task<IActionResult> Get([FromQuery] Guid? teamId, [FromQuery] string? name, [FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        var result = await _listHandler.Handle(new ListPlayersQuery(teamId, name, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var player = await _getByIdHandler.Handle(new GetPlayerByIdQuery(id), cancellationToken);
        return player is null ? NotFound() : Ok(player);
    }

    [HttpPost("teams/{teamId:guid}")]
    public Task<IActionResult> Create(Guid teamId, [FromBody] PlayerCreateRequest request, CancellationToken cancellationToken)
    {
        return ExecuteIdempotentPostAsync(
            _idempotencyService,
            async ct =>
            {
                var result = await _createHandler.Handle(new CreatePlayerCommand(teamId, request.FullName, request.JerseyNumber), ct);
                if (result.IsFailure)
                {
                    var statusCode = result.ErrorCode == "NotFound" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
                    return (statusCode, (object)new { error = result.ErrorMessage, code = result.ErrorCode });
                }

                return (StatusCodes.Status201Created, (object)new { id = result.Value });
            },
            cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] PlayerUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateHandler.Handle(new UpdatePlayerCommand(id, request.FullName, request.JerseyNumber), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PlayerPatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _patchHandler.Handle(new PatchPlayerCommand(id, request.FullName, request.JerseyNumber), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deleteHandler.Handle(new DeletePlayerCommand(id), cancellationToken);
        return NoContent();
    }
}
