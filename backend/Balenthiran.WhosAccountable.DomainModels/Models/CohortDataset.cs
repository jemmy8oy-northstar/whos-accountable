namespace Balenthiran.WhosAccountable.DomainModels.Models;

/// <summary>
/// The full versioned input to the scoring engine: the cohort of companies plus every
/// open-government record about them. This is the in-repo "data-as-code" payload
/// (DESIGN.md) — one deserialised bundle from which the entire leaderboard is a pure,
/// reproducible function.
/// </summary>
/// <param name="Companies">The fixed cohort.</param>
/// <param name="Emissions">All emissions records, across companies and years.</param>
/// <param name="Violations">All violation records, across companies and years.</param>
public sealed record CohortDataset(
    IReadOnlyList<Company> Companies,
    IReadOnlyList<EmissionsRecord> Emissions,
    IReadOnlyList<ViolationRecord> Violations)
{
    public IReadOnlyList<Company> Companies { get; init; } = Companies ?? [];
    public IReadOnlyList<EmissionsRecord> Emissions { get; init; } = Emissions ?? [];
    public IReadOnlyList<ViolationRecord> Violations { get; init; } = Violations ?? [];
}

/// <summary>
/// A company paired with its computed score and its position on a leaderboard.
/// <see cref="Rank"/> is 1-based within the direction the board was built for.
/// </summary>
public sealed record ScoredCompany(
    int Rank,
    Company Company,
    CompositeScore Score);

/// <summary>Which end of the leaderboard to build.</summary>
public enum LeaderboardDirection
{
    /// <summary>Best performers first (highest composite score). The "do-gooders" board.</summary>
    TopPerformers = 0,

    /// <summary>Worst performers first (lowest composite score). The "worst-offenders" board.</summary>
    WorstOffenders,
}
