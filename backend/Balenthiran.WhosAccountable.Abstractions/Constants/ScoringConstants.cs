namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>
/// Every published number the v1 scoring formula depends on, in one place, as named
/// constants. DESIGN.md makes this load-bearing: weights and scales live in the repo,
/// so any change to how companies are scored is an explicit, reviewable diff (and the
/// golden-file test turns that diff into a visible leaderboard change). These are v1
/// values chosen for a sensible spread, not calibrated against the full cohort — that
/// calibration is a later, versioned change once real datasets land.
///
/// These live in Abstractions, not Services: they are the published scoring methodology
/// (a domain contract the future methodology page / read API will surface), not an
/// implementation detail of one service. They are fixed constants, not tunable runtime
/// config, so they are not in appsettings — changing them must be a reviewed code diff.
/// </summary>
public static class ScoringConstants
{
    // ---- composite weights (must be the only pillars in the v1 composite) ----

    /// <summary>Weight of the emissions pillar in the composite. Open-gov: UK ETS.</summary>
    public const double EmissionsWeight = 0.40;

    /// <summary>
    /// Weight of the violations pillar. Heaviest because EA/Ofwat/EDM enforcement is the
    /// most vivid, UK-specific and hardest-to-dispute open-government evidence (DESIGN.md).
    /// </summary>
    public const double ViolationsWeight = 0.60;

    // ---- emissions pillar ----

    /// <summary>
    /// Intensity (tCO2e per £m revenue) at which the emissions sub-score decays to 1/e
    /// (~36.8). Higher intensity → lower score, via score = 100·e^(-intensity/scale).
    /// </summary>
    public const double EmissionsIntensityScale = 200.0;

    // ---- violations pillar ----

    /// <summary>Harm units contributed per £1,000,000 of fines.</summary>
    public const double FineHarmPerMillionGbp = 1.0;

    /// <summary>Harm units contributed per recorded incident.</summary>
    public const double IncidentHarmPerCount = 0.5;

    /// <summary>Harm units contributed per 1,000 hours of sewage / storm-overflow discharge.</summary>
    public const double DischargeHarmPer1000Hours = 1.0;

    /// <summary>
    /// Total harm at which the violations sub-score decays to 1/e, via
    /// score = 100·e^(-harm/scale). A company with no recorded enforcement scores 100.
    /// </summary>
    public const double ViolationHarmScale = 5.0;

    // ---- presentation ----

    /// <summary>Decimal places all published scores are rounded to (keeps results stable and reproducible).</summary>
    public const int ScoreDecimals = 2;
}
