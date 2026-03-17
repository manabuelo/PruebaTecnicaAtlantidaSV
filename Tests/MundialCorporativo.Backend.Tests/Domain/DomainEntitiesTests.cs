using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Domain.Enums;
using MundialCorporativo.Domain.Events;

namespace MundialCorporativo.Backend.Tests.Domain;

public class DomainEntitiesTests
{
    [Fact]
    public void TeamConstructor_ShouldAddTeamCreatedDomainEvent()
    {
        var team = new Team(Guid.NewGuid(), "Alpha");

        Assert.Single(team.DomainEvents);
        Assert.IsType<TeamCreatedDomainEvent>(team.DomainEvents.First());
    }

    [Fact]
    public void MatchRegisterResult_ShouldSetCompletedScoresAndDomainEvent()
    {
        var match = new Match(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        match.RegisterResult(2, 1);

        Assert.Equal(MatchStatus.Completed, match.Status);
        Assert.Equal(2, match.HomeScore);
        Assert.Equal(1, match.AwayScore);
        Assert.Contains(match.DomainEvents, evt => evt is MatchResultRegisteredDomainEvent);
    }

    [Fact]
    public void PlayerAddGoals_ShouldIncreaseGoalsScored()
    {
        var player = new Player(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", 9);

        player.AddGoals(3);

        Assert.Equal(3, player.GoalsScored);
    }
}
