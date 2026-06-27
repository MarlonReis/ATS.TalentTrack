namespace ATS.Domain.Candidaturas.Events;

using ATS.Domain.Shared;

public sealed class CandidaturaReprovadaEvent : IDomainEvent
{
    public Guid CandidaturaId { get; }
    public Guid CandidatoId { get; }
    public Guid VagaId { get; }
    public string? Observacoes { get; }
    public DateTime OcorridoEm { get; }

    public CandidaturaReprovadaEvent(Guid candidaturaId, Guid candidatoId, Guid vagaId, string? observacoes)
    {
        CandidaturaId = candidaturaId;
        CandidatoId = candidatoId;
        VagaId = vagaId;
        Observacoes = observacoes;
        OcorridoEm = DateTime.UtcNow;
    }
}
