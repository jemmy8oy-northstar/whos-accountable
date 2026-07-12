using System.Text.Json;
using System.Text.Json.Serialization;
using Balenthiran.WhosAccountable.Abstractions;

namespace Balenthiran.WhosAccountable.Services.Datasets;

/// <summary>
/// Parses the in-repo "data-as-code" JSON into a <see cref="CohortDataset"/>. Enums are
/// read by name (so the JSON is legible in diffs) and property matching is
/// case-insensitive. The engine never touches JSON directly — everything flows through here.
/// </summary>
public sealed class DatasetLoader : IDatasetLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public CohortDataset Load(string json)
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
