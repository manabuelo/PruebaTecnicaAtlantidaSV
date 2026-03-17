using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MundialCorporativo.Infrastructure.Persistence.Repositories;

public class MatchScoreRepository : IMatchScoreRepository
{
    private readonly AppDbContext _dbContext;
    public MatchScoreRepository(AppDbContext dbContext)
    {
        _dbContext=dbContext;
    }

    public Task<MatchScore?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _dbContext.MatchScores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(MatchScore matchScore, CancellationToken cancellationToken)
        => _dbContext.MatchScores.AddAsync(matchScore, cancellationToken).AsTask();

    public void Remove(MatchScore matchScore)
        => _dbContext.MatchScores.Remove(matchScore);
}

