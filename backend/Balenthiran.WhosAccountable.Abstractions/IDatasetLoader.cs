namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>
/// Deserialises the in-repo "data-as-code" JSON into a <see cref="CohortDataset"/>.
/// Kept behind an interface so the WebApi read model (a later PR) and the tests load
/// datasets the same way.
/// </summary>
public interface IDatasetLoader
{
    /// <summary>Parses a single JSON document into a dataset.</summary>
    CohortDataset Load(string json);
}
