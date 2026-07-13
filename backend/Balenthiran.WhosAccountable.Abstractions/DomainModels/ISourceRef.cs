namespace Balenthiran.WhosAccountable.Abstractions.DomainModels;

/// <summary>
/// Provenance stamped on every input record: which published dataset it came from, which
/// versioned snapshot, when it was retrieved, and the exact public URL to click through to.
/// </summary>
public interface ISourceRef
{
    SourceKind Kind { get; }
    int Version { get; }
    DateOnly RetrievedOn { get; }
    string Url { get; }
}
