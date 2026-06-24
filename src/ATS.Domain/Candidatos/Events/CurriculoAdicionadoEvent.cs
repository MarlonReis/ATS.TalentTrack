namespace ATS.Domain.Candidatos.Events;

using ATS.Domain.Shared;

public sealed class CurriculoAdicionadoEvent : IDomainEvent
{
    public Guid CandidatoId { get; }
    public string NomeArquivo { get; }
    public DateTime OcorridoEm { get; }

    public CurriculoAdicionadoEvent(Guid candidatoId, string nomeArquivo)
    {
        CandidatoId = candidatoId;
        NomeArquivo = nomeArquivo;
        OcorridoEm = DateTime.UtcNow;
    }
}
