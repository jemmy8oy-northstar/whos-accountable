namespace Balenthiran.WhosAccountable.Abstractions.DomainModels;

/// <summary>
/// A single recorded environmental enforcement event against a company, from an open-government
/// source (Environment Agency, Ofwat, or Event Duration Monitoring). Each event carries its own
/// <see cref="Source"/> and a human-readable <see cref="Description"/> for citation.
/// </summary>
public interface IViolationRecord
{
    string CompanyId { get; }
    int Year { get; }
    decimal FineGbp { get; }
    int IncidentCount { get; }
    double? DischargeHours { get; }
    string Description { get; }
    ISourceRef Source { get; }
}
