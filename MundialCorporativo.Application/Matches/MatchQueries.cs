using Dapper;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Read;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;

namespace MundialCorporativo.Application.Matches;

public record GetMatchByIdQuery(Guid MatchId) : IQuery<MatchDto?>;
public record ListMatchesQuery(Guid? TeamId, DateTime? DateFromUtc, DateTime? DateToUtc, string? Status, int PageNumber, int PageSize, string? SortBy, string? SortDirection)
    : IQuery<PagedResult<MatchDto>>;

public class MatchQueryHandlers :
    IQueryHandler<GetMatchByIdQuery, MatchDto?>,
    IQueryHandler<ListMatchesQuery, PagedResult<MatchDto>>
{
    private readonly IReadDbConnectionFactory _connectionFactory;

    public MatchQueryHandlers(IReadDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<MatchDto?> Handle(GetMatchByIdQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        const string sql = @"
            SELECT m.""Id"", m.""HomeTeamId"", h.""Name"" AS HomeTeamName,
                   m.""AwayTeamId"", a.""Name"" AS AwayTeamName,
                   m.""MatchDateUtc"",
                   CASE m.""Status""
                       WHEN 1 THEN 'Scheduled'
                       WHEN 2 THEN 'Completed'
                       WHEN 3 THEN 'Cancelled'
                       ELSE 'Scheduled'
                   END AS Status,
                   m.""HomeScore"", m.""AwayScore""
            FROM ""Matches"" m
            INNER JOIN ""Teams"" h ON h.""Id"" = m.""HomeTeamId""
            INNER JOIN ""Teams"" a ON a.""Id"" = m.""AwayTeamId""
            WHERE m.""Id"" = @MatchId;
            ";

        return await connection.QuerySingleOrDefaultAsync<MatchDto>(sql, new { query.MatchId });
    }

    public async Task<PagedResult<MatchDto>> Handle(ListMatchesQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var page = new PageRequest(query.PageNumber, query.PageSize, query.SortBy, query.SortDirection);
        var orderBy = ResolveOrderBy(query.SortBy);
        var statusValue = ResolveStatusValue(query.Status);

                const string countSql = @"
                        SELECT COUNT(1)
                        FROM ""Matches"" m
                        WHERE (@TeamId IS NULL OR m.""HomeTeamId"" = @TeamId OR m.""AwayTeamId"" = @TeamId)
                            AND (@DateFromUtc IS NULL OR m.""MatchDateUtc"" >= @DateFromUtc)
                            AND (@DateToUtc IS NULL OR m.""MatchDateUtc"" <= @DateToUtc)
                            AND (@StatusValue IS NULL OR m.""Status"" = @StatusValue);
                        ";

        var total = await connection.ExecuteScalarAsync<int>(countSql, new
        {
            query.TeamId,
            query.DateFromUtc,
            query.DateToUtc,
            StatusValue = statusValue
        });

                var sql = $@"
                        SELECT m.""Id"", m.""HomeTeamId"", h.""Name"" AS HomeTeamName,
                                     m.""AwayTeamId"", a.""Name"" AS AwayTeamName,
                                     m.""MatchDateUtc"",
                                     CASE m.""Status""
                                             WHEN 1 THEN 'Scheduled'
                                             WHEN 2 THEN 'Completed'
                                             WHEN 3 THEN 'Cancelled'
                                             ELSE 'Scheduled'
                                     END AS Status,
                                     m.""HomeScore"", m.""AwayScore""
                        FROM ""Matches"" m
                        INNER JOIN ""Teams"" h ON h.""Id"" = m.""HomeTeamId""
                        INNER JOIN ""Teams"" a ON a.""Id"" = m.""AwayTeamId""
                        WHERE (@TeamId IS NULL OR m.""HomeTeamId"" = @TeamId OR m.""AwayTeamId"" = @TeamId)
                            AND (@DateFromUtc IS NULL OR m.""MatchDateUtc"" >= @DateFromUtc)
                            AND (@DateToUtc IS NULL OR m.""MatchDateUtc"" <= @DateToUtc)
                            AND (@StatusValue IS NULL OR m.""Status"" = @StatusValue)
                        ORDER BY {orderBy} {page.SafeSortDirection}
                        LIMIT @PageSize OFFSET @Offset;
                        ";

        var data = (await connection.QueryAsync<MatchDto>(sql, new
        {
            query.TeamId,
            query.DateFromUtc,
            query.DateToUtc,
            StatusValue = statusValue,
            Offset = page.Offset,
            PageSize = page.SafePageSize
        })).ToList();

        return new PagedResult<MatchDto>
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
            "status" => "m.\"Status\"",
            _ => "m.\"MatchDateUtc\""
        };

    private static int? ResolveStatusValue(string? status)
        => status?.ToLowerInvariant() switch
        {
            "scheduled" => 1,
            "completed" => 2,
            "cancelled" => 3,
            _ => null
        };
}
