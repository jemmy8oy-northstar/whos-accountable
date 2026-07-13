namespace Balenthiran.WhosAccountable.Abstractions.DomainModels;

/// <summary>
/// The outcome of scoring one pillar for one company. <see cref="Value"/> is null when the
/// underlying source has no data — an <em>unknown</em>, which the composite excludes rather than
/// treating as a neutral 50. <see cref="Contributed"/> is the same fact as a convenience flag.
/// </summary>
public interface IPillarScore
{
    Pillar Pillar { get; }
    double? Value { get; }
    double Weight { get; }
    string? Note { get; }

    /// <summary>True when this pillar had data and fed the composite.</summary>
    bool Contributed { get; }
}
