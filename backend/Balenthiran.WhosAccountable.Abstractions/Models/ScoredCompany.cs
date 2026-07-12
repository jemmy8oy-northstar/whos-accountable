namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>
/// A company paired with its computed score and its position on a leaderboard.
/// <see cref="Rank"/> is 1-based within the direction the board was built for.
/// </summary>
public sealed record ScoredCompany(
    int Rank,
    Company Company,
    CompositeScore Score);
