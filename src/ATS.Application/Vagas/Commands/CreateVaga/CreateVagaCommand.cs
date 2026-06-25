namespace ATS.Application.Vagas.Commands.CreateVaga;

public record CreateVagaCommand(
    string Titulo,
    string Descricao,
    string? Requisitos = null,
    decimal Salario = 0m,
    string Moeda = "BRL"
);
