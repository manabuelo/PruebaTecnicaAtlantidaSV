using Microsoft.Extensions.Logging.Abstractions;
using MundialCorporativo.Application.Matches;
using MundialCorporativo.Application.Tests.Support;
using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Domain.Enums;

namespace MundialCorporativo.Application.Tests.Matches;

public class MatchCommandHandlersTests
{
    private static MatchCommandHandlers Build(
        InMemoryMatchRepository matchRepo,
        InMemoryMatchScoreRepository matchScoreRepo,
        InMemoryTeamRepository teamRepo,
        InMemoryPlayerRepository? playerRepo = null,
        FakeUnitOfWork? uow = null,
        FakeDomainEventLogger? eventLogger = null)
        => new(
            matchRepo,
            matchScoreRepo,
            teamRepo,
            playerRepo ?? new InMemoryPlayerRepository(),
            uow ?? new FakeUnitOfWork(),
            eventLogger ?? new FakeDomainEventLogger(),
            new FakeTraceContext(),
            NullLogger<MatchCommandHandlers>.Instance);

    private static (InMemoryTeamRepository, Team, Team) TwoTeams()
    {
        var repo = new InMemoryTeamRepository();
        var home = new Team(Guid.NewGuid(), "Home");
        var away = new Team(Guid.NewGuid(), "Away");
        repo.Store[home.Id] = home;
        repo.Store[away.Id] = away;
        return (repo, home, away);
    }

    [Fact]
    public async Task CreateMatch_ValidTeams_PersistsAndCommits()
    {
        var (teamRepo, home, away) = TwoTeams();
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var uow = new FakeUnitOfWork();

        var result = await Build(matchRepo,matchScoreRepo, teamRepo, uow: uow)
            .Handle(new CreateMatchCommand(home.Id, away.Id, DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Single(matchRepo.Store);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task CreateMatch_SameTeamIds_ReturnsValidation()
    {
        var teamId = Guid.NewGuid();

        var result = await Build(new InMemoryMatchRepository(), new InMemoryMatchScoreRepository(), new InMemoryTeamRepository())
            .Handle(new CreateMatchCommand(teamId, teamId, DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task CreateMatch_MissingTeam_ReturnsNotFound()
    {
        var result = await Build(new InMemoryMatchRepository(), new InMemoryMatchScoreRepository(), new InMemoryTeamRepository())
            .Handle(new CreateMatchCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
    }

    [Fact]
    public async Task RegisterMatchResult_ValidData_CompletesMatchAndAddsGoals()
    {
        var (teamRepo, home, away) = TwoTeams();
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var playerRepo = new InMemoryPlayerRepository();
        var uow = new FakeUnitOfWork();
        var eventLogger = new FakeDomainEventLogger();

        var match = new Match(Guid.NewGuid(), home.Id, away.Id, DateTime.UtcNow);
        matchRepo.Store[match.Id] = match;

        var scorer = new Player(Guid.NewGuid(), home.Id, "Striker", 9);
        playerRepo.Store[scorer.Id] = scorer;

        var result = await Build(matchRepo,matchScoreRepo, teamRepo, playerRepo, uow, eventLogger)
            .Handle(new RegisterMatchResultCommand(match.Id, 3, 1, new[] { new RegisterGoalCommand(scorer.Id, 3,0) }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.Completed, match.Status);
        Assert.Equal(3, match.HomeScore);
        Assert.Equal(1, match.AwayScore);
        Assert.Equal(3, scorer.GoalsScored);
        Assert.Single(eventLogger.Events);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task RegisterMatchResult_NegativeScore_ReturnsValidation()
    {
        var result = await Build(new InMemoryMatchRepository(), new InMemoryMatchScoreRepository(), new InMemoryTeamRepository())
            .Handle(new RegisterMatchResultCommand(Guid.NewGuid(), -1, 0, Array.Empty<RegisterGoalCommand>()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task RegisterMatchResult_MatchNotFound_ReturnsNotFound()
    {
        var result = await Build(new InMemoryMatchRepository(), new InMemoryMatchScoreRepository(), new InMemoryTeamRepository())
            .Handle(new RegisterMatchResultCommand(Guid.NewGuid(), 1, 0, Array.Empty<RegisterGoalCommand>()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
    }

    [Fact]
    public async Task PatchMatch_Reschedule_UpdatesDate()
    {
        var (teamRepo, home, away) = TwoTeams();
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var newDate = new DateTime(2026, 6, 1, 20, 0, 0, DateTimeKind.Utc);
        var match = new Match(Guid.NewGuid(), home.Id, away.Id, DateTime.UtcNow);
        matchRepo.Store[match.Id] = match;

        var result = await Build(matchRepo,matchScoreRepo, teamRepo)
            .Handle(new PatchMatchCommand(match.Id, newDate, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newDate, match.MatchDateUtc);
    }

    [Fact]
    public async Task PatchMatch_Cancel_SetsStatusCancelled()
    {
        var (teamRepo, home, away) = TwoTeams();
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var match = new Match(Guid.NewGuid(), home.Id, away.Id, DateTime.UtcNow);
        matchRepo.Store[match.Id] = match;

        var result = await Build(matchRepo, matchScoreRepo,teamRepo)
            .Handle(new PatchMatchCommand(match.Id, null, true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.Cancelled, match.Status);
    }

    [Fact]
    public async Task DeleteMatch_Existing_RemovesAndCommits()
    {
        var matchRepo = new InMemoryMatchRepository();
        var matchScoreRepo = new InMemoryMatchScoreRepository();
        var uow = new FakeUnitOfWork();
        var match = new Match(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        matchRepo.Store[match.Id] = match;

        var result = await Build(matchRepo, matchScoreRepo,new InMemoryTeamRepository(), uow: uow)
            .Handle(new DeleteMatchCommand(match.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(matchRepo.Store);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task DeleteMatch_NotFound_SucceedsWithoutCommit()
    {
        var uow = new FakeUnitOfWork();

        var result = await Build(new InMemoryMatchRepository(), new InMemoryMatchScoreRepository(),new InMemoryTeamRepository(), uow: uow)
            .Handle(new DeleteMatchCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, uow.CommitCount);
    }
}
