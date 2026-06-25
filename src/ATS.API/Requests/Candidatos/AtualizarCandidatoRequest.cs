namespace ATS.API.Requests.Candidatos;

public sealed record AtualizarCandidatoRequest(
    string Nome,
    string Email,
    string Telefone);
