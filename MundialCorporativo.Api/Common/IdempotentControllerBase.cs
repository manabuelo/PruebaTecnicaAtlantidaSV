using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MundialCorporativo.Application.Abstractions.Idempotency;

namespace MundialCorporativo.Api.Common;

[ApiController]
public abstract class IdempotentControllerBase : ControllerBase
{
    protected const string IdempotencyHeader = "Idempotency-Key";

    protected async Task<IActionResult> ExecuteIdempotentPostAsync(
        IIdempotencyService idempotencyService,
        Func<CancellationToken, Task<(int StatusCode, object Payload)>> action,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue(IdempotencyHeader, out var key) || string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { error = "Idempotency-Key es obligatorio para POST.", code = "Validation" });
        }

        var existing = await idempotencyService.GetExistingAsync(key!, Request.Path, Request.Method, cancellationToken);
        if (existing is not null)
        {
            return new ContentResult
            {
                Content = existing.ResponseBody,
                ContentType = "application/json",
                StatusCode = existing.StatusCode
            };
        }

        var result = await action(cancellationToken);
        var serialized = JsonSerializer.Serialize(result.Payload);

        await idempotencyService.SaveAsync(key!, Request.Path, Request.Method, result.StatusCode, serialized, cancellationToken);

        return new ObjectResult(result.Payload) { StatusCode = result.StatusCode };
    }
}
