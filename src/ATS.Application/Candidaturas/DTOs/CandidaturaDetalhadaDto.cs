namespace ATS.Application.Candidaturas.DTOs;

using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;

public sealed record CandidaturaDetalhadaDto(
    Guid Id,
    Guid CandidatoId,
    string NomeCandidato,
    string EmailCandidato,
    string TelefoneCandidato,
    bool PossuiCurriculo,
    string? NomeCurriculo,
    Guid VagaId,
    string TituloVaga,
    StatusCandidatura Status,
    string StatusDescricao,
    DateTime DataCandidatura,
    string? Observacoes
)
{
    public static CandidaturaDetalhadaDto FromDomain(
        Candidatura candidatura,
        Candidato candidato,
        string tituloVaga) => new(
            Id: candidatura.Id,

            CandidatoId: candidato.Id,
            NomeCandidato: candidato.Nome,
            EmailCandidato: candidato.Email.Value,
            TelefoneCandidato: candidato.Telefone.Value,
            PossuiCurriculo: candidato.Curriculo is not null,
            NomeCurriculo: candidato.Curriculo?.NomeArquivo,

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
