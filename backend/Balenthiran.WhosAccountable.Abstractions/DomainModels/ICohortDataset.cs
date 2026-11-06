namespace Balenthiran.WhosAccountable.Abstractions.DomainModels;

/// <summary>
/// The full versioned input to the scoring engine: the cohort of companies plus every
/// open-government record about them. One deserialised bundle from which the entire leaderboard is
/// a pure, reproducible function (DESIGN.md "data-as-code").
/// </summary>
public interface ICohortDataset
{
    IReadOnlyList<ICompany> Companies { get; }
    IReadOnlyList<IEmissionsRecord> Emissions { get; }
    IReadOnlyList<IViolationRecord> Violations { get; }

    /// <summary>
    /// The sectors whose enforcement registers this dataset actually holds. Only a company in one
    /// of them can score a clean 100 on violations by having no record. Anywhere else, no record
    /// means nobody looked, so the pillar is unknown (BEH-SCORE-4).
    /// </summary>
    IReadOnlyList<Sector> ViolationsCoverage { get; }
}
