namespace ATS.API.Requests.Vagas;

public sealed record AtualizarVagaRequest(
    string Titulo,
    string Descricao,
    string? Requisitos = null,
    decimal Salario = 0m);
