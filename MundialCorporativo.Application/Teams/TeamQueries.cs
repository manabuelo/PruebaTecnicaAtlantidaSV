using Dapper;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Read;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;

namespace MundialCorporativo.Application.Teams;

public record GetTeamByIdQuery(Guid TeamId) : IQuery<TeamDto?>;
public record ListTeamsQuery(string? Name, int PageNumber, int PageSize, string? SortBy, string? SortDirection) : IQuery<PagedResult<TeamDto>>;

public class TeamQueryHandlers :
    IQueryHandler<GetTeamByIdQuery, TeamDto?>,
    IQueryHandler<ListTeamsQuery, PagedResult<TeamDto>>
{
    private readonly IReadDbConnectionFactory _connectionFactory;

    public TeamQueryHandlers(IReadDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TeamDto?> Handle(GetTeamByIdQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        const string sql = @"
            SELECT t.""Id"", t.""Name""
            FROM ""Teams"" t
            WHERE t.""Id"" = @TeamId;
            ";

        return await connection.QuerySingleOrDefaultAsync<TeamDto>(sql, new { query.TeamId });
    }

    public async Task<PagedResult<TeamDto>> Handle(ListTeamsQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var page = new PageRequest(query.PageNumber, query.PageSize, query.SortBy, query.SortDirection);
        var orderBy = ResolveOrderBy(query.SortBy);

        const string countSql = @"
            SELECT COUNT(1)
            FROM ""Teams"" t
            WHERE (@Name IS NULL OR t.""Name"" ILIKE '%' || @Name || '%');
            ";

        var total = await connection.ExecuteScalarAsync<int>(countSql, new { query.Name });

        var sql = $@"
            SELECT t.""Id"", t.""Name""
            FROM ""Teams"" t
            WHERE (@Name IS NULL OR t.""Name"" ILIKE '%' || @Name || '%')
            ORDER BY {orderBy} {page.SafeSortDirection}
            LIMIT @PageSize OFFSET @Offset;
            ";

        var data = (await connection.QueryAsync<TeamDto>(sql, new
        {
            query.Name,
            Offset = page.Offset,
            PageSize = page.SafePageSize
        })).ToList();

        return new PagedResult<TeamDto>
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
            "name" => "t.\"Name\"",
            _ => "t.\"Name\""
        };
}
