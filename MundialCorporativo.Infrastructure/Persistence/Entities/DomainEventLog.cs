namespace MundialCorporativo.Infrastructure.Persistence.Entities;

public class DomainEventLog
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
}
