
using ATS.Domain.Shared;

namespace ATS.Domain.Vagas.Events;

public sealed class VagaPublicadaEvent : IDomainEvent
{
    public Guid VagaId { get; }
    public string Titulo { get; }
    public DateTime OcorridoEm { get; }

    public VagaPublicadaEvent(Guid vagaId, string titulo)
    {
        VagaId = vagaId;
        Titulo = titulo;
        OcorridoEm = DateTime.UtcNow;
    }
}
