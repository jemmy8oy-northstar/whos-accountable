namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>
/// A pillar of the environmental score. Only <see cref="Emissions"/> and
/// <see cref="Violations"/> feed the v1 composite — both are sourced from open
/// government data (UK ETS; EA/Ofwat/EDM). <see cref="Trajectory"/> and
/// <see cref="Obstruction"/> are reserved: their sources (SBTi, InfluenceMap) are
/// licence-restricted for composite use (DESIGN.md licence audit, issue #9), so in
/// v1 they are shown only as attributed third-party signals, never scored here.
/// </summary>
public enum Pillar
{
    Emissions = 0,
    Violations,
    Trajectory,
    Obstruction,
}
