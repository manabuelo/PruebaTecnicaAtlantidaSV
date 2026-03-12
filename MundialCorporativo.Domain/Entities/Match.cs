using MundialCorporativo.Domain.Common;
using MundialCorporativo.Domain.Enums;
using MundialCorporativo.Domain.Events;

namespace MundialCorporativo.Domain.Entities;

public class Match : Entity
{
    public Match(Guid id, Guid homeTeamId, Guid awayTeamId, DateTime matchDateUtc)
    {
        Id = id;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        MatchDateUtc = matchDateUtc;
        Status = MatchStatus.Scheduled;
    }

    private Match()
    {
    }

    public Guid Id { get; private set; }
    public Guid HomeTeamId { get; private set; }
    public Guid AwayTeamId { get; private set; }
    public DateTime MatchDateUtc { get; private set; }
    public MatchStatus Status { get; private set; }
    public int? HomeScore { get; private set; }
    public int? AwayScore { get; private set; }

    public void Reschedule(DateTime matchDateUtc)
    {
        MatchDateUtc = matchDateUtc;
    }

    public void RegisterResult(int homeScore, int awayScore)
    {
        HomeScore = homeScore;
        AwayScore = awayScore;
        Status = MatchStatus.Completed;
        AddDomainEvent(new MatchResultRegisteredDomainEvent(Guid.NewGuid(), DateTime.UtcNow, Id, homeScore, awayScore));
    }

    public void Cancel()
    {
        Status = MatchStatus.Cancelled;
    }
}
