using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Domain.Events;

public record MatchResultRegisteredDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid MatchId,
    int HomeScore,
    int AwayScore) : DomainEvent(Id, OccurredOnUtc);
