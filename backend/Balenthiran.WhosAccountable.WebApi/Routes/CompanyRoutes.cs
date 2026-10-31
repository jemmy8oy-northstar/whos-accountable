namespace Balenthiran.WhosAccountable.WebApi.Routes;

public static class CompanyRoutes
{
    public static RouteGroupBuilder MapCompanyRoutes(this RouteGroupBuilder parentGroup)
    {
        parentGroup.MapGet("/companies/{id}", (string id, ScoreboardStore store) =>
        {
            var scored = store.Find(id);
            if (scored is null)
                return Results.NotFound(new { error = $"no company '{id}'." });

            var c = scored.Company;
            var s = scored.Score;
            return Results.Ok(new CompanyDto(
                c.Id, c.Name, c.Sector, c.CohortTags, c.Lei, c.CompaniesHouseNumber,
                s.Value, s.CoveredPillars, s.TotalPillars, s.Pillars.Select(PillarDto.From).ToList(),
                store.Dataset.Emissions.Where(e => e.CompanyId == c.Id)
                    .Select(e => new EmissionsEvidenceDto(e.Year, e.AbsoluteTco2e, e.IntensityTco2ePerGbpMillion, SourceDto.From(e.Source)))
                    .ToList(),
                store.Dataset.Violations.Where(v => v.CompanyId == c.Id)
                    .Select(v => new ViolationEvidenceDto(v.Year, v.FineGbp, v.IncidentCount, v.DischargeHours, v.Description, SourceDto.From(v.Source)))
                    .ToList()));
        })
        .WithName("GetCompany");

        return parentGroup;
    }
}
