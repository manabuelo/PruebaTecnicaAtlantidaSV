using Dapper;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Read;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;

namespace MundialCorporativo.Application.Players;

public record GetPlayerByIdQuery(Guid PlayerId) : IQuery<PlayerDto?>;
public record ListPlayersQuery(Guid? TeamId, string? Name, int PageNumber, int PageSize, string? SortBy, string? SortDirection) : IQuery<PagedResult<PlayerDto>>;

public class PlayerQueryHandlers :
    IQueryHandler<GetPlayerByIdQuery, PlayerDto?>,
    IQueryHandler<ListPlayersQuery, PagedResult<PlayerDto>>
{
    private readonly IReadDbConnectionFactory _connectionFactory;

    public PlayerQueryHandlers(IReadDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PlayerDto?> Handle(GetPlayerByIdQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        const string sql = @"
            SELECT p.""Id"", p.""TeamId"", t.""Name"" AS TeamName, p.""FullName"", p.""JerseyNumber"", p.""GoalsScored""
            FROM ""Players"" p
            INNER JOIN ""Teams"" t ON t.""Id"" = p.""TeamId""
            WHERE p.""Id"" = @PlayerId;
            ";

        return await connection.QuerySingleOrDefaultAsync<PlayerDto>(sql, new { query.PlayerId });
    }

    public async Task<PagedResult<PlayerDto>> Handle(ListPlayersQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var page = new PageRequest(query.PageNumber, query.PageSize, query.SortBy, query.SortDirection);
        var orderBy = ResolveOrderBy(query.SortBy);

                const string countSql = @"
                        SELECT COUNT(1)
                        FROM ""Players"" p
                        INNER JOIN ""Teams"" t ON t.""Id"" = p.""TeamId""
                        WHERE (@TeamId IS NULL OR p.""TeamId"" = @TeamId)
                            AND (@Name IS NULL OR p.""FullName"" ILIKE '%' || @Name || '%');
                        ";

        var total = await connection.ExecuteScalarAsync<int>(countSql, new { query.TeamId, query.Name });

                var sql = $@"
                        SELECT p.""Id"", p.""TeamId"", t.""Name"" AS TeamName, p.""FullName"", p.""JerseyNumber"", p.""GoalsScored""
                        FROM ""Players"" p
                        INNER JOIN ""Teams"" t ON t.""Id"" = p.""TeamId""
                        WHERE (@TeamId IS NULL OR p.""TeamId"" = @TeamId)
                            AND (@Name IS NULL OR p.""FullName"" ILIKE '%' || @Name || '%')
                        ORDER BY {orderBy} {page.SafeSortDirection}
                        LIMIT @PageSize OFFSET @Offset;
                        ";

        var data = (await connection.QueryAsync<PlayerDto>(sql, new
        {
            query.TeamId,
            query.Name,
            Offset = page.Offset,
            PageSize = page.SafePageSize
        })).ToList();

        return new PagedResult<PlayerDto>
        {
            Data = data,
            PageNumber = page.SafePageNumber,
            PageSize = page.SafePageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)page.SafePageSize)
        };
    }

    private static string ResolveOrderBy(string? sortBy)
        => sortBy?.ToLowerInvariant() switch
        {
            "fullname" => "p.\"FullName\"",
            "goalsscored" => "p.\"GoalsScored\"",
            _ => "p.\"FullName\""
        };
}
