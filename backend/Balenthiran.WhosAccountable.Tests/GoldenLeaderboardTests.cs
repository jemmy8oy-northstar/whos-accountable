using System.Text.Json;
using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Abstractions.DomainModels;
using Balenthiran.WhosAccountable.Services.Datasets;
using Balenthiran.WhosAccountable.Services.Scoring;

namespace Balenthiran.WhosAccountable.Tests;

/// <summary>
/// The golden-file test: it pins the ENTIRE leaderboard the engine produces over the synthetic
/// cohort, both directions, to a committed snapshot. Because the engine is a pure function of
/// the in-repo data and the published constants, any change to a weight, scale, or formula
/// turns into a reviewable diff of this file — that visible-diff property is the whole point of
/// "data-as-code" scoring (DESIGN.md). The cohort is fictional, so a wrong pin can never mean a
/// wrong published figure about a real company.
///
/// To regenerate after an intentional change: run the tests, then copy the
/// <c>synthetic-leaderboard.actual.json</c> written beside the test assembly over the committed
/// <c>Fixtures/synthetic-leaderboard.golden.json</c> and review the diff.
/// </summary>
public sealed class GoldenLeaderboardTests
{
    private static readonly JsonSerializerOptions SnapshotJson = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public void Leaderboard_matches_the_committed_golden_snapshot()
    {
        var dataset = new DatasetLoader().Load(Fixtures.ReadText("synthetic-cohort.json"));
        var engine = new ScoreEngine();

        var actual = new LeaderboardSnapshot(
            engine.BuildLeaderboard(dataset, LeaderboardDirection.WorstOffenders).Select(Project).ToList(),
            engine.BuildLeaderboard(dataset, LeaderboardDirection.TopPerformers).Select(Project).ToList());

        var actualJson = JsonSerializer.Serialize(actual, SnapshotJson);

        // Round-trip the committed golden through the same serializer so the comparison is on
        // values, not the file's hand-written whitespace/number formatting.
        var golden = JsonSerializer.Deserialize<LeaderboardSnapshot>(
            Fixtures.ReadText("synthetic-leaderboard.golden.json"), SnapshotJson)!;
        var goldenJson = JsonSerializer.Serialize(golden, SnapshotJson);

        if (actualJson != goldenJson)
        {
            // Leave the fresh output next to the assembly so a regen is a copy-and-review.
            File.WriteAllText(Fixtures.Path("synthetic-leaderboard.actual.json"), actualJson);
        }

        Assert.Equal(goldenJson, actualJson);
    }

    private static LeaderboardRow Project(IScoredCompany scored) => new(
        scored.Rank,
        scored.Company.Id,
        scored.Company.Name,
        scored.Score.Value,
        scored.Score.CoveredPillars,
        scored.Score.TotalPillars,
        scored.Score.Pillars.Select(p => new PillarRow(p.Pillar.ToString(), p.Value, p.Weight)).ToList());

    // Flat, serialisation-friendly projections of the scored leaderboard.
    private sealed record LeaderboardSnapshot(
        IReadOnlyList<LeaderboardRow> WorstOffenders,
        IReadOnlyList<LeaderboardRow> TopPerformers);

    private sealed record LeaderboardRow(
        int Rank,
        string Id,
        string Name,
        double? Score,
        int CoveredPillars,
        int TotalPillars,
        IReadOnlyList<PillarRow> Pillars);

    private sealed record PillarRow(string Pillar, double? Value, double Weight);
}
