using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Infrastructure.Persistence.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _dbContext;

    public MatchRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _dbContext.Matches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Match match, CancellationToken cancellationToken)
        => _dbContext.Matches.AddAsync(match, cancellationToken).AsTask();

    public void Remove(Match match)
        => _dbContext.Matches.Remove(match);
}
