using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Infrastructure.Persistence;

namespace MundialCorporativo.Infrastructure.Seed;

public class DatabaseSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseSeeder(AppDbContext dbContext, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await _dbContext.Teams.AnyAsync(cancellationToken))
        {
            return;
        }

        var teamNames = new[] { "Tigres Tech", "Leones Data", "Halcones DevOps", "Toros Cloud" };
        var teams = teamNames.Select(name => new Team(Guid.NewGuid(), name)).ToList();
        await _dbContext.Teams.AddRangeAsync(teams, cancellationToken);

        var players = new List<Player>();
        foreach (var team in teams)
        {
            for (var i = 1; i <= 5; i++)
            {
                players.Add(new Player(Guid.NewGuid(), team.Id, $"{team.Name} Jugador {i}", i));
            }
        }

        await _dbContext.Players.AddRangeAsync(players, cancellationToken);

        var matches = new List<Match>
        {
            new(Guid.NewGuid(), teams[0].Id, teams[1].Id, DateTime.UtcNow.AddDays(-6)),
            new(Guid.NewGuid(), teams[2].Id, teams[3].Id, DateTime.UtcNow.AddDays(-5)),
            new(Guid.NewGuid(), teams[0].Id, teams[2].Id, DateTime.UtcNow.AddDays(-3)),
            new(Guid.NewGuid(), teams[1].Id, teams[3].Id, DateTime.UtcNow.AddDays(1)),
            new(Guid.NewGuid(), teams[0].Id, teams[3].Id, DateTime.UtcNow.AddDays(3)),
            new(Guid.NewGuid(), teams[1].Id, teams[2].Id, DateTime.UtcNow.AddDays(5))
        };

        matches[0].RegisterResult(2, 1);
        matches[1].RegisterResult(0, 0);
        matches[2].RegisterResult(1, 3);

        await _dbContext.Matches.AddRangeAsync(matches, cancellationToken);

        var scorers = players.Take(6).ToList();
        scorers[0].AddGoals(2);
        scorers[1].AddGoals(1);
        scorers[2].AddGoals(1);
        scorers[3].AddGoals(1);
        scorers[4].AddGoals(1);
        scorers[5].AddGoals(1);

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
