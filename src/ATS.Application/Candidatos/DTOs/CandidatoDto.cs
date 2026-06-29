namespace ATS.Application.Candidatos.DTOs;

using ATS.Domain.Candidatos.Entities;

public record CandidatoDto(
    Guid Id,
    string Nome,
    string Email,
    string Telefone,
    bool PossuiCurriculo,
    string? NomeCurriculo,
    CurriculoDto? Curriculo,
    DateTime DataCadastro
)
{
    public static CandidatoDto FromDomain(Candidato candidato) => new(
        candidato.Id,
        candidato.Nome,
        candidato.Email.Value,
        candidato.Telefone.Value,
        candidato.Curriculo is not null,
        candidato.Curriculo?.NomeArquivo,
        candidato.Curriculo is not null ? CurriculoDto.FromDomain(candidato.Curriculo) : null,
        candidato.DataCadastro
    );
}
