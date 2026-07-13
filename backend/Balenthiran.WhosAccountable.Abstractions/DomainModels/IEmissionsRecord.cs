namespace Balenthiran.WhosAccountable.Abstractions.DomainModels;

/// <summary>
/// One company-year of disclosed greenhouse-gas emissions from a published source.
/// <see cref="IntensityTco2ePerGbpMillion"/> is what the emissions pillar scores; it is nullable
/// so a company that discloses tonnage but not revenue stays <em>unknown</em> rather than imputed.
/// </summary>
public interface IEmissionsRecord
{
    string CompanyId { get; }
    int Year { get; }
    double AbsoluteTco2e { get; }
    double? IntensityTco2ePerGbpMillion { get; }
    ISourceRef Source { get; }
}
