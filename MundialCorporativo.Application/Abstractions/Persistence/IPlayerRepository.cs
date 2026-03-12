using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Application.Abstractions.Persistence;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Player player, CancellationToken cancellationToken);
    void Remove(Player player);
}
