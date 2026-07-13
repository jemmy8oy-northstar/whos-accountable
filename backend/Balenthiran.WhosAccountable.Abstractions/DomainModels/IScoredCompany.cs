namespace Balenthiran.WhosAccountable.Abstractions.DomainModels;

/// <summary>
/// A company paired with its computed score and its position on a leaderboard.
/// <see cref="Rank"/> is 1-based within the direction the board was built for.
/// </summary>
public interface IScoredCompany
{
    int Rank { get; }
    ICompany Company { get; }
    ICompositeScore Score { get; }
}
