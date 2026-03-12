using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Application.Abstractions.Idempotency;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Infrastructure.Persistence;
using MundialCorporativo.Infrastructure.Persistence.Entities;

namespace MundialCorporativo.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly AppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public IdempotencyService(AppDbContext dbContext, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<IdempotencyRecordDto?> GetExistingAsync(string key, string path, string method, CancellationToken cancellationToken)
    {
        var record = await _dbContext.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key && x.Path == path && x.Method == method, cancellationToken);

        return record is null ? null : new IdempotencyRecordDto(record.StatusCode, record.ResponseBody);
    }

    public async Task SaveAsync(string key, string path, string method, int statusCode, string responseBody, CancellationToken cancellationToken)
    {
        await _dbContext.IdempotencyRecords.AddAsync(new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            Key = key,
            Path = path,
            Method = method,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            CreatedUtc = DateTime.UtcNow
        }, cancellationToken);

        try
        {
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
        }
    }
}
