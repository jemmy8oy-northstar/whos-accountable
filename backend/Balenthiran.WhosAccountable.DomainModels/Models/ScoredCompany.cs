using Balenthiran.WhosAccountable.Abstractions.DomainModels;

namespace Balenthiran.WhosAccountable.DomainModels.Models;

/// <summary>
/// A company paired with its computed score and its position on a leaderboard.
/// <see cref="Rank"/> is 1-based within the direction the board was built for. Holds the
/// abstractions (<see cref="ICompany"/>/<see cref="ICompositeScore"/>) rather than the concretes
/// so nothing concrete escapes the engine to a caller.
/// </summary>
public sealed record ScoredCompany(
    int Rank,
    ICompany Company,
    ICompositeScore Score) : IScoredCompany;
