using Balenthiran.WhosAccountable.Abstractions.DomainModels;

namespace Balenthiran.WhosAccountable.DomainModels.Models;

/// <summary>
/// One company-year of disclosed greenhouse-gas emissions from a published source
/// (UK ETS registry in v1). <see cref="IntensityTco2ePerGbpMillion"/> is what the
/// emissions pillar actually scores — absolute tonnage is not comparable across
/// companies of wildly different size, whereas intensity (emissions per £m revenue)
/// is. Intensity is nullable: a company that discloses tonnage but not revenue leaves
/// the pillar <em>unknown</em> rather than being imputed to a neutral score
/// (DESIGN.md: missing data is never imputed to neutral).
/// </summary>
/// <param name="CompanyId">Cohort company this record belongs to.</param>
/// <param name="Year">Reporting year.</param>
/// <param name="AbsoluteTco2e">Verified absolute emissions, tonnes CO2-equivalent.</param>
/// <param name="IntensityTco2ePerGbpMillion">Emissions intensity, tCO2e per £m revenue, if derivable.</param>
/// <param name="Source">Where this record came from.</param>
public sealed record EmissionsRecord(
    string CompanyId,
    int Year,
    double AbsoluteTco2e,
    double? IntensityTco2ePerGbpMillion,
    SourceRef Source) : IEmissionsRecord
{
    ISourceRef IEmissionsRecord.Source => Source;
}
