using MundialCorporativo.Application.Players;
using MundialCorporativo.Backend.Tests.Support;
using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Backend.Tests.Application;

public class PlayerCommandHandlersTests
{
    [Fact]
    public async Task CreatePlayer_WhenTeamDoesNotExist_ShouldReturnNotFound()
    {
        var playerRepo = new InMemoryPlayerRepository();
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var handler = new PlayerCommandHandlers(playerRepo, teamRepo, uow);

        var result = await handler.Handle(new CreatePlayerCommand(Guid.NewGuid(), "Player", 10), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
        Assert.Equal(0, uow.CommitCount);
    }

    [Fact]
    public async Task CreatePlayer_WithValidInput_ShouldPersistAndCommit()
    {
        var playerRepo = new InMemoryPlayerRepository();
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var team = new Team(Guid.NewGuid(), "Team");
        teamRepo.Store[team.Id] = team;
        var handler = new PlayerCommandHandlers(playerRepo, teamRepo, uow);

        var result = await handler.Handle(new CreatePlayerCommand(team.Id, " Player One ", 7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(playerRepo.Store);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task PatchPlayer_WithInvalidJersey_ShouldReturnValidation()
    {
        var playerRepo = new InMemoryPlayerRepository();
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Player", 9);
        playerRepo.Store[player.Id] = player;
        var handler = new PlayerCommandHandlers(playerRepo, teamRepo, uow);

        var result = await handler.Handle(new PatchPlayerCommand(player.Id, "Player", 0), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
        Assert.Equal(0, uow.CommitCount);
    }
}
