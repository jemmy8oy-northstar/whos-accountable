namespace Balenthiran.WhosAccountable.Abstractions;

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
