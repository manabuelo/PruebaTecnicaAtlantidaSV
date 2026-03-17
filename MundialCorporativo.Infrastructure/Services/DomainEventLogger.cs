using System.Text.Json;
using Microsoft.Extensions.Logging;
using MundialCorporativo.Application.Abstractions.Events;
using MundialCorporativo.Domain.Common;
using MundialCorporativo.Infrastructure.Persistence;
using MundialCorporativo.Infrastructure.Persistence.Entities;

namespace MundialCorporativo.Infrastructure.Services;

public class DomainEventLogger : IDomainEventLogger
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DomainEventLogger> _logger;

    public DomainEventLogger(AppDbContext dbContext, ILogger<DomainEventLogger> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task LogAsync(DomainEvent domainEvent, string traceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DomainEvent {EventType} TraceId={TraceId}", domainEvent.GetType().Name, traceId);

        return _dbContext.DomainEventLogs.AddAsync(new DomainEventLog
        {
            Id = Guid.NewGuid(),
            EventType = domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            TraceId = traceId,
            OccurredOnUtc = domainEvent.OccurredOnUtc
        }, cancellationToken).AsTask();
    }
}
