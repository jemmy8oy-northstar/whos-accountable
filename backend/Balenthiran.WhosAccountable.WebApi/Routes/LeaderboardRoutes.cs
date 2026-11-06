using Balenthiran.WhosAccountable.Abstractions;

namespace Balenthiran.WhosAccountable.WebApi.Routes;

public static class LeaderboardRoutes
{
    public static RouteGroupBuilder MapLeaderboardRoutes(this RouteGroupBuilder parentGroup)
    {
        parentGroup.MapGet("/leaderboard", (ScoreboardStore store, string? direction, string? sector, string? country) =>
        {
            LeaderboardDirection dir;
            switch (direction?.ToLowerInvariant())
            {
                case null or "" or "worst": dir = LeaderboardDirection.WorstOffenders; break;
                case "best": dir = LeaderboardDirection.TopPerformers; break;
                default: return Results.BadRequest(new { error = "direction must be 'worst' or 'best'." });
            }

            Sector? filter = null;
            if (!string.IsNullOrEmpty(sector))
            {
                if (!Enum.TryParse<Sector>(sector, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
                    return Results.BadRequest(new { error = $"unknown sector '{sector}'." });
                filter = parsed;
            }

            // A country is any two-letter code; one with no companies is an empty board, not an error.
            if (!string.IsNullOrEmpty(country) && (country.Length != 2 || !country.All(char.IsAsciiLetter)))
                return Results.BadRequest(new { error = "country must be a two-letter code, such as GB or US." });

            // Rank is 1-based within what is returned, so a filtered view starts at 1.
            var entries = store.Board(dir)
                .Where(s => filter is null || s.Company.Sector == filter)
                .Where(s => string.IsNullOrEmpty(country) || string.Equals(s.Company.Country, country, StringComparison.OrdinalIgnoreCase))
                .Select((s, i) => LeaderboardEntryDto.From(i + 1, s))
                .ToList();
            return Results.Ok(entries);
        })
        .WithName("GetLeaderboard");

        return parentGroup;
    }
}
