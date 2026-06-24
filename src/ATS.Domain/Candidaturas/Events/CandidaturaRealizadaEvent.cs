using ATS.Domain.Shared;

namespace ATS.Domain.Candidaturas.Enums;

public sealed class CandidaturaRealizadaEvent : IDomainEvent
{
    public Guid CandidaturaId { get; }
    public Guid CandidatoId { get; }
    public Guid VagaId { get; }
    public DateTime OcorridoEm { get; }

    public CandidaturaRealizadaEvent(Guid candidaturaId, Guid candidatoId, Guid vagaId)
    {
        CandidaturaId = candidaturaId;
        CandidatoId = candidatoId;
        VagaId = vagaId;
        OcorridoEm = DateTime.UtcNow;
    }
}
