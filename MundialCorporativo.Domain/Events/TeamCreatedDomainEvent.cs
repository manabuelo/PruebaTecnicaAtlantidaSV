using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Domain.Events;

public record TeamCreatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid TeamId,
    string TeamName) : DomainEvent(Id, OccurredOnUtc);
