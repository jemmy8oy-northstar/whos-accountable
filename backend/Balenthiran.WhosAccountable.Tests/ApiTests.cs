using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Balenthiran.WhosAccountable.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Balenthiran.WhosAccountable.Tests;

/// <summary>
/// Drives the read API in-process over the synthetic cohort (six companies, one of them fully
/// opaque), so the orderings are known. One test boots it over the committed real cohort with the
/// default configuration, which is how it runs outside tests.
/// </summary>
public sealed class ApiTests : IClassFixture<ApiTests.SyntheticApi>
{
    public sealed class SyntheticApi : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dataset:Path"] = Path.Combine(AppContext.BaseDirectory, "Fixtures", "synthetic-cohort.json"),
            }));
    }

    private readonly HttpClient _client;

    public ApiTests(SyntheticApi api) => _client = api.CreateClient();

    private async Task<JsonElement> Get(string url, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(expected, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static List<string> Ids(JsonElement board) =>
        board.EnumerateArray().Select(e => e.GetProperty("companyId").GetString()!).ToList();

    [Fact]
    public async Task BEH_LEADER_1_worst_board_is_lowest_composite_first_and_is_the_default()
    {
        var board = await Get("/api/leaderboard?direction=worst");

        Assert.Equal(
            ["thameswell-water", "northgale-energy", "ferrous-holdings", "middlemarch-foods", "verdigris-renewables", "opaque-industries"],
            Ids(board));
        Assert.Equal(Enumerable.Range(1, 6), board.EnumerateArray().Select(e => e.GetProperty("rank").GetInt32()));
        Assert.Equal(Ids(board), Ids(await Get("/api/leaderboard")));
    }

    [Fact]
    public async Task BEH_LEADER_2_best_board_is_highest_composite_first()
    {
        var board = await Get("/api/leaderboard?direction=best");

        Assert.Equal(
            ["verdigris-renewables", "middlemarch-foods", "ferrous-holdings", "northgale-energy", "thameswell-water", "opaque-industries"],
            Ids(board));
    }

    [Theory]
    [InlineData("worst")]
    [InlineData("best")]
    public async Task BEH_SCORE_4_unknown_composite_is_null_and_sorts_after_every_scored_company(string direction)
    {
        var board = (await Get($"/api/leaderboard?direction={direction}")).EnumerateArray().ToList();

        var last = board[^1];
        Assert.Equal("opaque-industries", last.GetProperty("companyId").GetString());
        Assert.Equal(JsonValueKind.Null, last.GetProperty("composite").ValueKind);
        Assert.All(last.GetProperty("pillars").EnumerateArray(),
            p => Assert.Equal(JsonValueKind.Null, p.GetProperty("value").ValueKind));
        Assert.All(board.Take(board.Count - 1), e => Assert.NotEqual(JsonValueKind.Null, e.GetProperty("composite").ValueKind));
    }

    [Fact]
    public async Task BEH_API_1_sector_filter_keeps_one_sector_and_reranks_from_one()
    {
        var board = await Get("/api/leaderboard?direction=worst&sector=EnergyUtilities");

        Assert.Equal(["northgale-energy", "verdigris-renewables"], Ids(board));
        Assert.Equal([1, 2], board.EnumerateArray().Select(e => e.GetProperty("rank").GetInt32()));
        Assert.All(board.EnumerateArray(), e => Assert.Equal("EnergyUtilities", e.GetProperty("sector").GetString()));
    }

    [Theory]
    [InlineData("/api/leaderboard?direction=sideways")]
    [InlineData("/api/leaderboard?sector=NotASector")]
    public async Task BEH_API_1_bad_direction_or_sector_is_a_400(string url)
    {
        await Get(url, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BEH_API_1_company_returns_score_pillars_and_source_evidence()
    {
        var company = await Get("/api/companies/thameswell-water");

        Assert.Equal("Thameswell Water", company.GetProperty("name").GetString());
        Assert.Equal(28.16, company.GetProperty("composite").GetDouble());
        Assert.Equal(2, company.GetProperty("pillars").GetArrayLength());
        Assert.Equal(2, company.GetProperty("emissions").GetArrayLength());
        var violation = company.GetProperty("violations")[0];
        Assert.Equal("EventDurationMonitoring", violation.GetProperty("source").GetProperty("kind").GetString());
        Assert.StartsWith("https://", violation.GetProperty("source").GetProperty("url").GetString());
    }

    [Fact]
    public async Task BEH_API_1_unknown_company_is_a_404()
    {
        await Get("/api/companies/no-such-company", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BEH_API_1_status_reports_the_loaded_cohort()
    {
        var status = await Get("/api/status");

        Assert.Equal("ok", status.GetProperty("status").GetString());
        Assert.Equal(6, status.GetProperty("companies").GetInt32());
    }

    [Fact]
    public async Task BEH_SCORE_5_methodology_publishes_the_real_weights_and_constants()
    {
        var m = await Get("/api/methodology");

        Assert.Equal(ScoringConstants.EmissionsWeight, m.GetProperty("weights").GetProperty("Emissions").GetDouble());
        Assert.Equal(ScoringConstants.ViolationsWeight, m.GetProperty("weights").GetProperty("Violations").GetDouble());
        var constants = m.GetProperty("constants");
        Assert.Equal(ScoringConstants.EmissionsIntensityScale, constants.GetProperty("EmissionsIntensityScale").GetDouble());
        Assert.Equal(ScoringConstants.ViolationHarmScale, constants.GetProperty("ViolationHarmScale").GetDouble());
        Assert.Equal(ScoringConstants.ScoreDecimals, m.GetProperty("scoreDecimals").GetInt32());
        Assert.Equal(6, m.GetProperty("companyCount").GetInt32());
        Assert.True(m.GetProperty("sources").GetArrayLength() > 0);
    }

    [Fact]
    public async Task BEH_API_1_default_configuration_serves_the_committed_real_cohort()
    {
        using var api = new WebApplicationFactory<Program>();
        var board = await api.CreateClient().GetFromJsonAsync<JsonElement>("/api/leaderboard");

        Assert.Equal(10, board.GetArrayLength());
    }
}
