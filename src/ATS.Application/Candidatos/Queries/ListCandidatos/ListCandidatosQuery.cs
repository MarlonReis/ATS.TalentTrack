namespace ATS.Application.Candidatos.Queries.ListCandidatos;

public record ListCandidatosQuery(
    int Pagina = 1,
    int TamanhoPagina = 20
);
