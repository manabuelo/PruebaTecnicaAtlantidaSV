using System.Net;
using System.Text;
using System.Text.Json;
using MundialCorporativo.Api.Tests.Support;

namespace MundialCorporativo.Api.Tests.Controllers;

public class PlayersControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public PlayersControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET /api/players ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetPlayers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/players");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── GET /api/players/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task GetPlayerById_Unknown_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/players/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlayerById_AfterCreate_ReturnsOk()
    {
        var (_, teamId) = await CreateTeamAsync("Player Team");
        var (_, playerId) = await CreatePlayerAsync(teamId, "John Doe", 9);
        var response = await _client.GetAsync($"/api/players/{playerId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── POST /api/players/teams/{teamId} ──────────────────────────────────────

    [Fact]
    public async Task CreatePlayer_WithoutIdempotencyKey_ReturnsBadRequest()
    {
        var (_, teamId) = await CreateTeamAsync("Team No Key");
        var content = Json(new { fullName = "Ghost Player", jerseyNumber = 1 });
        var response = await _client.PostAsync($"/api/players/teams/{teamId}", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlayer_TeamNotFound_ReturnsNotFound()
    {
        var req = PostWithKey($"/api/players/teams/{Guid.NewGuid()}",
            Json(new { fullName = "Ghost", jerseyNumber = 7 }),
            Guid.NewGuid().ToString());
        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlayer_WithValidKey_ReturnsCreated()
    {
        var (_, teamId) = await CreateTeamAsync("Create Player Team");
        var (status, id) = await CreatePlayerAsync(teamId, "New Player", 11);
        Assert.Equal(HttpStatusCode.Created, status);
        Assert.NotEqual(Guid.Empty, id);
    }

    // ── PUT /api/players/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePlayer_ReturnsOk()
    {
        var (_, teamId) = await CreateTeamAsync("Update Player Team");
        var (_, playerId) = await CreatePlayerAsync(teamId, "Original Name", 5);
        var response = await _client.PutAsync($"/api/players/{playerId}",
            Json(new { fullName = "Updated Name", jerseyNumber = 5 }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── PATCH /api/players/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task PatchPlayer_ReturnsOk()
    {
        var (_, teamId) = await CreateTeamAsync("Patch Player Team");
        var (_, playerId) = await CreatePlayerAsync(teamId, "Patch Target", 3);
        var response = await _client.PatchAsync($"/api/players/{playerId}",
            Json(new { fullName = "Patched Name" }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── DELETE /api/players/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task DeletePlayer_ReturnsNoContent()
    {
        var (_, teamId) = await CreateTeamAsync("Delete Player Team");
        var (_, playerId) = await CreatePlayerAsync(teamId, "Delete Me", 2);
        var response = await _client.DeleteAsync($"/api/players/{playerId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<(HttpStatusCode status, Guid id)> CreateTeamAsync(string name)
    {
        var req = PostWithKey("/api/teams", Json(new { name }), Guid.NewGuid().ToString());
        var response = await _client.SendAsync(req);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (response.StatusCode, doc.RootElement.GetProperty("id").GetGuid());
    }

    private async Task<(HttpStatusCode status, Guid id)> CreatePlayerAsync(Guid teamId, string fullName, int jersey)
    {
        var req = PostWithKey($"/api/players/teams/{teamId}",
            Json(new { fullName, jerseyNumber = jersey }),
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
