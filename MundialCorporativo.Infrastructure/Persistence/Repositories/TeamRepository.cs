using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Infrastructure.Persistence.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly AppDbContext _dbContext;

    public TeamRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _dbContext.Teams.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Team team, CancellationToken cancellationToken)
        => _dbContext.Teams.AddAsync(team, cancellationToken).AsTask();

    public void Remove(Team team)
        => _dbContext.Teams.Remove(team);
}
