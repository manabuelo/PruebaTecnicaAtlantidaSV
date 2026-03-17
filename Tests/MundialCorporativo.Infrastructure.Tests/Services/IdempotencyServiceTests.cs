using MundialCorporativo.Application.Abstractions.Idempotency;
using MundialCorporativo.Infrastructure.Persistence;
using MundialCorporativo.Infrastructure.Services;
using MundialCorporativo.Infrastructure.Tests.Support;

namespace MundialCorporativo.Infrastructure.Tests.Services;

public class IdempotencyServiceTests
{
    private static (IdempotencyService, AppDbContext) Build()
    {
        var db = DbContextFactory.Create();
        var uow = new FakeUnitOfWork(db);
        return (new IdempotencyService(db, uow), db);
    }

    [Fact]
    public async Task GetExisting_WhenNoRecord_ReturnsNull()
    {
        var (service, _) = Build();

        var result = await service.GetExistingAsync("key1", "/api/teams", "POST", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAndRetrieve_ReturnsSavedResponseBody()
    {
        var (service, _) = Build();

        await service.SaveAsync("key1", "/api/teams", "POST", 201, "{\"id\":\"abc\"}", CancellationToken.None);

        var record = await service.GetExistingAsync("key1", "/api/teams", "POST", CancellationToken.None);
        Assert.NotNull(record);
        Assert.Equal(201, record!.StatusCode);
        Assert.Equal("{\"id\":\"abc\"}", record.ResponseBody);
    }

    [Fact]
    public async Task GetExisting_DifferentPath_ReturnsNull()
    {
        var (service, _) = Build();

        await service.SaveAsync("key1", "/api/teams", "POST", 200, "{}", CancellationToken.None);

        var result = await service.GetExistingAsync("key1", "/api/players", "POST", CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveTwice_SameKey_DoesNotThrow()
    {
        var (service, _) = Build();

        await service.SaveAsync("key1", "/api/teams", "POST", 201, "a", CancellationToken.None);

        var ex = await Record.ExceptionAsync(() =>
            service.SaveAsync("key1", "/api/teams", "POST", 201, "b", CancellationToken.None));

        Assert.Null(ex);
    }
}
