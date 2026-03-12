namespace MundialCorporativo.Application.Abstractions.Idempotency;

public interface IIdempotencyService
{
    Task<IdempotencyRecordDto?> GetExistingAsync(string key, string path, string method, CancellationToken cancellationToken);
    Task SaveAsync(string key, string path, string method, int statusCode, string responseBody, CancellationToken cancellationToken);
}

public record IdempotencyRecordDto(int StatusCode, string ResponseBody);
