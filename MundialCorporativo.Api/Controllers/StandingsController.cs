using Microsoft.AspNetCore.Mvc;
using MundialCorporativo.Api.Contracts;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;
using MundialCorporativo.Application.Standings;

namespace MundialCorporativo.Api.Controllers;

[ApiController]
[Route("api/standings")]
public class StandingsController : ControllerBase
{
    private readonly IQueryHandler<GetStandingsQuery, PagedResult<StandingDto>> _standingsHandler;
    private readonly IQueryHandler<GetTopScorersQuery, PagedResult<TopScorerDto>> _topScorersHandler;

    public StandingsController(
        IQueryHandler<GetStandingsQuery, PagedResult<StandingDto>> standingsHandler,
        IQueryHandler<GetTopScorersQuery, PagedResult<TopScorerDto>> topScorersHandler)
    {
        _standingsHandler = standingsHandler;
        _topScorersHandler = topScorersHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        var result = await _standingsHandler.Handle(new GetStandingsQuery(request.PageNumber, request.PageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-scorers")]
    public async Task<IActionResult> TopScorers([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        var result = await _topScorersHandler.Handle(new GetTopScorersQuery(request.PageNumber, request.PageSize), cancellationToken);
        return Ok(result);
    }
}
