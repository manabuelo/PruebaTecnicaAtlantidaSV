namespace MundialCorporativo.Api.Contracts;

public record PaginationRequest(int PageNumber = 1, int PageSize = 10, string? SortBy = null, string? SortDirection = null);

public record TeamCreateRequest(string Name);
public record TeamUpdateRequest(string Name);
public record TeamPatchRequest(string? Name);

public record PlayerCreateRequest(string FullName, int JerseyNumber);
public record PlayerUpdateRequest(string FullName, int JerseyNumber);
public record PlayerPatchRequest(string? FullName, int? JerseyNumber);

public record MatchCreateRequest(Guid HomeTeamId, Guid AwayTeamId, DateTime MatchDateUtc);
public record MatchPatchRequest(DateTime? MatchDateUtc, bool? Cancelled);
public record RegisterMatchResultRequest(int HomeScore, int AwayScore, IReadOnlyCollection<RegisterGoalRequest>? Goals);
public record RegisterGoalRequest(Guid PlayerId, int Goals);
