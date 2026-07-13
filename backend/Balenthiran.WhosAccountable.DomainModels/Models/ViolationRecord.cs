using Balenthiran.WhosAccountable.Abstractions.DomainModels;

namespace Balenthiran.WhosAccountable.DomainModels.Models;

/// <summary>
/// A single recorded environmental enforcement event against a company, from an
/// open-government source (Environment Agency, Ofwat, or Event Duration Monitoring).
/// This is the strongest, most UK-specific pillar (DESIGN.md). Each event carries its
/// own <see cref="Source"/> and a human-readable <see cref="Description"/> so the
/// company page can cite it directly.
/// </summary>
/// <param name="CompanyId">Cohort company this event is attributed to.</param>
/// <param name="Year">Year the penalty / incident was recorded.</param>
/// <param name="FineGbp">Monetary penalty in GBP (0 if the event carried no fine).</param>
/// <param name="IncidentCount">Number of distinct incidents this record represents (default 1).</param>
/// <param name="DischargeHours">Sewage / storm-overflow discharge hours (EDM data), where applicable.</param>
/// <param name="Description">Short public description of the event.</param>
/// <param name="Source">Where this record came from.</param>
public sealed record ViolationRecord(
    string CompanyId,
    int Year,
    decimal FineGbp,
    int IncidentCount,
    double? DischargeHours,
    string Description,
    SourceRef Source) : IViolationRecord
{
    public int IncidentCount { get; init; } = IncidentCount <= 0 ? 1 : IncidentCount;

    ISourceRef IViolationRecord.Source => Source;
}
