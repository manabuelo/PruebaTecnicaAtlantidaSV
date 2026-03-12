namespace MundialCorporativo.Application.DTOs;

public record TeamDto(Guid Id, string Name);

public record PlayerDto(Guid Id, Guid TeamId, string TeamName, string FullName, int JerseyNumber, int GoalsScored);

public record MatchDto(
    Guid Id,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTime MatchDateUtc,
    string Status,
    int? HomeScore,
    int? AwayScore);

public record StandingDto(Guid TeamId, string TeamName, int Played, int Won, int Drawn, int Lost, int GoalsFor, int GoalsAgainst, int GoalDifference, int Points);

public record TopScorerDto(Guid PlayerId, string PlayerName, string TeamName, int GoalsScored);
