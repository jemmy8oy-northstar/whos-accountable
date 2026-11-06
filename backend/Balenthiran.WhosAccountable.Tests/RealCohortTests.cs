using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Abstractions.DomainModels;
using Balenthiran.WhosAccountable.Services.Datasets;
using Balenthiran.WhosAccountable.Services.Scoring;

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

    private const string UkEtsComplianceReport =
        "https://reports.view-emissions-trading-registry.service.gov.uk/ets-reports/section5/UK_ETS_Compliance_Report_Emissions_%26_Surrenders_2026.xlsx";

    private static ICohortDataset Load() =>
        new DatasetLoader().Load(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "cohort-v1.json")));

    [Fact]
    public void The_real_cohort_loads_through_the_same_loader_as_the_engine()
    {
        var dataset = Load();

        Assert.Equal(42, dataset.Companies.Count);
        Assert.Equal(10, dataset.Companies.Count(c => c.Sector == Sector.WaterUtilities));
        Assert.Equal(16, dataset.Companies.Count(c => c.Sector == Sector.EnergyUtilities));
        Assert.Equal(16, dataset.Companies.Count(c => c.Sector == Sector.OilAndGas));
    }

    [Fact]
    public void Every_oil_and_gas_company_is_tagged_and_holds_only_emissions()
    {
        // Slice 3 has no violations source yet: OPRED and EA enforcement for these sites is a later
        // slice, so an oil and gas company must not appear clean on violations by having a record.
        var dataset = Load();
        var oilAndGas = dataset.Companies.Where(c => c.Sector == Sector.OilAndGas).ToList();

        Assert.All(oilAndGas, c => Assert.Equal(new[] { "oil-and-gas" }, c.CohortTags));
        Assert.All(oilAndGas, c => Assert.Single(dataset.Emissions, e => e.CompanyId == c.Id));
        Assert.DoesNotContain(dataset.Violations, v => oilAndGas.Any(c => c.Id == v.CompanyId));
    }

    [Fact]
    public void Every_company_is_identified_by_a_companies_house_number()
    {
        // Eight digits for England and Wales; SC for Scotland, OC for an LLP, FC for an overseas
        // company registered here because it has a UK establishment.
        var numbers = Load().Companies.Select(c => c.CompaniesHouseNumber).ToList();

        Assert.All(numbers, n => Assert.Matches("^([0-9]{8}|(SC|OC|FC)[0-9]{6})$", n));
        Assert.Equal(numbers.Count, numbers.Distinct().Count());
    }

    [Fact]
    public void Every_record_links_to_its_published_source_with_a_retrieval_date()
    {
        var dataset = Load();

        Assert.All(dataset.Violations, v =>
        {
            Assert.Equal(SourceKind.EventDurationMonitoring, v.Source.Kind);
            Assert.Equal(EdmDataset, v.Source.Url);
            Assert.True(v.Source.RetrievedOn >= new DateOnly(2026, 1, 1));
        });
        Assert.All(dataset.Emissions, e =>
        {
            Assert.Equal(SourceKind.UkEtsRegistry, e.Source.Kind);
            Assert.Equal(UkEtsComplianceReport, e.Source.Url);
            Assert.True(e.Source.RetrievedOn >= new DateOnly(2026, 1, 1));
        });
    }

    [Fact]
    public void Each_emitter_holds_one_year_of_verified_emissions_and_no_intensity()
    {
        // Revenue is not a like-for-like denominator for a single power station: a tolled plant's
        // revenue is a fee for being available, so its intensity measures the contract, not the
        // carbon. Intensity stays unknown until the scale is calibrated (data/README.md, slice 2).
        // Slice 3's oil and gas holders are on the same terms (data/README.md, slice 3).
        var emissions = Load().Emissions;

        Assert.All(emissions.GroupBy(e => e.CompanyId), g => Assert.Single(g));
        Assert.All(emissions, e =>
        {
            Assert.Equal(2025, e.Year);
            Assert.True(e.AbsoluteTco2e > 0);
            Assert.Null(e.IntensityTco2ePerGbpMillion);
        });
    }

    [Fact]
    public void A_company_outside_the_held_enforcement_registers_gets_no_composite()
    {
        // The real cohort holds EDM returns for water companies and no enforcement register for
        // anyone else. With intensity unknown too, a power generator has no pillar with a value,
        // so it is listed unscored, never a clean 100 (BEH-SCORE-4, BEH-SCORE-8).
        var dataset = Load();
        var engine = new ScoreEngine();

        Assert.Equal(new[] { Sector.WaterUtilities }, dataset.ViolationsCoverage);
        Assert.All(dataset.Companies.Where(c => c.Sector != Sector.WaterUtilities),
            c => Assert.Null(engine.ScoreCompany(c, dataset).Value));
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
