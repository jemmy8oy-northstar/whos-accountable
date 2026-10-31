using Balenthiran.WhosAccountable.Abstractions;

namespace Balenthiran.WhosAccountable.WebApi.Routes;

public static class MethodologyRoutes
{
    public static RouteGroupBuilder MapMethodologyRoutes(this RouteGroupBuilder parentGroup)
    {
        parentGroup.MapGet("/methodology", (ScoreboardStore store) =>
        {
            // Sources are whatever the loaded records cite; the dataset itself carries no separate version.
            var sources = store.Dataset.Emissions.Select(e => e.Source)
                .Concat(store.Dataset.Violations.Select(v => v.Source))
                .Select(SourceDto.From)
                .Distinct()
                .OrderBy(s => s.Kind).ThenBy(s => s.Version).ThenBy(s => s.Url, StringComparer.Ordinal)
                .ToList();

            return Results.Ok(new MethodologyDto(
                new Dictionary<string, double>
                {
                    [nameof(Pillar.Emissions)] = ScoringConstants.EmissionsWeight,
                    [nameof(Pillar.Violations)] = ScoringConstants.ViolationsWeight,
                },
                new Dictionary<string, double>
                {
                    [nameof(ScoringConstants.EmissionsIntensityScale)] = ScoringConstants.EmissionsIntensityScale,
                    [nameof(ScoringConstants.FineHarmPerMillionGbp)] = ScoringConstants.FineHarmPerMillionGbp,
                    [nameof(ScoringConstants.IncidentHarmPerCount)] = ScoringConstants.IncidentHarmPerCount,
                    [nameof(ScoringConstants.DischargeHarmPer1000Hours)] = ScoringConstants.DischargeHarmPer1000Hours,
                    [nameof(ScoringConstants.ViolationHarmScale)] = ScoringConstants.ViolationHarmScale,
                },
                ScoringConstants.ScoreDecimals,
                store.Dataset.Companies.Count,
                sources));
        })
        .WithName("GetMethodology");

        return parentGroup;
    }
}
