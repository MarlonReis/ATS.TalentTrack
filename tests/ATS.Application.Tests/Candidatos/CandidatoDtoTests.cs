namespace ATS.Application.Tests.Candidatos;

using ATS.Application.Candidatos.DTOs;
using ATS.Domain.Candidatos.Entities;

public class CandidatoDtoTests
{
    [Fact]
    public void FromDomain_SemCurriculo_DeveMapearPropriedadesCorretamente()
    {
        var candidato = Candidato.Criar("Ana Lima", "ana@example.com", "11988880000");

        var dto = CandidatoDto.FromDomain(candidato);

        Assert.Equal(candidato.Id, dto.Id);
        Assert.Equal(candidato.Nome, dto.Nome);
        Assert.Equal(candidato.Email.Value, dto.Email);
        Assert.Equal(candidato.Telefone.Value, dto.Telefone);
        Assert.False(dto.PossuiCurriculo);
        Assert.Null(dto.NomeCurriculo);
        Assert.Null(dto.Curriculo);
        Assert.Equal(candidato.DataCadastro, dto.DataCadastro);
    }

    [Fact]
    public void FromDomain_ComCurriculo_DeveMapearCurriculoCorretamente()
    {
        var candidato = Candidato.Criar("Ana Lima", "ana@example.com", "11988880000");
        candidato.AdicionarCurriculo("curriculo.pdf", "application/pdf", "base64data==");

        var dto = CandidatoDto.FromDomain(candidato);

        Assert.True(dto.PossuiCurriculo);
        Assert.Equal("curriculo.pdf", dto.NomeCurriculo);
        Assert.NotNull(dto.Curriculo);
        Assert.Equal("curriculo.pdf", dto.Curriculo.NomeArquivo);
        Assert.Equal("application/pdf", dto.Curriculo.ContentType);
        Assert.Equal("base64data==", dto.Curriculo.UrlOuBase64);
    }
}
