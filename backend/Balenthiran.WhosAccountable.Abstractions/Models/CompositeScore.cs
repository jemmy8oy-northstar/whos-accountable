namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>
/// A company's overall environmental score, 0–100, computed as the weight-renormalised
/// average of the pillars that had data. <see cref="Value"/> is null when no pillar
/// scored (total opacity) — again, never imputed to a neutral number. Callers get the
/// full <see cref="Pillars"/> breakdown and the coverage counts so the UI can be honest
/// about how much of the picture is actually known.
/// </summary>
/// <param name="CompanyId">The scored company.</param>
/// <param name="Value">Composite score in [0,100], or null if no pillar had data.</param>
/// <param name="Pillars">Per-pillar breakdown, including unknown pillars.</param>
public sealed record CompositeScore(
    string CompanyId,
    double? Value,
    IReadOnlyList<PillarScore> Pillars)
{
    /// <summary>Number of pillars that had data and contributed to <see cref="Value"/>.</summary>
    public int CoveredPillars => Pillars.Count(p => p.Contributed);

    /// <summary>Total number of composite pillars considered (covered or unknown).</summary>
    public int TotalPillars => Pillars.Count;
}
