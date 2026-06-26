namespace ATS.API.Observability;

public sealed class ObservabilitySettings
{
    public string ServiceName { get; set; } = "ats-api";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string? OtlpEndpoint { get; set; }
    public bool EnableConsoleExporter { get; set; }
    public bool EnablePrometheusEndpoint { get; set; } = true;
}
