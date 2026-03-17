using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Domain.Enums;
using MundialCorporativo.Domain.Events;

namespace MundialCorporativo.Domain.Tests;

public class MatchTests
{
    [Fact]
    public void Constructor_ShouldSetScheduledStatus()
    {
        var match = new Match(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(MatchStatus.Scheduled, match.Status);
        Assert.Null(match.HomeScore);
        Assert.Null(match.AwayScore);
    }

    [Fact]
    public void RegisterResult_ShouldSetScoresAndCompleteStatus()
    {
        var match = new Match(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        match.RegisterResult(3, 1);

        Assert.Equal(MatchStatus.Completed, match.Status);
        Assert.Equal(3, match.HomeScore);
        Assert.Equal(1, match.AwayScore);
    }

    [Fact]
    public void RegisterResult_ShouldRaiseMatchResultRegisteredDomainEvent()
    {
        var match = new Match(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        match.RegisterResult(2, 0);

        var evt = Assert.IsType<MatchResultRegisteredDomainEvent>(match.DomainEvents.Single());
        Assert.Equal(match.Id, evt.MatchId);
        Assert.Equal(2, evt.HomeScore);
        Assert.Equal(0, evt.AwayScore);
    }

    [Fact]
    public void Cancel_ShouldSetCancelledStatus()
    {
        var match = new Match(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        match.Cancel();

        Assert.Equal(MatchStatus.Cancelled, match.Status);
    }

    [Fact]
    public void Reschedule_ShouldUpdateMatchDate()
    {
        var original = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var newDate  = new DateTime(2026, 4, 1, 15, 0, 0, DateTimeKind.Utc);
        var match = new Match(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), original);

        match.Reschedule(newDate);

        Assert.Equal(newDate, match.MatchDateUtc);
    }
}
