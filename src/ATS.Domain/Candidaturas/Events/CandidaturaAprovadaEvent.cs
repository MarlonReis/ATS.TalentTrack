namespace ATS.Domain.Candidaturas.Events;

using ATS.Domain.Shared;

public sealed class CandidaturaAprovadaEvent : IDomainEvent
{
    public Guid CandidaturaId { get; }
    public Guid CandidatoId { get; }
    public Guid VagaId { get; }
    public string? Observacoes { get; }
    public DateTime OcorridoEm { get; }

    public CandidaturaAprovadaEvent(Guid candidaturaId, Guid candidatoId, Guid vagaId, string? observacoes)
    {
        CandidaturaId = candidaturaId;
        CandidatoId = candidatoId;
        VagaId = vagaId;
        Observacoes = observacoes;
        OcorridoEm = DateTime.UtcNow;
    }
}
