namespace MundialCorporativo.Domain.Common;

public abstract record DomainEvent(Guid Id, DateTime OccurredOnUtc);
