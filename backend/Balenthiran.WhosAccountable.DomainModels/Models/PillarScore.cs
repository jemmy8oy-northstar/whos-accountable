using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Abstractions.DomainModels;

namespace Balenthiran.WhosAccountable.DomainModels.Models;

/// <summary>
/// The outcome of scoring one pillar for one company. <see cref="Value"/> is null when
/// the underlying source has no data for this company — an <em>unknown</em>, which the
/// composite excludes from its weighted average rather than treating as a neutral 50
/// (DESIGN.md). <see cref="Contributed"/> is the same fact as a convenience flag.
/// <see cref="Note"/> explains a null (e.g. "no emissions intensity disclosed") so the
/// methodology page can show why a company is unscored on a pillar.
/// </summary>
/// <param name="Pillar">Which pillar this is.</param>
/// <param name="Value">Sub-score in [0,100] where 100 = best, or null if unknown.</param>
/// <param name="Weight">The pillar's published weight in the composite.</param>
/// <param name="Note">Human-readable explanation, especially for a null value.</param>
public sealed record PillarScore(
    Pillar Pillar,
    double? Value,
    double Weight,
    string? Note = null) : IPillarScore
{
    /// <summary>True when this pillar had data and fed the composite.</summary>
    public bool Contributed => Value.HasValue;
}
