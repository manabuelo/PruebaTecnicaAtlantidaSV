using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Application.Abstractions.Events;

public interface IDomainEventLogger
{
    Task LogAsync(DomainEvent domainEvent, string traceId, CancellationToken cancellationToken);
}
