namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>
/// A company in the versioned cohort (DESIGN.md: a fixed, in-repo list of ~150
/// UK-linked companies). Identity is a stable, human-readable <see cref="Id"/> slug
/// so datasets and URLs stay legible in diffs; <see cref="Lei"/> /
/// <see cref="CompaniesHouseNumber"/> are the authoritative external identifiers used
/// to reconcile records across sources.
/// </summary>
public sealed record Company(
    string Id,
    string Name,
    Sector Sector,
    IReadOnlyList<string> CohortTags,
    string? Lei = null,
    string? CompaniesHouseNumber = null)
{
    public IReadOnlyList<string> CohortTags { get; init; } = CohortTags ?? [];
}
