using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Common;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Application.Players;

public record CreatePlayerCommand(Guid TeamId, string FullName, int JerseyNumber) : ICommand<Result<Guid>>;
public record UpdatePlayerCommand(Guid PlayerId, string FullName, int JerseyNumber) : ICommand<Result>;
public record PatchPlayerCommand(Guid PlayerId, string? FullName, int? JerseyNumber) : ICommand<Result>;
public record DeletePlayerCommand(Guid PlayerId) : ICommand<Result>;

public class PlayerCommandHandlers :
    ICommandHandler<CreatePlayerCommand, Result<Guid>>,
    ICommandHandler<UpdatePlayerCommand, Result>,
    ICommandHandler<PatchPlayerCommand, Result>,
    ICommandHandler<DeletePlayerCommand, Result>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlayerCommandHandlers(
        IPlayerRepository playerRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork)
    {
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreatePlayerCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.FullName) || command.JerseyNumber <= 0)
        {
            return Result<Guid>.Failure("Datos de jugador invalidos.", "Validation");
        }

        var team = await _teamRepository.GetByIdAsync(command.TeamId, cancellationToken);
        if (team is null)
        {
            return Result<Guid>.Failure("Equipo no encontrado.", "NotFound");
        }

        var player = new Player(Guid.NewGuid(), command.TeamId, command.FullName.Trim(), command.JerseyNumber);
        await _playerRepository.AddAsync(player, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(player.Id);
    }

    public async Task<Result> Handle(UpdatePlayerCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.FullName) || command.JerseyNumber <= 0)
        {
            return Result.Failure("Datos de jugador invalidos.", "Validation");
        }

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result.Failure("Jugador no encontrado.", "NotFound");
        }

        player.Update(command.FullName.Trim(), command.JerseyNumber);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(PatchPlayerCommand command, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result.Failure("Jugador no encontrado.", "NotFound");
        }

        var fullName = string.IsNullOrWhiteSpace(command.FullName) ? player.FullName : command.FullName.Trim();
        var jerseyNumber = command.JerseyNumber.GetValueOrDefault(player.JerseyNumber);
        if (jerseyNumber <= 0)
        {
            return Result.Failure("Numero de camiseta invalido.", "Validation");
        }

        player.Update(fullName, jerseyNumber);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeletePlayerCommand command, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result.Success();
        }

        _playerRepository.Remove(player);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
