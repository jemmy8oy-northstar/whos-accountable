namespace Balenthiran.WhosAccountable.DomainModels.Models;

/// <summary>
/// Provenance stamped on every input record: which published dataset it came from,
/// which versioned snapshot, when it was retrieved, and the exact public URL to click
/// through to. DESIGN.md ("data-as-code"): datasets live versioned in the repo with
/// retrieval date + source URL, so every score change is a reviewable, attributable diff.
/// </summary>
/// <param name="Kind">The published source this record was extracted from.</param>
/// <param name="Version">Dataset snapshot version (bumped when the in-repo dataset is refreshed).</param>
/// <param name="RetrievedOn">The date the snapshot was pulled from the source.</param>
/// <param name="Url">Deep link to the specific public record.</param>
public sealed record SourceRef(
    SourceKind Kind,
    int Version,
    DateOnly RetrievedOn,
    string Url);
