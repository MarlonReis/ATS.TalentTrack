namespace ATS.Domain.Candidatos.Events;

using ATS.Domain.Shared;


public sealed class CandidatoCriadoEvent : IDomainEvent
{
    public Guid CandidatoId { get; }

    public string Nome { get; }

    public DateTime OcorridoEm { get; }

    public CandidatoCriadoEvent(Guid candidatoId, string nome)
    {
        CandidatoId = candidatoId;
        Nome = nome;
        OcorridoEm = DateTime.UtcNow;
    }
}
