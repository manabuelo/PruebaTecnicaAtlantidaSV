using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using MundialCorporativo.Api.Middleware;

namespace MundialCorporativo.Api.Tests.Middleware;

public class CorrelationMiddlewareTests
{
    [Fact]
    public async Task Invoke_WithoutCorrelationHeader_GeneratesNewTraceId()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        string? capturedTraceId = null;

        RequestDelegate next = ctx =>
        {
            capturedTraceId = ctx.TraceIdentifier;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationMiddleware(next, NullLogger<CorrelationMiddleware>.Instance);

        await middleware.Invoke(httpContext);

        Assert.NotNull(capturedTraceId);
        Assert.NotEmpty(capturedTraceId);
        Assert.Equal(capturedTraceId, httpContext.Response.Headers[CorrelationMiddleware.TraceHeader].ToString());
    }

    [Fact]
    public async Task Invoke_WithCorrelationHeader_UsesProvidedId()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Headers[CorrelationMiddleware.CorrelationHeader] = "my-trace-id";

        string? capturedTraceId = null;

        RequestDelegate next = ctx =>
        {
            capturedTraceId = ctx.TraceIdentifier;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationMiddleware(next, NullLogger<CorrelationMiddleware>.Instance);

        await middleware.Invoke(httpContext);

        Assert.Equal("my-trace-id", capturedTraceId);
        Assert.Equal("my-trace-id", httpContext.Response.Headers[CorrelationMiddleware.TraceHeader].ToString());
    }

    [Fact]
    public async Task Invoke_SetsXTraceIdResponseHeader()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Headers[CorrelationMiddleware.CorrelationHeader] = "abc-123";

        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask, NullLogger<CorrelationMiddleware>.Instance);

        await middleware.Invoke(httpContext);

        Assert.Equal("abc-123", httpContext.Response.Headers[CorrelationMiddleware.TraceHeader].ToString());
    }
}
