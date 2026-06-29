namespace ATS.Application.Vagas.Queries.ListVagas;

using ATS.Domain.Vagas.Enums;

public record ListVagasComCursorQuery(
    string? Cursor = null,
    int Limite = 20,
    StatusVaga? Status = null
);
