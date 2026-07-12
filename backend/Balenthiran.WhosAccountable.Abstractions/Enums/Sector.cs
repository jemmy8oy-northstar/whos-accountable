namespace Balenthiran.WhosAccountable.Abstractions;

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
