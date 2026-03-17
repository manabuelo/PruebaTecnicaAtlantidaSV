using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Infrastructure.Persistence;

namespace MundialCorporativo.Infrastructure.Tests.Support;

internal static class DbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public FakeUnitOfWork(AppDbContext db) => _db = db;

    public Task<int> CommitAsync(CancellationToken cancellationToken)
        => _db.SaveChangesAsync(cancellationToken);
}
