namespace ATS.Application.Vagas.Commands.UpdateVaga;

public record UpdateVagaCommand(
    Guid Id,
    string Titulo,
    string Descricao,
    string? Requisitos = null,
    decimal Salario = 0m
);
