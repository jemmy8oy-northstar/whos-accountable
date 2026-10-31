using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Abstractions.DomainModels;

namespace Balenthiran.WhosAccountable.WebApi;

/// <summary>
/// The cohort and its scores, computed once at startup and held in memory. There is no database:
/// the dataset is committed data-as-code and scoring is a pure function of it (DESIGN.md), so
/// nothing can change between requests.
/// </summary>
public sealed class ScoreboardStore
{
    private readonly Dictionary<LeaderboardDirection, IReadOnlyList<IScoredCompany>> _boards;
    private readonly Dictionary<string, IScoredCompany> _byId;

    public ScoreboardStore(ICohortDataset dataset, IScoreEngine engine)
    {
        Dataset = dataset;
        _boards = Enum.GetValues<LeaderboardDirection>()
            .ToDictionary(d => d, d => engine.BuildLeaderboard(dataset, d));
        _byId = _boards[LeaderboardDirection.WorstOffenders].ToDictionary(s => s.Company.Id);
    }

    public ICohortDataset Dataset { get; }

    public IReadOnlyList<IScoredCompany> Board(LeaderboardDirection direction) => _boards[direction];

    public IScoredCompany? Find(string id) => _byId.GetValueOrDefault(id);
}
