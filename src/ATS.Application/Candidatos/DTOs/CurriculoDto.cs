namespace ATS.Application.Candidatos.DTOs;

using ATS.Domain.Candidatos.ValueObjects;

public record CurriculoDto(
    string NomeArquivo,
    string ContentType,
    string UrlOuBase64,
    DateTime DataUpload
)
{
    public static CurriculoDto FromDomain(Curriculo curriculo) => new(
        curriculo.NomeArquivo,
        curriculo.ContentType,
        curriculo.UrlOuBase64,
        curriculo.DataUpload);
}
