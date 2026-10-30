using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Abstractions.DomainModels;
using Balenthiran.WhosAccountable.Services.Datasets;

namespace Balenthiran.WhosAccountable.Tests;

/// <summary>
/// Pins the shape of the real cohort in <c>data/cohort-v1.json</c> (BEH-DATA-3, whos-accountable#13).
/// It checks the provenance rules every real record must keep, not the figures themselves. Those
/// are copied from a published table, and data/README.md records how they were reconciled against
/// the row-level data. A test that re-typed them would only check the copy against itself.
/// </summary>
public sealed class RealCohortTests
{
    private const string EdmDataset =
        "https://www.data.gov.uk/dataset/19f6064d-7356-466f-844e-d20ea10ae9fd/event-duration-monitoring-storm-overflows-annual-returns";

    private static ICohortDataset Load() =>
        new DatasetLoader().Load(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "cohort-v1.json")));

    [Fact]
    public void The_real_cohort_loads_through_the_same_loader_as_the_engine()
    {
        var dataset = Load();

        Assert.Equal(10, dataset.Companies.Count);
        Assert.All(dataset.Companies, c => Assert.Equal(Sector.WaterUtilities, c.Sector));
    }

    [Fact]
    public void Every_company_is_identified_by_a_companies_house_number()
    {
        var numbers = Load().Companies.Select(c => c.CompaniesHouseNumber).ToList();

        Assert.All(numbers, n => Assert.Matches("^[0-9]{8}$", n));
        Assert.Equal(numbers.Count, numbers.Distinct().Count());
    }

    [Fact]
    public void Every_record_links_to_its_published_source_with_a_retrieval_date()
    {
        var violations = Load().Violations;

        Assert.All(violations, v =>
        {
            Assert.Equal(SourceKind.EventDurationMonitoring, v.Source.Kind);
            Assert.Equal(EdmDataset, v.Source.Url);
            Assert.True(v.Source.RetrievedOn >= new DateOnly(2026, 1, 1));
        });
    }

    [Fact]
    public void Each_company_holds_exactly_one_year_so_the_engine_does_not_double_count()
    {
        // ScoreEngine sums every violation record a company has, across years. Until it scores
        // per year, a second year of EDM data would double a company's discharge hours.
        var violations = Load().Violations;

        Assert.All(violations.GroupBy(v => v.CompanyId), g => Assert.Single(g));
        Assert.All(violations, v => Assert.Equal(2025, v.Year));
    }

    [Fact]
    public void An_edm_spill_is_never_recorded_as_a_fine()
    {
        // A spill is a recorded discharge, not a penalty. Fines come from EA/Ofwat, a later source.
        Assert.All(Load().Violations, v =>
        {
            Assert.Equal(0m, v.FineGbp);
            Assert.Contains("not of breaches", v.Description);
        });
    }
}
