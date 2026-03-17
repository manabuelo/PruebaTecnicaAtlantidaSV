using Microsoft.Extensions.Logging.Abstractions;
using MundialCorporativo.Domain.Events;
using MundialCorporativo.Infrastructure.Services;
using MundialCorporativo.Infrastructure.Tests.Support;

namespace MundialCorporativo.Infrastructure.Tests.Services;

public class DomainEventLoggerTests
{
    [Fact]
    public async Task LogAsync_SavesDomainEventLogEntry()
    {
        var db = DbContextFactory.Create();
        var logger = new DomainEventLogger(db, NullLogger<DomainEventLogger>.Instance);

        var evt = new TeamCreatedDomainEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "Alpha");

        await logger.LogAsync(evt, "trace-001", CancellationToken.None);
        await db.SaveChangesAsync();

        var entry = db.DomainEventLogs.Single();
        Assert.Equal("TeamCreatedDomainEvent", entry.EventType);
        Assert.Equal("trace-001", entry.TraceId);
        Assert.Equal(evt.OccurredOnUtc, entry.OccurredOnUtc);
        Assert.Contains("Alpha", entry.Payload);
    }

    [Fact]
    public async Task LogAsync_MultipleEvents_SavesAll()
    {
        var db = DbContextFactory.Create();
        var logger = new DomainEventLogger(db, NullLogger<DomainEventLogger>.Instance);

        var evt1 = new TeamCreatedDomainEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "Alpha");
        var evt2 = new MatchResultRegisteredDomainEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), 2, 1);

        await logger.LogAsync(evt1, "trace-001", CancellationToken.None);
        await logger.LogAsync(evt2, "trace-002", CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Equal(2, db.DomainEventLogs.Count());
    }
}
