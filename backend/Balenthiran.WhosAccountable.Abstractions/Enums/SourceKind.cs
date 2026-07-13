namespace Balenthiran.WhosAccountable.Abstractions;

/// <summary>
/// A published dataset a record was extracted from — carried on every record so that
/// each number in the product links back to a public source (the entire credibility
/// story of the app, DESIGN.md). Open-government sources only in v1.
/// </summary>
public enum SourceKind
{
    /// <summary>UK Emissions Trading Scheme registry — verified absolute emissions. Open data.</summary>
    UkEtsRegistry = 0,

    /// <summary>Environment Agency enforcement / prosecution records. Open Government Licence.</summary>
    EnvironmentAgency,

    /// <summary>Ofwat penalties against water companies. Open Government Licence.</summary>
    Ofwat,

    /// <summary>Event Duration Monitoring — storm-overflow / sewage discharge data. Open Government Licence.</summary>
    EventDurationMonitoring,
}
