namespace ATS.Application.Vagas.DTOs;

using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;


public sealed record VagaDto(
    Guid Id,
    string Titulo,
    string Descricao,
    string Requisitos,
    decimal Salario,
    string Moeda,
    StatusVaga Status,
    string StatusDescricao,
    DateTime DataAbertura,
    DateTime? DataEncerramento
)
{
    public static VagaDto FromDomain(Vaga vaga) => new(
        Id: vaga.Id,
        Titulo: vaga.Titulo,
        Descricao: vaga.Descricao,
        Requisitos: vaga.Requisitos,
        Salario: vaga.Salario.Valor,
        Moeda: vaga.Salario.Moeda,
        Status: vaga.Status,
        StatusDescricao: DescricaoStatus(vaga.Status),
        DataAbertura: vaga.DataAbertura,
        DataEncerramento: vaga.DataEncerramento
    );

    private static string DescricaoStatus(StatusVaga status) => status switch
    {
        StatusVaga.Rascunho => "Rascunho",
        StatusVaga.Aberta => "Aberta",
        StatusVaga.Fechada => "Fechada",
        _ => status.ToString()
    };
}
