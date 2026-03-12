namespace MundialCorporativo.Application.Abstractions.Observability;

public interface ITraceContext
{
    string TraceId { get; }
}
