using System.Net;
using System.Text;
using System.Text.Json;
using MundialCorporativo.Api.Tests.Support;

namespace MundialCorporativo.Api.Tests.Controllers;

public class MatchesControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public MatchesControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET /api/matches ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetMatches_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/matches");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── GET /api/matches/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task GetMatchById_Unknown_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/matches/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMatchById_AfterCreate_ReturnsOk()
    {
        var (homeId, awayId) = await CreateTwoTeamsAsync("Home A", "Away A");
        var (_, matchId) = await CreateMatchAsync(homeId, awayId);
        var response = await _client.GetAsync($"/api/matches/{matchId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── POST /api/matches ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateMatch_WithoutIdempotencyKey_ReturnsBadRequest()
    {
        var (homeId, awayId) = await CreateTwoTeamsAsync("Home B", "Away B");
        var content = Json(new { homeTeamId = homeId, awayTeamId = awayId, matchDateUtc = DateTime.UtcNow });
        var response = await _client.PostAsync("/api/matches", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_SameTeams_ReturnsBadRequest()
    {
        var (_, teamId) = await CreateTeamAsync("Lone Team");
        var req = PostWithKey("/api/matches",
            Json(new { homeTeamId = teamId, awayTeamId = teamId, matchDateUtc = DateTime.UtcNow }),
            Guid.NewGuid().ToString());
        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_TeamsNotFound_ReturnsNotFound()
    {
        var req = PostWithKey("/api/matches",
            Json(new { homeTeamId = Guid.NewGuid(), awayTeamId = Guid.NewGuid(), matchDateUtc = DateTime.UtcNow }),
            Guid.NewGuid().ToString());
        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_WithValidKey_ReturnsCreated()
    {
        var (homeId, awayId) = await CreateTwoTeamsAsync("Home C", "Away C");
        var (status, id) = await CreateMatchAsync(homeId, awayId);
        Assert.Equal(HttpStatusCode.Created, status);
        Assert.NotEqual(Guid.Empty, id);
    }

    // ── POST /api/matches/{id}/result ─────────────────────────────────────────

    [Fact]
    public async Task RegisterResult_ReturnsOk()
    {
        var (homeId, awayId) = await CreateTwoTeamsAsync("Home D", "Away D");
        var (_, matchId) = await CreateMatchAsync(homeId, awayId);

        var req = PostWithKey($"/api/matches/{matchId}/result",
            Json(new { homeScore = 2, awayScore = 1, goals = Array.Empty<object>() }),
            Guid.NewGuid().ToString());
        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterResult_NegativeScore_ReturnsBadRequest()
    {
        var (homeId, awayId) = await CreateTwoTeamsAsync("Home E", "Away E");
        var (_, matchId) = await CreateMatchAsync(homeId, awayId);

        var req = PostWithKey($"/api/matches/{matchId}/result",
            Json(new { homeScore = -1, awayScore = 0, goals = Array.Empty<object>() }),
            Guid.NewGuid().ToString());
        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── PATCH /api/matches/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task PatchMatch_Reschedule_ReturnsOk()
    {
        var (homeId, awayId) = await CreateTwoTeamsAsync("Home F", "Away F");
        var (_, matchId) = await CreateMatchAsync(homeId, awayId);

        var newDate = DateTime.UtcNow.AddDays(7);
        var response = await _client.PatchAsync($"/api/matches/{matchId}",
            Json(new { matchDateUtc = newDate, cancelled = (bool?)null }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PatchMatch_Cancel_ReturnsOk()
    {
        var (homeId, awayId) = await CreateTwoTeamsAsync("Home G", "Away G");
        var (_, matchId) = await CreateMatchAsync(homeId, awayId);

        var response = await _client.PatchAsync($"/api/matches/{matchId}",
            Json(new { matchDateUtc = (DateTime?)null, cancelled = true }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── DELETE /api/matches/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteMatch_ReturnsNoContent()
    {
        var (homeId, awayId) = await CreateTwoTeamsAsync("Home H", "Away H");
        var (_, matchId) = await CreateMatchAsync(homeId, awayId);

        var response = await _client.DeleteAsync($"/api/matches/{matchId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<(Guid homeId, Guid awayId)> CreateTwoTeamsAsync(string home, string away)
    {
        var homeId = await CreateTeamIdAsync(home);
        var awayId = await CreateTeamIdAsync(away);
        return (homeId, awayId);
    }

    private async Task<Guid> CreateTeamIdAsync(string name)
    {
        var req = PostWithKey("/api/teams", Json(new { name }), Guid.NewGuid().ToString());
        var resp = await _client.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<(HttpStatusCode, Guid id)> CreateTeamAsync(string name)
    {
        var req = PostWithKey("/api/teams", Json(new { name }), Guid.NewGuid().ToString());
        var resp = await _client.SendAsync(req);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return (resp.StatusCode, doc.RootElement.GetProperty("id").GetGuid());
    }

    private async Task<(HttpStatusCode status, Guid id)> CreateMatchAsync(Guid homeId, Guid awayId)
    {
        var req = PostWithKey("/api/matches",
            Json(new { homeTeamId = homeId, awayTeamId = awayId, matchDateUtc = DateTime.UtcNow.AddDays(1) }),
            Guid.NewGuid().ToString());
        var response = await _client.SendAsync(req);
        if (!response.IsSuccessStatusCode) return (response.StatusCode, Guid.Empty);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (response.StatusCode, doc.RootElement.GetProperty("id").GetGuid());
    }

    private static StringContent Json(object payload) =>
        new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static HttpRequestMessage PostWithKey(string url, StringContent body, string key)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = body };
        req.Headers.Add("Idempotency-Key", key);
        return req;
    }
}
