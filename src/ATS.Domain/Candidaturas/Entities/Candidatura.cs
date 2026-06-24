namespace ATS.Domain.Candidaturas.Entities;

using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Shared;

public sealed class Candidatura : AggregateRoot
{
    public Guid CandidatoId { get; private set; }
    public Guid VagaId { get; private set; }
    public StatusCandidatura Status { get; private set; }
    public DateTime DataCandidatura { get; private set; }
    public string? Observacoes { get; private set; }

    private Candidatura() { }

    public static Candidatura Criar(Guid candidatoId, Guid vagaId)
    {
        if (candidatoId == Guid.Empty)
        {
            throw new DomainException("CandidatoId inválido.");
        }

        if (vagaId == Guid.Empty)
        {
            throw new DomainException("VagaId inválido.");
        }

        var candidatura = new Candidatura
        {
            CandidatoId = candidatoId,
            VagaId = vagaId,
            Status = StatusCandidatura.EmAnalise,
            DataCandidatura = DateTime.UtcNow
        };

        candidatura.AddDomainEvent(new CandidaturaRealizadaEvent(
            candidatura.Id, candidatoId, vagaId));

        return candidatura;
    }

    public void Aprovar(string? observacoes = null)
    {
        if (Status != StatusCandidatura.EmAnalise)
        {
            throw new DomainException("Somente candidaturas 'Em Análise' podem ser aprovadas.");
        }

        Status = StatusCandidatura.Aprovado;
        Observacoes = observacoes;
    }

    public void Reprovar(string? observacoes = null)
    {
        if (Status != StatusCandidatura.EmAnalise)
        {
            throw new DomainException("Somente candidaturas 'Em Análise' podem ser reprovadas.");
        }

        Status = StatusCandidatura.Reprovado;
        Observacoes = observacoes;
    }

    public void Cancelar()
    {
        if (Status == StatusCandidatura.Cancelado)
        {
            throw new DomainException("Candidatura já foi cancelada.");
        }

        Status = StatusCandidatura.Cancelado;
    }
}
