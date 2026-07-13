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
}
