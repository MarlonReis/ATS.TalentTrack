namespace ATS.Application.Candidaturas.DTOs;

using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;


public sealed record CandidaturaDto(
    Guid Id,
    Guid CandidatoId,
    string NomeCandidato,
    Guid VagaId,
    string TituloVaga,
    StatusCandidatura Status,
    string StatusDescricao,
    DateTime DataCandidatura,
    string? Observacoes
)
{

    public static CandidaturaDto FromDomain(
        Candidatura candidatura,
        string nomeCandidato,
        string tituloVaga) => new(
            Id: candidatura.Id,
            CandidatoId: candidatura.CandidatoId,
            NomeCandidato: nomeCandidato,
            VagaId: candidatura.VagaId,
            TituloVaga: tituloVaga,
            Status: candidatura.Status,
            StatusDescricao: DescricaoStatus(candidatura.Status),
            DataCandidatura: candidatura.DataCandidatura,
            Observacoes: candidatura.Observacoes
        );

    private static string DescricaoStatus(StatusCandidatura status) => status switch
    {
        StatusCandidatura.EmAnalise => "Em Análise",
        StatusCandidatura.Aprovado => "Aprovado",
        StatusCandidatura.Reprovado => "Reprovado",
        StatusCandidatura.Cancelado => "Cancelado",
        _ => status.ToString()
    };
}
