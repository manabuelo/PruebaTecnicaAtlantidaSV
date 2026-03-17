using System.Net;
using MundialCorporativo.Api.Tests.Support;

namespace MundialCorporativo.Api.Tests.Controllers;

public class StandingsControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public StandingsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStandings_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/standings");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetStandings_WithPagination_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/standings?pageNumber=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTopScorers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/standings/top-scorers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTopScorers_WithPagination_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/standings/top-scorers?pageNumber=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
