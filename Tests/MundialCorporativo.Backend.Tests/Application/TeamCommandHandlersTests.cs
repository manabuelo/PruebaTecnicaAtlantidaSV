using Microsoft.Extensions.Logging.Abstractions;
using MundialCorporativo.Application.Teams;
using MundialCorporativo.Backend.Tests.Support;

namespace MundialCorporativo.Backend.Tests.Application;

public class TeamCommandHandlersTests
{
    [Fact]
    public async Task CreateTeam_WithValidName_ShouldPersistCommitAndLogDomainEvent()
    {
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var eventLogger = new FakeDomainEventLogger();
        var handler = new TeamCommandHandlers(teamRepo, uow, eventLogger, new FakeTraceContext(), NullLogger<TeamCommandHandlers>.Instance);

        var result = await handler.Handle(new CreateTeamCommand(" Team A "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Single(teamRepo.Store);
        Assert.Equal(1, uow.CommitCount);
        Assert.Single(eventLogger.Events);
    }

    [Fact]
    public async Task UpdateTeam_WhenNotFound_ShouldReturnNotFound()
    {
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var handler = new TeamCommandHandlers(teamRepo, uow, new FakeDomainEventLogger(), new FakeTraceContext(), NullLogger<TeamCommandHandlers>.Instance);

        var result = await handler.Handle(new UpdateTeamCommand(Guid.NewGuid(), "Updated"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
        Assert.Equal(0, uow.CommitCount);
    }

    [Fact]
    public async Task DeleteTeam_WhenMissing_ShouldReturnSuccessWithoutCommit()
    {
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var handler = new TeamCommandHandlers(teamRepo, uow, new FakeDomainEventLogger(), new FakeTraceContext(), NullLogger<TeamCommandHandlers>.Instance);

        var result = await handler.Handle(new DeleteTeamCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, uow.CommitCount);
    }
}
