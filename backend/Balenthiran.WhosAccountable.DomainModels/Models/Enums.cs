namespace Balenthiran.WhosAccountable.DomainModels.Models;

/// <summary>
/// Broad sector bucket for a cohort company. Used only for the leaderboard's sector
/// filter and to group like-for-like comparisons — it is deliberately coarse, not a
/// full industry taxonomy. Extend as the cohort (DESIGN.md) grows beyond FTSE 100 +
/// UK utilities + Carbon Majors.
/// </summary>
public enum Sector
{
    Unknown = 0,
    WaterUtilities,
    EnergyUtilities,
    OilAndGas,
    Mining,
    Financials,
    ConsumerGoods,
    Industrials,
    Other,
}

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

/// <summary>
/// A published dataset a record was extracted from — carried on every record so that
/// each number in the product links back to a public source (the entire credibility
/// story of the app, DESIGN.md). Open-government sources only in v1.
/// </summary>
public enum SourceKind
{
    /// <summary>UK Emissions Trading Scheme registry — verified absolute emissions. Open data.</summary>
    UkEtsRegistry = 0,

    /// <summary>Environment Agency enforcement / prosecution records. Open Government Licence.</summary>
    EnvironmentAgency,

    /// <summary>Ofwat penalties against water companies. Open Government Licence.</summary>
    Ofwat,

    /// <summary>Event Duration Monitoring — storm-overflow / sewage discharge data. Open Government Licence.</summary>
    EventDurationMonitoring,
}
