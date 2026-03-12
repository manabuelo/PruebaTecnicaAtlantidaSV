using System.Data;
using MundialCorporativo.Application.Abstractions.Read;
using Npgsql;

namespace MundialCorporativo.Infrastructure.Read;

public class ReadDbConnectionFactory : IReadDbConnectionFactory
{
    private readonly string _connectionString;

    public ReadDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
