namespace ATS.Application.Candidatos.Commands.AddCurriculo;

public record AddCurriculoCommand(
    Guid CandidatoId,
    string NomeArquivo,
    string ContentType,
    string UrlOuBase64
);
