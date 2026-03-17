using MundialCorporativo.Application.Players;
using MundialCorporativo.Application.Tests.Support;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Application.Tests.Players;

public class PlayerCommandHandlersTests
{
    private static PlayerCommandHandlers Build(
        InMemoryPlayerRepository playerRepo,
        InMemoryTeamRepository teamRepo,
        FakeUnitOfWork uow)
        => new(playerRepo, teamRepo, uow);

    [Fact]
    public async Task CreatePlayer_ValidInput_PersistsAndCommits()
    {
        var playerRepo = new InMemoryPlayerRepository();
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var team = new Team(Guid.NewGuid(), "Team");
        teamRepo.Store[team.Id] = team;

        var result = await Build(playerRepo, teamRepo, uow).Handle(new CreatePlayerCommand(team.Id, " Striker ", 9), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Single(playerRepo.Store);
        Assert.Equal("Striker", playerRepo.Store.Values.Single().FullName);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task CreatePlayer_TeamNotFound_ReturnsNotFound()
    {
        var result = await Build(new InMemoryPlayerRepository(), new InMemoryTeamRepository(), new FakeUnitOfWork())
            .Handle(new CreatePlayerCommand(Guid.NewGuid(), "Player", 10), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
    }

    [Fact]
    public async Task CreatePlayer_BlankName_ReturnsValidation()
    {
        var teamRepo = new InMemoryTeamRepository();
        var team = new Team(Guid.NewGuid(), "Team");
        teamRepo.Store[team.Id] = team;

        var result = await Build(new InMemoryPlayerRepository(), teamRepo, new FakeUnitOfWork())
            .Handle(new CreatePlayerCommand(team.Id, "  ", 10), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task CreatePlayer_InvalidJersey_ReturnsValidation()
    {
        var teamRepo = new InMemoryTeamRepository();
        var team = new Team(Guid.NewGuid(), "Team");
        teamRepo.Store[team.Id] = team;

        var result = await Build(new InMemoryPlayerRepository(), teamRepo, new FakeUnitOfWork())
            .Handle(new CreatePlayerCommand(team.Id, "Player", 0), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task UpdatePlayer_ExistingPlayer_UpdatesAndCommits()
    {
        var playerRepo = new InMemoryPlayerRepository();
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Old Name", 5);
        playerRepo.Store[player.Id] = player;

        var uow = new FakeUnitOfWork();
        var result = await Build(playerRepo, new InMemoryTeamRepository(), uow)
            .Handle(new UpdatePlayerCommand(player.Id, "New Name", 11), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", player.FullName);
        Assert.Equal(11, player.JerseyNumber);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task UpdatePlayer_NotFound_ReturnsNotFound()
    {
        var result = await Build(new InMemoryPlayerRepository(), new InMemoryTeamRepository(), new FakeUnitOfWork())
            .Handle(new UpdatePlayerCommand(Guid.NewGuid(), "Name", 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
    }

    [Fact]
    public async Task PatchPlayer_NullValues_KeepsExistingValues()
    {
        var playerRepo = new InMemoryPlayerRepository();
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Original", 7);
        playerRepo.Store[player.Id] = player;

        var result = await Build(playerRepo, new InMemoryTeamRepository(), new FakeUnitOfWork())
            .Handle(new PatchPlayerCommand(player.Id, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Original", player.FullName);
        Assert.Equal(7, player.JerseyNumber);
    }

    [Fact]
    public async Task PatchPlayer_InvalidJersey_ReturnsValidation()
    {
        var playerRepo = new InMemoryPlayerRepository();
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Player", 5);
        playerRepo.Store[player.Id] = player;

        var result = await Build(playerRepo, new InMemoryTeamRepository(), new FakeUnitOfWork())
            .Handle(new PatchPlayerCommand(player.Id, null, 0), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task DeletePlayer_Existing_RemovesAndCommits()
    {
        var playerRepo = new InMemoryPlayerRepository();
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Player", 1);
        playerRepo.Store[player.Id] = player;
        var uow = new FakeUnitOfWork();

        var result = await Build(playerRepo, new InMemoryTeamRepository(), uow)
            .Handle(new DeletePlayerCommand(player.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(playerRepo.Store);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task DeletePlayer_NotFound_SucceedsWithoutCommit()
    {
        var uow = new FakeUnitOfWork();

        var result = await Build(new InMemoryPlayerRepository(), new InMemoryTeamRepository(), uow)
            .Handle(new DeletePlayerCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, uow.CommitCount);
    }
}
