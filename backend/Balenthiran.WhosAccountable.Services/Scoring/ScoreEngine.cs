using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Abstractions.DomainModels;
using Balenthiran.WhosAccountable.DomainModels.Models;

namespace Balenthiran.WhosAccountable.Services.Scoring;

/// <summary>
/// The v1 environmental scoring engine — a pure function from a versioned
/// <see cref="ICohortDataset"/> to scores. Only the two open-government pillars feed the
/// composite (emissions from UK ETS, violations from EA/Ofwat/EDM); the licence-restricted
/// signals (SBTi, InfluenceMap, CDP) are deliberately absent here (DESIGN.md / issue #9).
/// It consumes and returns only interfaces (nothing concrete leaks past the DI seam) but
/// constructs concrete domain records internally.
///
/// Two rules from the design shape everything:
/// <list type="bullet">
///   <item>Missing <em>disclosure</em> is never imputed to neutral. If a company discloses
///   no emissions intensity, the emissions pillar is <c>null</c> (unknown) and simply drops
///   out of the weighted average — it does not become a middling 50.</item>
///   <item>Missing <em>enforcement record</em> is not the same as missing disclosure. The
///   open-gov enforcement registers cover the whole cohort, so a company absent from them
///   has genuinely no recorded penalties and scores a clean 100 on violations.</item>
/// </list>
/// </summary>
public sealed class ScoreEngine : IScoreEngine
{
    public ICompositeScore ScoreCompany(ICompany company, ICohortDataset dataset)
    {
        ArgumentNullException.ThrowIfNull(company);
        ArgumentNullException.ThrowIfNull(dataset);

        var companyEmissions = dataset.Emissions.Where(e => e.CompanyId == company.Id).ToList();
        var companyViolations = dataset.Violations.Where(v => v.CompanyId == company.Id).ToList();

        // A clean violations record (100) is only meaningful if we hold *some* data for the
        // company — evidence it has actually been ingested and would show up in the registers
        // if it offended. A company with no records of any kind is a coverage gap, not a
        // do-gooder, so it stays fully unknown rather than being crowned by its own absence.
        var hasAnyData = companyEmissions.Count > 0 || companyViolations.Count > 0;

        var emissions = ScoreEmissions(companyEmissions);
        var violations = ScoreViolations(companyViolations, hasAnyData);

        var pillars = new[] { emissions, violations };
        return new CompositeScore(company.Id, Composite(pillars), pillars);
    }

    public IReadOnlyList<IScoredCompany> BuildLeaderboard(ICohortDataset dataset, LeaderboardDirection direction)
    {
        ArgumentNullException.ThrowIfNull(dataset);

        // Score everyone, then order. Unknown composites (null) always sort last, whichever
        // direction we're building — an unscored company is neither best nor worst.
        var scored = dataset.Companies
            .Select(c => (Company: c, Score: ScoreCompany(c, dataset)))
            .ToList();

        IEnumerable<(ICompany Company, ICompositeScore Score)> ordered = direction switch
        {
            LeaderboardDirection.TopPerformers => scored
                .OrderByDescending(x => x.Score.Value.HasValue)
                .ThenByDescending(x => x.Score.Value ?? 0)
                .ThenBy(x => x.Company.Name, StringComparer.Ordinal),
            LeaderboardDirection.WorstOffenders => scored
                .OrderByDescending(x => x.Score.Value.HasValue)
                .ThenBy(x => x.Score.Value ?? 0)
                .ThenBy(x => x.Company.Name, StringComparer.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

        return ordered
            .Select((x, i) => new ScoredCompany(i + 1, x.Company, x.Score))
            .ToList();
    }

    /// <summary>
    /// Emissions pillar: scored on the most recent disclosed intensity (tCO2e per £m
    /// revenue). Absolute tonnage is not comparable across companies of different size,
    /// so a company that discloses tonnage but no derivable intensity is left unknown.
    /// </summary>
    private static PillarScore ScoreEmissions(IReadOnlyList<IEmissionsRecord> records)
    {
        var latest = records
            .Where(r => r.IntensityTco2ePerGbpMillion is > 0)
            .OrderByDescending(r => r.Year)
            .FirstOrDefault();

        if (latest is null)
        {
            return new PillarScore(Pillar.Emissions, null, ScoringConstants.EmissionsWeight,
                "No emissions intensity disclosed — pillar unknown, excluded from composite.");
        }

        var intensity = latest.IntensityTco2ePerGbpMillion!.Value;
        var score = 100.0 * Math.Exp(-intensity / ScoringConstants.EmissionsIntensityScale);
        return new PillarScore(Pillar.Emissions, Round(score), ScoringConstants.EmissionsWeight,
            $"Scored on {latest.Year} intensity of {intensity:0.##} tCO2e/£m.");
    }

    /// <summary>
    /// Violations pillar: aggregates every recorded enforcement event into a single "harm"
    /// figure (fines + incident counts + discharge hours, each weighted), then decays it to
    /// a 0–100 score. No enforcement records means a clean 100 — but only when we actually
    /// hold data for the company (<paramref name="companyHasAnyData"/>): a company we hold
    /// nothing on is a coverage gap, left unknown rather than crowned by its own absence.
    /// </summary>
    private static PillarScore ScoreViolations(IReadOnlyList<IViolationRecord> records, bool companyHasAnyData)
    {
        if (records.Count == 0)
        {
            return companyHasAnyData
                ? new PillarScore(Pillar.Violations, 100.0, ScoringConstants.ViolationsWeight,
                    "No recorded enforcement action in the open-gov registers.")
                : new PillarScore(Pillar.Violations, null, ScoringConstants.ViolationsWeight,
                    "No data held for this company — pillar unknown, excluded from composite.");
        }

        var totalFineMillions = (double)records.Sum(r => r.FineGbp) / 1_000_000.0;
        var totalIncidents = records.Sum(r => r.IncidentCount);
        var totalDischarge1000s = records.Sum(r => r.DischargeHours ?? 0) / 1_000.0;

        var harm = totalFineMillions * ScoringConstants.FineHarmPerMillionGbp
                   + totalIncidents * ScoringConstants.IncidentHarmPerCount
                   + totalDischarge1000s * ScoringConstants.DischargeHarmPer1000Hours;

        var score = 100.0 * Math.Exp(-harm / ScoringConstants.ViolationHarmScale);
        return new PillarScore(Pillar.Violations, Round(score), ScoringConstants.ViolationsWeight,
            $"{records.Count} enforcement record(s): £{records.Sum(r => r.FineGbp):N0} in fines, "
            + $"{totalIncidents} incident(s).");
    }

    /// <summary>
    /// Weight-renormalised average over the pillars that actually have a value. Null pillars
    /// drop out entirely; if none remain the composite is null (total opacity, never a
    /// neutral fabricated number).
    /// </summary>
    private static double? Composite(IReadOnlyList<PillarScore> pillars)
    {
        var contributing = pillars.Where(p => p.Value.HasValue).ToList();
        if (contributing.Count == 0)
            return null;

        var totalWeight = contributing.Sum(p => p.Weight);
        if (totalWeight <= 0)
            return null;

        var weighted = contributing.Sum(p => p.Value!.Value * p.Weight);
        return Round(weighted / totalWeight);
    }

    private static double Round(double value) =>
        Math.Round(value, ScoringConstants.ScoreDecimals, MidpointRounding.AwayFromZero);
}
