using Microsoft.Extensions.Logging;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Events;
using MundialCorporativo.Application.Abstractions.Observability;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Common;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Application.Matches;

public record CreateMatchCommand(Guid HomeTeamId, Guid AwayTeamId, DateTime MatchDateUtc) : ICommand<Result<Guid>>;
public record RegisterMatchResultCommand(Guid MatchId, int HomeScore, int AwayScore, IReadOnlyCollection<RegisterGoalCommand> Goals) : ICommand<Result>;
public record PatchMatchCommand(Guid MatchId, DateTime? MatchDateUtc, bool? Cancelled) : ICommand<Result>;
public record DeleteMatchCommand(Guid MatchId) : ICommand<Result>;
public record RegisterGoalCommand(Guid PlayerId, int Goals);

public class MatchCommandHandlers :
    ICommandHandler<CreateMatchCommand, Result<Guid>>,
    ICommandHandler<RegisterMatchResultCommand, Result>,
    ICommandHandler<PatchMatchCommand, Result>,
    ICommandHandler<DeleteMatchCommand, Result>
{
    private readonly IMatchRepository _matchRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventLogger _domainEventLogger;
    private readonly ITraceContext _traceContext;
    private readonly ILogger<MatchCommandHandlers> _logger;

    public MatchCommandHandlers(
        IMatchRepository matchRepository,
        ITeamRepository teamRepository,
        IPlayerRepository playerRepository,
        IUnitOfWork unitOfWork,
        IDomainEventLogger domainEventLogger,
        ITraceContext traceContext,
        ILogger<MatchCommandHandlers> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _domainEventLogger = domainEventLogger;
        _traceContext = traceContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateMatchCommand command, CancellationToken cancellationToken)
    {
        if (command.HomeTeamId == command.AwayTeamId)
        {
            return Result<Guid>.Failure("Un equipo no puede jugar contra si mismo.", "Validation");
        }

        var homeTeam = await _teamRepository.GetByIdAsync(command.HomeTeamId, cancellationToken);
        var awayTeam = await _teamRepository.GetByIdAsync(command.AwayTeamId, cancellationToken);
        if (homeTeam is null || awayTeam is null)
        {
            return Result<Guid>.Failure("Uno o ambos equipos no existen.", "NotFound");
        }

        var match = new Match(Guid.NewGuid(), command.HomeTeamId, command.AwayTeamId, command.MatchDateUtc);
        await _matchRepository.AddAsync(match, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(match.Id);
    }

    public async Task<Result> Handle(RegisterMatchResultCommand command, CancellationToken cancellationToken)
    {
        if (command.HomeScore < 0 || command.AwayScore < 0)
        {
            return Result.Failure("Los goles no pueden ser negativos.", "Validation");
        }

        var match = await _matchRepository.GetByIdAsync(command.MatchId, cancellationToken);
        if (match is null)
        {
            return Result.Failure("Partido no encontrado.", "NotFound");
        }

        match.RegisterResult(command.HomeScore, command.AwayScore);

        foreach (var goal in command.Goals.Where(x => x.Goals > 0))
        {
            var player = await _playerRepository.GetByIdAsync(goal.PlayerId, cancellationToken);
            if (player is not null)
            {
                player.AddGoals(goal.Goals);
            }
        }

        foreach (var domainEvent in match.DomainEvents)
        {
            await _domainEventLogger.LogAsync(domainEvent, _traceContext.TraceId, cancellationToken);
        }

        match.ClearDomainEvents();
        await _unitOfWork.CommitAsync(cancellationToken);
        _logger.LogInformation("Match result registered {MatchId} {HomeScore}-{AwayScore}", command.MatchId, command.HomeScore, command.AwayScore);

        return Result.Success();
    }

    public async Task<Result> Handle(PatchMatchCommand command, CancellationToken cancellationToken)
    {
        var match = await _matchRepository.GetByIdAsync(command.MatchId, cancellationToken);
        if (match is null)
        {
            return Result.Failure("Partido no encontrado.", "NotFound");
        }

        if (command.MatchDateUtc.HasValue)
        {
            match.Reschedule(command.MatchDateUtc.Value);
        }

        if (command.Cancelled == true)
        {
            match.Cancel();
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteMatchCommand command, CancellationToken cancellationToken)
    {
        var match = await _matchRepository.GetByIdAsync(command.MatchId, cancellationToken);
        if (match is null)
        {
            return Result.Success();
        }

        _matchRepository.Remove(match);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
