namespace ATS.Domain.Candidaturas.Events;

using ATS.Domain.Shared;

public sealed class CandidaturaCanceladaEvent : IDomainEvent
{
    public Guid CandidaturaId { get; }
    public Guid CandidatoId { get; }
    public Guid VagaId { get; }
    public DateTime OcorridoEm { get; }

    public CandidaturaCanceladaEvent(Guid candidaturaId, Guid candidatoId, Guid vagaId)
    {
        CandidaturaId = candidaturaId;
        CandidatoId = candidatoId;
        VagaId = vagaId;
        OcorridoEm = DateTime.UtcNow;
    }
}
