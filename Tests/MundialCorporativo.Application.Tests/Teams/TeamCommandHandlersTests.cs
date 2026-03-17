using Microsoft.Extensions.Logging.Abstractions;
using MundialCorporativo.Application.Teams;
using MundialCorporativo.Application.Tests.Support;

namespace MundialCorporativo.Application.Tests.Teams;

public class TeamCommandHandlersTests
{
    private TeamCommandHandlers Build(
        InMemoryTeamRepository? teamRepo = null,
        FakeUnitOfWork? uow = null,
        FakeDomainEventLogger? logger = null)
    {
        return new TeamCommandHandlers(
            teamRepo ?? new InMemoryTeamRepository(),
            uow ?? new FakeUnitOfWork(),
            logger ?? new FakeDomainEventLogger(),
            new FakeTraceContext(),
            NullLogger<TeamCommandHandlers>.Instance);
    }

    [Fact]
    public async Task CreateTeam_ValidName_ReturnsSuccessWithId()
    {
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var handler = Build(teamRepo, uow);

        var result = await handler.Handle(new CreateTeamCommand("Tigres Tech"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Single(teamRepo.Store);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task CreateTeam_BlankName_ReturnsValidationFailure()
    {
        var result = await Build().Handle(new CreateTeamCommand("   "), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task CreateTeam_TrimsName()
    {
        var teamRepo = new InMemoryTeamRepository();
        var handler = Build(teamRepo);

        await handler.Handle(new CreateTeamCommand("  Tigres  "), CancellationToken.None);

        Assert.Equal("Tigres", teamRepo.Store.Values.Single().Name);
    }

    [Fact]
    public async Task CreateTeam_RaisesDomainEventAndLogs()
    {
        var eventLogger = new FakeDomainEventLogger();
        var handler = Build(logger: eventLogger);

        await handler.Handle(new CreateTeamCommand("Alpha"), CancellationToken.None);

        Assert.Single(eventLogger.Events);
    }

    [Fact]
    public async Task UpdateTeam_ExistingTeam_RenamesAndCommits()
    {
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var handler = Build(teamRepo, uow);

        var createResult = await handler.Handle(new CreateTeamCommand("Old Name"), CancellationToken.None);
        uow = new FakeUnitOfWork();

        var updateHandler = Build(teamRepo, uow);
        var result = await updateHandler.Handle(new UpdateTeamCommand(createResult.Value, "New Name"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", teamRepo.Store[createResult.Value].Name);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task UpdateTeam_NotFound_ReturnsNotFound()
    {
        var result = await Build().Handle(new UpdateTeamCommand(Guid.NewGuid(), "X"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
    }

    [Fact]
    public async Task UpdateTeam_BlankName_ReturnsValidation()
    {
        var result = await Build().Handle(new UpdateTeamCommand(Guid.NewGuid(), " "), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public async Task PatchTeam_WithNewName_Renames()
    {
        var teamRepo = new InMemoryTeamRepository();
        var handler = Build(teamRepo);

        var cr = await handler.Handle(new CreateTeamCommand("Alpha"), CancellationToken.None);
        var patchHandler = Build(teamRepo);
        var result = await patchHandler.Handle(new PatchTeamCommand(cr.Value, "Beta"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Beta", teamRepo.Store[cr.Value].Name);
    }

    [Fact]
    public async Task PatchTeam_NullName_NameUnchanged()
    {
        var teamRepo = new InMemoryTeamRepository();
        var handler = Build(teamRepo);

        var cr = await handler.Handle(new CreateTeamCommand("Alpha"), CancellationToken.None);
        var patchHandler = Build(teamRepo);
        var result = await patchHandler.Handle(new PatchTeamCommand(cr.Value, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Alpha", teamRepo.Store[cr.Value].Name);
    }

    [Fact]
    public async Task DeleteTeam_Existing_RemovesAndCommits()
    {
        var teamRepo = new InMemoryTeamRepository();
        var uow = new FakeUnitOfWork();
        var handler = Build(teamRepo, uow);

        var cr = await handler.Handle(new CreateTeamCommand("Alpha"), CancellationToken.None);
        uow = new FakeUnitOfWork();
        var deleteHandler = Build(teamRepo, uow);
        var result = await deleteHandler.Handle(new DeleteTeamCommand(cr.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(teamRepo.Store);
        Assert.Equal(1, uow.CommitCount);
    }

    [Fact]
    public async Task DeleteTeam_NotFound_ReturnsSuccessWithoutCommit()
    {
        var uow = new FakeUnitOfWork();
        var result = await Build(uow: uow).Handle(new DeleteTeamCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, uow.CommitCount);
    }
}
