namespace Balenthiran.WhosAccountable.Abstractions.DomainModels;

/// <summary>
/// A company in the versioned cohort. Identity is the stable, human-readable <see cref="Id"/>
/// slug; <see cref="Lei"/> / <see cref="CompaniesHouseNumber"/> are the authoritative external
/// identifiers used to reconcile records across sources.
/// </summary>
public interface ICompany
{
    string Id { get; }
    string Name { get; }
    Sector Sector { get; }
    IReadOnlyList<string> CohortTags { get; }
    string? Lei { get; }
    string? CompaniesHouseNumber { get; }
}
