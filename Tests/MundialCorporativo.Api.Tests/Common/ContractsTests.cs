using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MundialCorporativo.Api.Common;
using MundialCorporativo.Api.Contracts;

namespace MundialCorporativo.Api.Tests.Common;

public class IdempotentControllerBaseTests
{
    [Fact]
    public void PaginationRequest_DefaultValues_AreCorrect()
    {
        var request = new PaginationRequest();

        Assert.Equal(1, request.PageNumber);
        Assert.Equal(10, request.PageSize);
        Assert.Null(request.SortBy);
        Assert.Null(request.SortDirection);
    }

    [Fact]
    public void TeamCreateRequest_SetsName()
    {
        var request = new TeamCreateRequest("TeamA");

        Assert.Equal("TeamA", request.Name);
    }

    [Fact]
    public void PlayerCreateRequest_SetsProperties()
    {
        var request = new PlayerCreateRequest("Jane Doe", 10);

        Assert.Equal("Jane Doe", request.FullName);
        Assert.Equal(10, request.JerseyNumber);
    }

    [Fact]
    public void MatchCreateRequest_SetsProperties()
    {
        var homeId = Guid.NewGuid();
        var awayId = Guid.NewGuid();
        var date   = DateTime.UtcNow;

        var request = new MatchCreateRequest(homeId, awayId, date);

        Assert.Equal(homeId, request.HomeTeamId);
        Assert.Equal(awayId, request.AwayTeamId);
        Assert.Equal(date, request.MatchDateUtc);
    }

    [Fact]
    public void RegisterMatchResultRequest_SetsScoresAndGoals()
    {
        var goal = new RegisterGoalRequest(Guid.NewGuid(), 2, 0);
        var request = new RegisterMatchResultRequest(3, 1, new[] { goal });

        Assert.Equal(3, request.HomeScore);
        Assert.Equal(1, request.AwayScore);
        Assert.Single(request.Goals!);
        Assert.Equal(2, request.Goals!.First().Goals);
        Assert.Equal(0, request.Goals!.First().GoalsAgainst);
    }
}
