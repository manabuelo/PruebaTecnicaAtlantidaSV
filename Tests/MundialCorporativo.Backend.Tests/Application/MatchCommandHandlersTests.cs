using Microsoft.Extensions.Logging.Abstractions;
using MundialCorporativo.Application.Matches;
using MundialCorporativo.Backend.Tests.Support;
using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Domain.Enums;

namespace MundialCorporativo.Backend.Tests.Application;

public class MatchCommandHandlersTests
{
    [Fact]
    public async Task CreateMatch_WithSameTeamIds_ShouldReturnValidation()
    {
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var teamRepo = new InMemoryTeamRepository();
        var playerRepo = new InMemoryPlayerRepository();
        var uow = new FakeUnitOfWork();
        var handler = new MatchCommandHandlers(
            matchRepo,
            matchScoreRepo,
            teamRepo,
            playerRepo,
            uow,
            new FakeDomainEventLogger(),
            new FakeTraceContext(),
            NullLogger<MatchCommandHandlers>.Instance);

        var teamId = Guid.NewGuid();
        var result = await handler.Handle(new CreateMatchCommand(teamId, teamId, DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task CreateMatch_WhenAnyTeamMissing_ShouldReturnNotFound()
    {
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var teamRepo = new InMemoryTeamRepository();
        var playerRepo = new InMemoryPlayerRepository();
        var uow = new FakeUnitOfWork();
        var handler = new MatchCommandHandlers(
            matchRepo,
            matchScoreRepo,
            teamRepo,
            playerRepo,
            uow,
            new FakeDomainEventLogger(),
            new FakeTraceContext(),
            NullLogger<MatchCommandHandlers>.Instance);

        var result = await handler.Handle(new CreateMatchCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
    }

    [Fact]
    public async Task RegisterMatchResult_WithNegativeScores_ShouldReturnValidation()
    {
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var teamRepo = new InMemoryTeamRepository();
        var playerRepo = new InMemoryPlayerRepository();
        var uow = new FakeUnitOfWork();
        var handler = new MatchCommandHandlers(
            matchRepo,
            matchScoreRepo,
            teamRepo,
            playerRepo,
            uow,
            new FakeDomainEventLogger(),
            new FakeTraceContext(),
            NullLogger<MatchCommandHandlers>.Instance);

        var result = await handler.Handle(new RegisterMatchResultCommand(Guid.NewGuid(), -1, 0, Array.Empty<RegisterGoalCommand>()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task RegisterMatchResult_WithValidData_ShouldCompleteMatchAddGoalsAndCommit()
    {
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var teamRepo = new InMemoryTeamRepository();
        var playerRepo = new InMemoryPlayerRepository();
        var uow = new FakeUnitOfWork();
        var eventLogger = new FakeDomainEventLogger();
        var homeTeam = new Team(Guid.NewGuid(), "Home");
        var awayTeam = new Team(Guid.NewGuid(), "Away");
        var match = new Match(Guid.NewGuid(), homeTeam.Id, awayTeam.Id, DateTime.UtcNow);
        var scorer = new Player(Guid.NewGuid(), homeTeam.Id, "Striker", 10);

        teamRepo.Store[homeTeam.Id] = homeTeam;
        teamRepo.Store[awayTeam.Id] = awayTeam;
        matchRepo.Store[match.Id] = match;
        playerRepo.Store[scorer.Id] = scorer;

        var handler = new MatchCommandHandlers(
            matchRepo,
            matchScoreRepo,
            teamRepo,
            playerRepo,
            uow,
            eventLogger,
            new FakeTraceContext(),
            NullLogger<MatchCommandHandlers>.Instance);

        var result = await handler.Handle(
            new RegisterMatchResultCommand(match.Id, 3, 1, new[] { new RegisterGoalCommand(scorer.Id, 2, 0) }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.Completed, match.Status);
        Assert.Equal(3, match.HomeScore);
        Assert.Equal(1, match.AwayScore);
        Assert.Equal(2, scorer.GoalsScored);
        Assert.Equal(1, uow.CommitCount);
        Assert.Single(eventLogger.Events);
    }
}
