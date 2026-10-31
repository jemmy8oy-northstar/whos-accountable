using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Abstractions.DomainModels;

namespace Balenthiran.WhosAccountable.WebApi;

// Response shapes. The domain interfaces nest company inside score inside rank, which a client has
// to unpick; these flatten them. Enums travel as names (see Program.cs).

public sealed record PillarDto(Pillar Pillar, double? Value, double Weight, bool Contributed, string? Note)
{
    public static PillarDto From(IPillarScore p) => new(p.Pillar, p.Value, p.Weight, p.Contributed, p.Note);
}

public sealed record LeaderboardEntryDto(
    int Rank, string CompanyId, string Name, Sector Sector,
    double? Composite, int CoveredPillars, int TotalPillars, IReadOnlyList<PillarDto> Pillars)
{
    public static LeaderboardEntryDto From(int rank, IScoredCompany s) => new(
        rank, s.Company.Id, s.Company.Name, s.Company.Sector, s.Score.Value,
        s.Score.CoveredPillars, s.Score.TotalPillars, s.Score.Pillars.Select(PillarDto.From).ToList());
}

public sealed record SourceDto(SourceKind Kind, int Version, DateOnly RetrievedOn, string Url)
{
    public static SourceDto From(ISourceRef s) => new(s.Kind, s.Version, s.RetrievedOn, s.Url);
}

public sealed record EmissionsEvidenceDto(int Year, double AbsoluteTco2e, double? IntensityTco2ePerGbpMillion, SourceDto Source);

public sealed record ViolationEvidenceDto(
    int Year, decimal FineGbp, int IncidentCount, double? DischargeHours, string Description, SourceDto Source);

public sealed record CompanyDto(
    string Id, string Name, Sector Sector, IReadOnlyList<string> CohortTags, string? Lei, string? CompaniesHouseNumber,
    double? Composite, int CoveredPillars, int TotalPillars, IReadOnlyList<PillarDto> Pillars,
    IReadOnlyList<EmissionsEvidenceDto> Emissions, IReadOnlyList<ViolationEvidenceDto> Violations);

public sealed record MethodologyDto(
    IReadOnlyDictionary<string, double> Weights,
    IReadOnlyDictionary<string, double> Constants,
    int ScoreDecimals,
    int CompanyCount,
    IReadOnlyList<SourceDto> Sources);
