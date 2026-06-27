namespace ATS.Domain.Vagas.Events;

using ATS.Domain.Shared;

public sealed class VagaFechadaEvent : IDomainEvent
{
    public Guid VagaId { get; }
    public string Titulo { get; }
    public DateTime OcorridoEm { get; }

    public VagaFechadaEvent(Guid vagaId, string titulo)
    {
        VagaId = vagaId;
        Titulo = titulo;
        OcorridoEm = DateTime.UtcNow;
    }
}
