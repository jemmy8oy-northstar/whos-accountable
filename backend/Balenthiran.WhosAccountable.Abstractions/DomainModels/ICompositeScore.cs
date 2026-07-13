namespace Balenthiran.WhosAccountable.Abstractions.DomainModels;

/// <summary>
/// A company's overall environmental score, 0–100, computed as the weight-renormalised average of
/// the pillars that had data. <see cref="Value"/> is null under total opacity — never imputed.
/// Callers get the full <see cref="Pillars"/> breakdown and coverage counts so the UI can be honest
/// about how much of the picture is actually known.
/// </summary>
public interface ICompositeScore
{
    string CompanyId { get; }
    double? Value { get; }
    IReadOnlyList<IPillarScore> Pillars { get; }

    /// <summary>Number of pillars that had data and contributed to <see cref="Value"/>.</summary>
    int CoveredPillars { get; }

    /// <summary>Total number of composite pillars considered (covered or unknown).</summary>
    int TotalPillars { get; }
}
