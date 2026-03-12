using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Application.Abstractions.Persistence;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Match match, CancellationToken cancellationToken);
    void Remove(Match match);
}
