using Balenthiran.WhosAccountable.DomainModels.Models;

namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>
/// Turns a versioned <see cref="CohortDataset"/> into scores. The whole engine is a pure
/// function — no clock, no randomness, no I/O — so the same input always yields the same
/// output and a golden-file test can pin the entire leaderboard (DESIGN.md). Every number
/// it produces is reproducible by inspection from the published weights/constants and the
/// in-repo data.
/// </summary>
public interface IScoreEngine
{
    /// <summary>Scores one company from its records within the dataset.</summary>
    CompositeScore ScoreCompany(Company company, CohortDataset dataset);

    /// <summary>
    /// Scores every company in the dataset and orders them into a leaderboard.
    /// Companies with no scorable pillar (a null composite) are always sorted to the end,
    /// regardless of direction — an unknown company is neither a top performer nor a
    /// worst offender.
    /// </summary>
    IReadOnlyList<ScoredCompany> BuildLeaderboard(CohortDataset dataset, LeaderboardDirection direction);
}
