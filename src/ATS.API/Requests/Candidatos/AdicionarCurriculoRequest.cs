namespace ATS.API.Requests.Candidatos;

public sealed record AdicionarCurriculoRequest(
    string NomeArquivo,
    string ContentType,
    string UrlOuBase64);
