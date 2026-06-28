namespace ATS.Application.Tests.Candidatos;

using ATS.Application.Candidatos.DTOs;
using ATS.Domain.Candidatos.ValueObjects;

public class CurriculoDtoTests
{
    [Fact]
    public void FromDomain_DeveMapearTodasAsPropriedades()
    {
        var curriculo = Curriculo.Create("cv.pdf", "application/pdf", "base64abc==");

        var dto = CurriculoDto.FromDomain(curriculo);

        Assert.Equal(curriculo.NomeArquivo, dto.NomeArquivo);
        Assert.Equal(curriculo.ContentType, dto.ContentType);
        Assert.Equal(curriculo.UrlOuBase64, dto.UrlOuBase64);
        Assert.Equal(curriculo.DataUpload, dto.DataUpload);
    }

    [Fact]
    public void FromDomain_DeveRetornarRegistroComValoresCorretos()
    {
        var curriculo = Curriculo.Create("lattes.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "contenteBase64");

        var dto = CurriculoDto.FromDomain(curriculo);

        Assert.Equal("lattes.docx", dto.NomeArquivo);
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", dto.ContentType);
        Assert.Equal("contenteBase64", dto.UrlOuBase64);
    }
}
