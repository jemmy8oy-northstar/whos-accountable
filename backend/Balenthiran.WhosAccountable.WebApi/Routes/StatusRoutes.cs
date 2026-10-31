namespace Balenthiran.WhosAccountable.WebApi.Routes;

public static class StatusRoutes
{
    public static RouteGroupBuilder MapStatusRoutes(this RouteGroupBuilder parentGroup)
    {
        // Resolving the store forces the dataset to load, so "ok" means the cohort is actually in memory.
        parentGroup.MapGet("/status", (ScoreboardStore store) => Results.Ok(new
        {
            status = "ok",
            companies = store.Dataset.Companies.Count,
            timestamp = DateTimeOffset.UtcNow,
        }))
        .WithName("GetStatus");

        return parentGroup;
    }
}
