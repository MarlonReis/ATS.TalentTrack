namespace ATS.Application.Candidatos.Queries.ListCandidatos;

public record ListCandidatosComCursorQuery(
    string? Cursor = null,
    int Limite = 20
);
