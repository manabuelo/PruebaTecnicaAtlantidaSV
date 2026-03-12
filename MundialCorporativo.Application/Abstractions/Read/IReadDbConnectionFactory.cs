using System.Data;

namespace MundialCorporativo.Application.Abstractions.Read;

public interface IReadDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
