using Dapper;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Read;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;

namespace MundialCorporativo.Application.Standings;

public record GetStandingsQuery(int PageNumber, int PageSize) : IQuery<PagedResult<StandingDto>>;
public record GetTopScorersQuery(int PageNumber, int PageSize) : IQuery<PagedResult<TopScorerDto>>;

public class StandingQueryHandlers :
    IQueryHandler<GetStandingsQuery, PagedResult<StandingDto>>,
    IQueryHandler<GetTopScorersQuery, PagedResult<TopScorerDto>>
{
    private readonly IReadDbConnectionFactory _connectionFactory;

    public StandingQueryHandlers(IReadDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<StandingDto>> Handle(GetStandingsQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var page = new PageRequest(query.PageNumber, query.PageSize);

        const string countSql = "SELECT COUNT(1) FROM \"Teams\";";
        var total = await connection.ExecuteScalarAsync<int>(countSql);

        const string sql = @"
            WITH TeamStats AS (
                SELECT
                    t.""Id"" AS TeamId,
                    t.""Name"" AS TeamName,
                    COUNT(m.""Id"") FILTER (WHERE m.""Status"" = 2) AS Played,
                    SUM(CASE WHEN m.""Status"" = 2 AND ((m.""HomeTeamId"" = t.""Id"" AND m.""HomeScore"" > m.""AwayScore"") OR (m.""AwayTeamId"" = t.""Id"" AND m.""AwayScore"" > m.""HomeScore"")) THEN 1 ELSE 0 END) AS Won,
                    SUM(CASE WHEN m.""Status"" = 2 AND m.""HomeScore"" = m.""AwayScore"" THEN 1 ELSE 0 END) AS Drawn,
                    SUM(CASE WHEN m.""Status"" = 2 AND ((m.""HomeTeamId"" = t.""Id"" AND m.""HomeScore"" < m.""AwayScore"") OR (m.""AwayTeamId"" = t.""Id"" AND m.""AwayScore"" < m.""HomeScore"")) THEN 1 ELSE 0 END) AS Lost,
                    SUM(CASE WHEN m.""Status"" = 2 AND m.""HomeTeamId"" = t.""Id"" THEN COALESCE(m.""HomeScore"",0)
                             WHEN m.""Status"" = 2 AND m.""AwayTeamId"" = t.""Id"" THEN COALESCE(m.""AwayScore"",0) ELSE 0 END) AS GoalsFor,
                    SUM(CASE WHEN m.""Status"" = 2 AND m.""HomeTeamId"" = t.""Id"" THEN COALESCE(m.""AwayScore"",0)
                             WHEN m.""Status"" = 2 AND m.""AwayTeamId"" = t.""Id"" THEN COALESCE(m.""HomeScore"",0) ELSE 0 END) AS GoalsAgainst
                FROM ""Teams"" t
                LEFT JOIN ""Matches"" m ON m.""HomeTeamId"" = t.""Id"" OR m.""AwayTeamId"" = t.""Id""
                GROUP BY t.""Id"", t.""Name""
            )
            SELECT
                TeamId,
                TeamName,
                Played,
                Won,
                Drawn,
                Lost,
                GoalsFor,
                GoalsAgainst,
                (GoalsFor - GoalsAgainst) AS GoalDifference,
                (Won * 3 + Drawn) AS Points
            FROM TeamStats
            ORDER BY Points DESC, GoalDifference DESC, GoalsFor DESC, TeamName ASC
            LIMIT @PageSize OFFSET @Offset;
            ";

        var data = (await connection.QueryAsync<StandingDto>(sql, new { Offset = page.Offset, PageSize = page.SafePageSize })).ToList();

        return new PagedResult<StandingDto>
        {
            Data = data,
            PageNumber = page.SafePageNumber,
            PageSize = page.SafePageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)page.SafePageSize)
        };
    }

    public async Task<PagedResult<TopScorerDto>> Handle(GetTopScorersQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var page = new PageRequest(query.PageNumber, query.PageSize);

        const string countSql = "SELECT COUNT(1) FROM \"Players\";";
        var total = await connection.ExecuteScalarAsync<int>(countSql);

        const string sql = @"
            SELECT p.""Id"" AS PlayerId, p.""FullName"" AS PlayerName, t.""Name"" AS TeamName, p.""GoalsScored""
            FROM ""Players"" p
            INNER JOIN ""Teams"" t ON t.""Id"" = p.""TeamId""
            ORDER BY p.""GoalsScored"" DESC, p.""FullName"" ASC
            LIMIT @PageSize OFFSET @Offset;
            ";

        var data = (await connection.QueryAsync<TopScorerDto>(sql, new { Offset = page.Offset, PageSize = page.SafePageSize })).ToList();

        return new PagedResult<TopScorerDto>
        {
            Data = data,
            PageNumber = page.SafePageNumber,
            PageSize = page.SafePageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)page.SafePageSize)
        };
    }
}
