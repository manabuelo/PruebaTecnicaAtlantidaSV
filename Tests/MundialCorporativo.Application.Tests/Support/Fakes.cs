using MundialCorporativo.Application.Abstractions.Events;
using MundialCorporativo.Application.Abstractions.Observability;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Domain.Common;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Application.Tests.Support;

internal sealed class InMemoryTeamRepository : ITeamRepository
{
    public readonly Dictionary<Guid, Team> Store = new();

    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Store.TryGetValue(id, out var team);
        return Task.FromResult(team);
    }

    public Task AddAsync(Team team, CancellationToken cancellationToken)
    {
        Store[team.Id] = team;
        return Task.CompletedTask;
    }

    public void Remove(Team team) => Store.Remove(team.Id);
}

internal sealed class InMemoryPlayerRepository : IPlayerRepository
{
    public readonly Dictionary<Guid, Player> Store = new();

    public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Store.TryGetValue(id, out var player);
        return Task.FromResult(player);
    }

    public Task AddAsync(Player player, CancellationToken cancellationToken)
    {
        Store[player.Id] = player;
        return Task.CompletedTask;
    }

    public void Remove(Player player) => Store.Remove(player.Id);
}

internal sealed class InMemoryMatchRepository : IMatchRepository
{
    public readonly Dictionary<Guid, Match> Store = new();

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Store.TryGetValue(id, out var match);
        return Task.FromResult(match);
    }

    public Task AddAsync(Match match, CancellationToken cancellationToken)
    {
        Store[match.Id] = match;
        return Task.CompletedTask;
    }

    public void Remove(Match match) => Store.Remove(match.Id);
}

internal sealed class InMemoryMatchScoreRepository : IMatchScoreRepository
{
    public readonly Dictionary<Guid, MatchScore> Store = new();
    public Task<MatchScore?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Store.TryGetValue(id, out var matchScore);
        return Task.FromResult(matchScore);
    }
    public Task AddAsync(MatchScore matchScore, CancellationToken cancellationToken)
    {
        Store[matchScore.Id] = matchScore;
        return Task.CompletedTask;
    }
    public void Remove(MatchScore matchScore) => Store.Remove(matchScore.Id);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int CommitCount { get; private set; }

    public Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        CommitCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeDomainEventLogger : IDomainEventLogger
{
    public readonly List<DomainEvent> Events = new();

    public Task LogAsync(DomainEvent domainEvent, string traceId, CancellationToken cancellationToken)
    {
        Events.Add(domainEvent);
        return Task.CompletedTask;
    }
}

internal sealed class FakeTraceContext : ITraceContext
{
    public string TraceId => "test-trace";
}
