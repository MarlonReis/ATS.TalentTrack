namespace ATS.Application.Observability;

using System.Diagnostics.Metrics;

public static class AtsMetrics
{
    public const string MeterName = "ATS.Application";

    private static readonly Meter _meter = new(MeterName, "1.0.0");

    public static readonly Counter<long> CandidatosCriados =
        _meter.CreateCounter<long>(
            "ats.candidatos.criados",
            unit: "{candidatos}",
            description: "Número de candidatos criados.");

    public static readonly Counter<long> VagasCriadas =
        _meter.CreateCounter<long>(
            "ats.vagas.criadas",
            unit: "{vagas}",
            description: "Número de vagas criadas.");

    public static readonly Counter<long> CandidaturasCriadas =
        _meter.CreateCounter<long>(
            "ats.candidaturas.criadas",
            unit: "{candidaturas}",
            description: "Número de candidaturas realizadas.");
}
