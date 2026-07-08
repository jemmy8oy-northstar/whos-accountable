using Balenthiran.WhosAccountable.DomainModels.Models;
using Balenthiran.WhosAccountable.Services.Scoring;

namespace Balenthiran.WhosAccountable.Tests;

/// <summary>
/// Behavioural unit tests for <see cref="ScoreEngine"/> — the two design rules that make the
/// product trustworthy (missing disclosure is never a neutral fudge; renormalise over the
/// pillars you actually have) plus leaderboard ordering. The exact numeric values are pinned
/// separately by the golden-file test; here we assert the *rules*.
/// </summary>
public sealed class ScoreEngineTests
{
    private readonly ScoreEngine _engine = new();

    [Fact]
    public void Company_with_no_records_at_all_is_unknown_not_neutral()
    {
        var company = Fixtures.Company("ghost");
        var dataset = Fixtures.Dataset([company]);

        var score = _engine.ScoreCompany(company, dataset);

        Assert.Null(score.Value);
        Assert.Equal(0, score.CoveredPillars);
        Assert.All(score.Pillars, p => Assert.Null(p.Value));
    }

    [Fact]
    public void Emissions_without_derivable_intensity_leaves_that_pillar_unknown()
    {
        var company = Fixtures.Company("tonnage-only");
        // Discloses absolute tonnage but no intensity — not comparable, so unknown, not a middling 50.
        var dataset = Fixtures.Dataset(
            [company],
            emissions: [Fixtures.Emissions("tonnage-only", 2024, intensity: null, absolute: 500000)],
            violations: [Fixtures.Violation("tonnage-only", 2024, fineGbp: 1_000_000)]);

        var score = _engine.ScoreCompany(company, dataset);
        var emissions = score.Pillars.Single(p => p.Pillar == Pillar.Emissions);

        Assert.Null(emissions.Value);
        Assert.False(emissions.Contributed);
    }

    [Fact]
    public void Composite_renormalises_over_only_the_present_pillars()
    {
        var company = Fixtures.Company("violations-only");
        // No emissions intensity → emissions pillar drops out; the composite must equal the
        // single surviving pillar exactly, never a weight-diluted value.
        var dataset = Fixtures.Dataset(
            [company],
            violations: [Fixtures.Violation("violations-only", 2024, fineGbp: 3_000_000, incidents: 2)]);

        var score = _engine.ScoreCompany(company, dataset);
        var violations = score.Pillars.Single(p => p.Pillar == Pillar.Violations);

        Assert.Equal(1, score.CoveredPillars);
        Assert.NotNull(violations.Value);
        Assert.Equal(violations.Value, score.Value);
    }

    [Fact]
    public void No_enforcement_but_has_emissions_scores_a_clean_hundred_on_violations()
    {
        var company = Fixtures.Company("clean-emitter");
        var dataset = Fixtures.Dataset(
            [company],
            emissions: [Fixtures.Emissions("clean-emitter", 2024, intensity: 50)]);

        var score = _engine.ScoreCompany(company, dataset);
        var violations = score.Pillars.Single(p => p.Pillar == Pillar.Violations);

        Assert.Equal(100.0, violations.Value);
        Assert.Equal(2, score.CoveredPillars);
    }

    [Fact]
    public void More_enforcement_harm_yields_a_lower_violations_score()
    {
        var light = Fixtures.Company("light");
        var heavy = Fixtures.Company("heavy");
        var dataset = Fixtures.Dataset(
            [light, heavy],
            violations:
            [
                Fixtures.Violation("light", 2024, fineGbp: 100_000, incidents: 1),
                Fixtures.Violation("heavy", 2024, fineGbp: 8_000_000, incidents: 6, dischargeHours: 10_000),
            ]);

        var lightV = _engine.ScoreCompany(light, dataset).Pillars.Single(p => p.Pillar == Pillar.Violations).Value;
        var heavyV = _engine.ScoreCompany(heavy, dataset).Pillars.Single(p => p.Pillar == Pillar.Violations).Value;

        Assert.NotNull(lightV);
        Assert.NotNull(heavyV);
        Assert.True(heavyV < lightV, $"expected heavier offender to score lower ({heavyV} < {lightV})");
    }

    [Fact]
    public void Every_score_stays_within_zero_to_one_hundred()
    {
        var dataset = LoadedSyntheticDataset();

        foreach (var company in dataset.Companies)
        {
            var score = _engine.ScoreCompany(company, dataset);
            if (score.Value is { } v)
                Assert.InRange(v, 0.0, 100.0);
            foreach (var pillar in score.Pillars.Where(p => p.Value.HasValue))
                Assert.InRange(pillar.Value!.Value, 0.0, 100.0);
        }
    }

    [Fact]
    public void Unknown_companies_sort_last_in_both_directions()
    {
        var dataset = LoadedSyntheticDataset();

        var worst = _engine.BuildLeaderboard(dataset, LeaderboardDirection.WorstOffenders);
        var top = _engine.BuildLeaderboard(dataset, LeaderboardDirection.TopPerformers);

        Assert.Null(worst[^1].Score.Value);
        Assert.Null(top[^1].Score.Value);
        // The worst board runs ascending, the top board descending, over the same scored set.
        Assert.Equal(
            worst.Where(r => r.Score.Value.HasValue).Select(r => r.Company.Id),
            top.Where(r => r.Score.Value.HasValue).Select(r => r.Company.Id).Reverse());
    }

    [Fact]
    public void Ranks_are_dense_and_one_based()
    {
        var dataset = LoadedSyntheticDataset();

        var board = _engine.BuildLeaderboard(dataset, LeaderboardDirection.WorstOffenders);

        Assert.Equal(Enumerable.Range(1, board.Count), board.Select(r => r.Rank));
    }

    [Fact]
    public void Scoring_is_deterministic()
    {
        var dataset = LoadedSyntheticDataset();

        var first = _engine.BuildLeaderboard(dataset, LeaderboardDirection.WorstOffenders);
        var second = _engine.BuildLeaderboard(dataset, LeaderboardDirection.WorstOffenders);

        Assert.Equal(
            first.Select(r => (r.Rank, r.Company.Id, r.Score.Value)),
            second.Select(r => (r.Rank, r.Company.Id, r.Score.Value)));
    }

    [Fact]
    public void ScoreCompany_rejects_null_arguments()
    {
        var company = Fixtures.Company("x");
        var dataset = Fixtures.Dataset([company]);

        Assert.Throws<ArgumentNullException>(() => _engine.ScoreCompany(null!, dataset));
        Assert.Throws<ArgumentNullException>(() => _engine.ScoreCompany(company, null!));
    }

    private static CohortDataset LoadedSyntheticDataset() =>
        new Services.Datasets.DatasetLoader().Load(Fixtures.ReadText("synthetic-cohort.json"));
}
