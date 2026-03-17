using MundialCorporativo.Domain.Entities;

namespace MundialCorporativo.Domain.Tests;

public class PlayerTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var id = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var player = new Player(id, teamId, "Jane Doe", 10);

        Assert.Equal(id, player.Id);
        Assert.Equal(teamId, player.TeamId);
        Assert.Equal("Jane Doe", player.FullName);
        Assert.Equal(10, player.JerseyNumber);
        Assert.Equal(0, player.GoalsScored);
    }

    [Fact]
    public void Update_ShouldChangeNameAndJersey()
    {
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", 10);

        player.Update("John Doe", 7);

        Assert.Equal("John Doe", player.FullName);
        Assert.Equal(7, player.JerseyNumber);
    }

    [Fact]
    public void AddGoals_ShouldAccumulateGoals()
    {
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", 10);

        player.AddGoals(2);
        player.AddGoals(3);

        Assert.Equal(5, player.GoalsScored);
    }

    [Fact]
    public void AddGoals_WithZero_ShouldNotChange()
    {
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", 10);

        player.AddGoals(0);

        Assert.Equal(0, player.GoalsScored);
    }
}
