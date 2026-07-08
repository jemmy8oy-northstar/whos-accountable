using Balenthiran.WhosAccountable.DomainModels.Models;

namespace Balenthiran.WhosAccountable.Tests;

/// <summary>
/// Test-only builders for domain records and access to the on-disk JSON fixtures. All data
/// here is synthetic (fictional companies) — see <c>Fixtures/synthetic-cohort.json</c>.
/// </summary>
internal static class Fixtures
{
    /// <summary>Path to a JSON fixture copied next to the test assembly at build time.</summary>
    public static string Path(string fileName) =>
        System.IO.Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);

    public static string ReadText(string fileName) => File.ReadAllText(Path(fileName));

    /// <summary>A throwaway <see cref="SourceRef"/> — provenance is not what these tests exercise.</summary>
    public static SourceRef Src(SourceKind kind = SourceKind.UkEtsRegistry) =>
        new(kind, 1, new DateOnly(2026, 1, 15), "https://example.invalid/synthetic");

    public static Company Company(string id, string name = "Test Co", Sector sector = Sector.Other) =>
        new(id, name, sector, ["synthetic"]);

    public static EmissionsRecord Emissions(string companyId, int year, double? intensity, double absolute = 1000) =>
        new(companyId, year, absolute, intensity, Src());

    public static ViolationRecord Violation(
        string companyId, int year, decimal fineGbp, int incidents = 1, double? dischargeHours = null) =>
        new(companyId, year, fineGbp, incidents, dischargeHours, "Synthetic enforcement event.",
            Src(SourceKind.EnvironmentAgency));

    public static CohortDataset Dataset(
        IEnumerable<Company> companies,
        IEnumerable<EmissionsRecord>? emissions = null,
        IEnumerable<ViolationRecord>? violations = null) =>
        new([.. companies], [.. emissions ?? []], [.. violations ?? []]);
}
