using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Application.Abstractions.Persistence;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Team team, CancellationToken cancellationToken);
    void Remove(Team team);
}
