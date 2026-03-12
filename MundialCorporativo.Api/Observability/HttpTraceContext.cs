using MundialCorporativo.Application.Abstractions.Observability;

namespace MundialCorporativo.Api.Observability;

public class HttpTraceContext : ITraceContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTraceContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TraceId => _httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;
}
