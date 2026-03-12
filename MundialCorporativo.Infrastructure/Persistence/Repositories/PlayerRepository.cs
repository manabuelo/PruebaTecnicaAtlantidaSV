using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Infrastructure.Persistence.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly AppDbContext _dbContext;

    public PlayerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _dbContext.Players.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Player player, CancellationToken cancellationToken)
        => _dbContext.Players.AddAsync(player, cancellationToken).AsTask();

    public void Remove(Player player)
        => _dbContext.Players.Remove(player);
}
