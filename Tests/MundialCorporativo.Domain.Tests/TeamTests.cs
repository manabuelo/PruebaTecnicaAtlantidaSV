using MundialCorporativo.Domain.Common;
using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Domain.Enums;
using MundialCorporativo.Domain.Events;

namespace MundialCorporativo.Domain.Tests;

public class TeamTests
{
    [Fact]
    public void Constructor_ShouldSetNameAndId()
    {
        var id = Guid.NewGuid();

        var team = new Team(id, "Alpha");

        Assert.Equal(id, team.Id);
        Assert.Equal("Alpha", team.Name);
    }

    [Fact]
    public void Constructor_ShouldRaiseTeamCreatedDomainEvent()
    {
        var team = new Team(Guid.NewGuid(), "Alpha");

        Assert.Single(team.DomainEvents);
        var evt = Assert.IsType<TeamCreatedDomainEvent>(team.DomainEvents.First());
        Assert.Equal(team.Id, evt.TeamId);
        Assert.Equal("Alpha", evt.TeamName);
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        var team = new Team(Guid.NewGuid(), "Alpha");

        team.Rename("Beta");

        Assert.Equal("Beta", team.Name);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyCollection()
    {
        var team = new Team(Guid.NewGuid(), "Alpha");

        team.ClearDomainEvents();

        Assert.Empty(team.DomainEvents);
    }
}
