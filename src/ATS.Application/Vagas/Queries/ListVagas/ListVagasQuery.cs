namespace ATS.Application.Vagas.Queries.ListVagas;

using ATS.Domain.Vagas.Enums;

public record ListVagasQuery(
    int Pagina = 1,
    int TamanhoPagina = 20,
    StatusVaga? Status = null
);
