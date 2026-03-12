using Microsoft.Extensions.Logging;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Events;
using MundialCorporativo.Application.Abstractions.Observability;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Common;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Application.Teams;

public record CreateTeamCommand(string Name) : ICommand<Result<Guid>>;
public record UpdateTeamCommand(Guid TeamId, string Name) : ICommand<Result>;
public record PatchTeamCommand(Guid TeamId, string? Name) : ICommand<Result>;
public record DeleteTeamCommand(Guid TeamId) : ICommand<Result>;

public class TeamCommandHandlers :
    ICommandHandler<CreateTeamCommand, Result<Guid>>,
    ICommandHandler<UpdateTeamCommand, Result>,
    ICommandHandler<PatchTeamCommand, Result>,
    ICommandHandler<DeleteTeamCommand, Result>
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventLogger _domainEventLogger;
    private readonly ITraceContext _traceContext;
    private readonly ILogger<TeamCommandHandlers> _logger;

    public TeamCommandHandlers(
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        IDomainEventLogger domainEventLogger,
        ITraceContext traceContext,
        ILogger<TeamCommandHandlers> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _domainEventLogger = domainEventLogger;
        _traceContext = traceContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<Guid>.Failure("El nombre del equipo es requerido.", "Validation");
        }

        var team = new Team(Guid.NewGuid(), command.Name.Trim());
        await _teamRepository.AddAsync(team, cancellationToken);

        foreach (var domainEvent in team.DomainEvents)
        {
            await _domainEventLogger.LogAsync(domainEvent, _traceContext.TraceId, cancellationToken);
        }

        team.ClearDomainEvents();
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Team created {TeamId} {TeamName}", team.Id, team.Name);
        return Result<Guid>.Success(team.Id);
    }

    public async Task<Result> Handle(UpdateTeamCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure("El nombre del equipo es requerido.", "Validation");
        }

        var team = await _teamRepository.GetByIdAsync(command.TeamId, cancellationToken);
        if (team is null)
        {
            return Result.Failure("Equipo no encontrado.", "NotFound");
        }

        team.Rename(command.Name.Trim());
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(PatchTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetByIdAsync(command.TeamId, cancellationToken);
        if (team is null)
        {
            return Result.Failure("Equipo no encontrado.", "NotFound");
        }

        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            team.Rename(command.Name.Trim());
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetByIdAsync(command.TeamId, cancellationToken);
        if (team is null)
        {
            return Result.Success();
        }

        _teamRepository.Remove(team);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
