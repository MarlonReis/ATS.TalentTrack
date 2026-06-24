using ATS.Domain.Candidatos.ValueObjects;
using ATS.Domain.Shared;
using Xunit;

namespace ATS.Domain.Tests.Candidatos;

public class CurriculoTests
{

    [Theory]
    [InlineData("curriculo.pdf", "application/pdf", "base64content==")]
    [InlineData("meu_cv.doc", "application/msword", "https://storage/cv.doc")]
    [InlineData("portfolio.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "data:application/octet-stream;base64,abc")]
    public void DeveCriarCurriculoComTodasAsPropriedadesCorretamenteDefinidas(
        string nomeArquivo, string contentType, string urlOuBase64)
    {

        var antes = DateTime.UtcNow;


        var curriculo = Curriculo.Create(nomeArquivo, contentType, urlOuBase64);


        Assert.Equal(nomeArquivo, curriculo.NomeArquivo);
        Assert.Equal(contentType, curriculo.ContentType);
        Assert.Equal(urlOuBase64, curriculo.UrlOuBase64);
        Assert.True(curriculo.DataUpload >= antes);
        Assert.True(curriculo.DataUpload <= DateTime.UtcNow);
    }


    [Theory]
    [InlineData("cv.pdf", "application/pdf")]
    [InlineData("cv.doc", "application/msword")]
    [InlineData("cv.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("CV.PDF", "application/pdf")]
    public void DeveAceitarTodasAsExtensoesPermitidas(
        string nomeArquivo, string contentType)
    {

        var curriculo = Curriculo.Create(nomeArquivo, contentType, "conteudo");


        Assert.NotNull(curriculo);
        Assert.Equal(nomeArquivo, curriculo.NomeArquivo);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveLancarExcecaoQuandoNomeDoArquivoForNuloOuVazio(
        string? nomeArquivo)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Curriculo.Create(nomeArquivo!, "application/pdf", "conteudo"));


        Assert.Equal("Nome do arquivo do currículo é obrigatório.", excecao.Message);
    }


    [Theory]
    [InlineData("foto.jpg")]
    [InlineData("imagem.png")]
    [InlineData("planilha.xlsx")]
    [InlineData("script.exe")]
    [InlineData("semExtensao")]
    public void DeveLancarExcecaoQuandoExtensaoDoArquivoNaoForPermitida(
        string nomeArquivo)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Curriculo.Create(nomeArquivo, "application/octet-stream", "conteudo"));


        Assert.Contains("não permitido", excecao.Message);
        Assert.Contains("Use PDF, DOC ou DOCX", excecao.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveLancarExcecaoQuandoConteudoForNuloOuVazio(
        string? urlOuBase64)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Curriculo.Create("curriculo.pdf", "application/pdf", urlOuBase64!));


        Assert.Equal("Conteúdo do currículo é obrigatório.", excecao.Message);
    }


    [Theory]
    [InlineData("versao1.pdf", "versao2.pdf")]
    [InlineData("curriculo_antigo.doc", "curriculo_novo.doc")]
    public void DeveConsiderarCurriculosComNomesDiferentesComoNaoIguais(
        string nomeA, string nomeB)
    {

        var curriculoA = Curriculo.Create(nomeA, "application/pdf", "conteudoA");
        var curriculoB = Curriculo.Create(nomeB, "application/pdf", "conteudoB");


        Assert.NotEqual(curriculoA, curriculoB);
    }

    [Theory]
    [InlineData("curriculo.pdf")]
    [InlineData("portfolio.docx")]
    public void DeveRetornarIgualdadeReflexivaParaAMesmaInstancia(
        string nomeArquivo)
    {

        var curriculo = Curriculo.Create(nomeArquivo, "application/pdf", "conteudo");


        Assert.Equal(curriculo, curriculo);
    }

    [Theory]
    [InlineData("curriculo.pdf")]
    [InlineData("portfolio.docx")]
    public void DeveUsarDataUploadComoSegundoComponenteDeIgualdade(string nomeArquivo)
    {

        var curriculoA = Curriculo.Create(nomeArquivo, "application/pdf", "conteudo");
        var curriculoB = Curriculo.Create(nomeArquivo, "application/pdf", "conteudo");

        var saoIguais = curriculoA.Equals((object)curriculoB);

        var dataUploadIgual = curriculoA.DataUpload == curriculoB.DataUpload;
        Assert.Equal(dataUploadIgual, saoIguais);
    }


    [Theory]
    [InlineData("curriculo_v1.pdf", "curriculo_v2.pdf")]
    [InlineData("cv_antigo.doc", "cv_novo.doc")]
    public void DeveGerarHashCodesDiferentesParaCurriculosComNomesDiferentes(
        string nomeA, string nomeB)
    {
        var curriculoA = Curriculo.Create(nomeA, "application/pdf", "conteudo");
        var curriculoB = Curriculo.Create(nomeB, "application/pdf", "conteudo");

        var hashA = curriculoA.GetHashCode();
        var hashB = curriculoB.GetHashCode();

        Assert.NotEqual(hashA, hashB);
    }


    [Theory]
    [InlineData("curriculo.pdf")]
    [InlineData("cv.docx")]
    public void DeveGerarHashCodesDiferentesQuandoDataUploadDiferirMesmoComNomeIgual(
        string nomeArquivo)
    {

        var curriculoA = Curriculo.Create(nomeArquivo, "application/pdf", "conteudo");
        var curriculoB = Curriculo.Create(nomeArquivo, "application/pdf", "conteudo");

        var hashA = curriculoA.GetHashCode();
        var hashB = curriculoB.GetHashCode();

        var dataUploadIgual = curriculoA.DataUpload == curriculoB.DataUpload;
        Assert.Equal(dataUploadIgual, hashA == hashB);
    }

}
