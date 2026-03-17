using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MundialCorporativo.Api.Tests.Support;

namespace MundialCorporativo.Api.Tests.Controllers;

public class TeamsControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public TeamsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET /api/teams ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTeams_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/teams");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── GET /api/teams/{id} ───────────────────────────────────────────────────

    [Fact]
    public async Task GetTeamById_Unknown_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/teams/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamById_AfterCreate_ReturnsOk()
    {
        var (_, id) = await CreateTeamAsync("Findable Team");
        var response = await _client.GetAsync($"/api/teams/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── POST /api/teams ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTeam_WithoutIdempotencyKey_ReturnsBadRequest()
    {
        var content = Json(new { name = "No Key Team" });
        var response = await _client.PostAsync("/api/teams", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_WithValidKey_ReturnsCreated()
    {
        var (status, id) = await CreateTeamAsync("New Team");
        Assert.Equal(HttpStatusCode.Created, status);
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task CreateTeam_SameIdempotencyKey_ReturnsCachedResponse()
    {
        var key = Guid.NewGuid().ToString();
        var content1 = Json(new { name = "Idempotent Team" });
        var req1 = PostWithKey("/api/teams", content1, key);
        var r1 = await _client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.Created, r1.StatusCode);

        // Same key, second call should return the cached Created response
        var content2 = Json(new { name = "Idempotent Team" });
        var req2 = PostWithKey("/api/teams", content2, key);
        var r2 = await _client.SendAsync(req2);
        Assert.Equal(HttpStatusCode.Created, r2.StatusCode);
    }

    // ── PUT /api/teams/{id} ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTeam_ReturnsOk()
    {
        var (_, id) = await CreateTeamAsync("Update Target");
        var response = await _client.PutAsync($"/api/teams/{id}", Json(new { name = "Renamed" }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTeam_NotFound_Returns404()
    {
        var response = await _client.PutAsync($"/api/teams/{Guid.NewGuid()}", Json(new { name = "Ghost" }));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── PATCH /api/teams/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task PatchTeam_ReturnsOk()
    {
        var (_, id) = await CreateTeamAsync("Patch Target");
        var response = await _client.PatchAsync($"/api/teams/{id}", Json(new { name = "Patched" }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── DELETE /api/teams/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTeam_ReturnsNoContent()
    {
        var (_, id) = await CreateTeamAsync("Delete Target");
        var response = await _client.DeleteAsync($"/api/teams/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    internal async Task<(HttpStatusCode status, Guid id)> CreateTeamAsync(string name)
    {
        var req = PostWithKey("/api/teams", Json(new { name }), Guid.NewGuid().ToString());
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
