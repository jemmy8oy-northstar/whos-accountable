using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Abstractions.DomainModels;
using Balenthiran.WhosAccountable.DomainModels.Models;

namespace Balenthiran.WhosAccountable.Services.Datasets;

/// <summary>
/// Parses the in-repo "data-as-code" JSON into a <see cref="ICohortDataset"/>. It deserialises
/// into the concrete <see cref="CohortDataset"/> record and hands back the interface, so the JSON
/// shape stays an implementation detail behind the abstraction. Enums are read by name (so the
/// JSON is legible in diffs) and property matching is case-insensitive. The engine never touches
/// JSON directly — everything flows through here.
/// </summary>
public sealed partial class DatasetLoader : IDatasetLoader
{
    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex CountryCodeRegex();

    private static readonly Regex CountryCode = CountryCodeRegex();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public ICohortDataset Load(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Dataset JSON is empty.");

        var dataset = JsonSerializer.Deserialize<CohortDataset>(json, JsonOptions)
                      ?? throw new InvalidOperationException("Dataset JSON deserialised to null.");

        var ids = new HashSet<string>();
        foreach (var company in dataset.Companies)
        {
            if (string.IsNullOrWhiteSpace(company.Id))
                throw new InvalidOperationException("A company is missing its 'id'.");
            if (!ids.Add(company.Id))
                throw new InvalidOperationException($"Duplicate company id '{company.Id}'.");
            // A missing country would drop the company from every country filter, so it is
            // refused like a missing id rather than defaulted.
            if (company.Country is null || !CountryCode.IsMatch(company.Country))
                throw new InvalidOperationException(
                    $"Company '{company.Id}' needs a 'country': an upper-case ISO 3166-1 alpha-2 code, such as GB or US.");
        }

        // Every record must attach to a known company — a stray companyId is a data bug
        // that would silently vanish from the leaderboard, so fail loudly instead.
        foreach (var companyId in dataset.Emissions.Select(e => e.CompanyId)
                     .Concat(dataset.Violations.Select(v => v.CompanyId)))
        {
            if (!ids.Contains(companyId))
                throw new InvalidOperationException($"Record references unknown company id '{companyId}'.");
        }

        return dataset;
    }
}
