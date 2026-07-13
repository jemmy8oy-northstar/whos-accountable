namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>Which end of the leaderboard to build.</summary>
public enum LeaderboardDirection
{
    /// <summary>Best performers first (highest composite score). The "do-gooders" board.</summary>
    TopPerformers = 0,

    /// <summary>Worst performers first (lowest composite score). The "worst-offenders" board.</summary>
    WorstOffenders,
}
