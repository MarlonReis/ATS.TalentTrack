using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Events;
using Xunit;

namespace ATS.Domain.Tests.Candidatos;

public class CurriculoAdicionadoEventTests
{
    private const string _nomeValido = "João da Silva";
    private const string _emailValido = "joao@email.com";
    private const string _telefoneValido = "11912345678";
    private const string _contentTypePdf = "application/pdf";


    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "curriculo.pdf")]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc", "portfolio.docx")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "cv.doc")]
    public void DeveCriarEventoComCandidatoIdENomeArquivoCorretamenteDefinidos(
        string candidatoIdStr, string nomeArquivo)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);


        var evento = new CurriculoAdicionadoEvent(candidatoId, nomeArquivo);


        Assert.Equal(candidatoId, evento.CandidatoId);
        Assert.Equal(nomeArquivo, evento.NomeArquivo);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "curriculo.pdf")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "portfolio.docx")]
    public void DeveAtribuirCandidatoIdENomeArquivoAsPropriedadesCorretas(
        string candidatoIdStr, string nomeArquivo)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);

        var evento = new CurriculoAdicionadoEvent(candidatoId, nomeArquivo);

        Assert.Equal(candidatoId, evento.CandidatoId);
        Assert.Equal(nomeArquivo, evento.NomeArquivo);
        Assert.NotEqual(candidatoId.ToString(), evento.NomeArquivo);
    }


    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "curriculo.pdf")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "portfolio.docx")]
    public void DeveDefinirOcorridoEmComoDataUtcNaCriacao(
        string candidatoIdStr, string nomeArquivo)
    {

        var antes = DateTime.UtcNow;
        var evento = new CurriculoAdicionadoEvent(Guid.Parse(candidatoIdStr), nomeArquivo);


        Assert.Equal(DateTimeKind.Utc, evento.OcorridoEm.Kind);


        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow);
    }


    [Theory]
    [InlineData("curriculo_v1.pdf", "curriculo_v2.pdf")]
    [InlineData("cv_antigo.doc", "cv_novo.docx")]
    public void DeveEventosDistintosTeremOcorridoEmIndependentes(
        string nomeArquivoA, string nomeArquivoB)
    {

        var eventoA = new CurriculoAdicionadoEvent(Guid.NewGuid(), nomeArquivoA);
        var eventoB = new CurriculoAdicionadoEvent(Guid.NewGuid(), nomeArquivoB);


        Assert.NotSame(eventoA, eventoB);
        Assert.True(eventoB.OcorridoEm >= eventoA.OcorridoEm);
    }

    [Theory]
    [InlineData("curriculo.pdf", _contentTypePdf, "base64==")]
    [InlineData("portfolio.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "https://storage/cv.docx")]
    public void DeveConterIdDoCandidatoENomeArquivoQuandoDisparadoPeloAgregado(
        string nomeArquivo, string contentType, string urlOuBase64)
    {

        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.ClearDomainEvents();


        candidato.AdicionarCurriculo(nomeArquivo, contentType, urlOuBase64);


        var evento = candidato.DomainEvents.OfType<CurriculoAdicionadoEvent>().Single();
        Assert.Equal(candidato.Id, evento.CandidatoId);
        Assert.Equal(nomeArquivo, evento.NomeArquivo);
    }

    [Theory]
    [InlineData("curriculo.pdf", _contentTypePdf, "base64==")]
    [InlineData("portfolio.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "https://storage/cv.docx")]
    public void DeveDispararExatamenteUmEventoAoAdicionarCurriculo(
        string nomeArquivo, string contentType, string urlOuBase64)
    {

        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.ClearDomainEvents();


        candidato.AdicionarCurriculo(nomeArquivo, contentType, urlOuBase64);


        var evento = Assert.Single(candidato.DomainEvents);
        Assert.IsType<CurriculoAdicionadoEvent>(evento);
    }

    [Theory]
    [InlineData("curriculo_v1.pdf", "curriculo_v2.pdf")]
    [InlineData("cv_antigo.doc", "cv_novo.docx")]
    public void DeveEventoDeSegundaAdicaoConterNomeDoNovoArquivo(
        string nomeAntigo, string nomeNovo)
    {

        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.AdicionarCurriculo(nomeAntigo, _contentTypePdf, "conteudoAntigo");
        candidato.ClearDomainEvents();


        candidato.AdicionarCurriculo(nomeNovo, _contentTypePdf, "conteudoNovo");


        var evento = candidato.DomainEvents.OfType<CurriculoAdicionadoEvent>().Single();
        Assert.Equal(nomeNovo, evento.NomeArquivo);
        Assert.NotEqual(nomeAntigo, evento.NomeArquivo);
    }

    [Theory]
    [InlineData("curriculo_v1.pdf", "curriculo_v2.pdf")]
    public void DeveIdDoCandidatoPermaneceOMesmoNaSubstituicaoDeCurriculo(
        string nomeAntigo, string nomeNovo)
    {

        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.AdicionarCurriculo(nomeAntigo, _contentTypePdf, "conteudoAntigo");
        candidato.ClearDomainEvents();


        candidato.AdicionarCurriculo(nomeNovo, _contentTypePdf, "conteudoNovo");


        var evento = candidato.DomainEvents.OfType<CurriculoAdicionadoEvent>().Single();
        Assert.Equal(candidato.Id, evento.CandidatoId);
    }


    [Theory]
    [InlineData("curriculo.pdf", _contentTypePdf, "base64==")]
    public void DeveOcorridoEmDoEventoSerPosteriorOuIgualAoCandidatoCriadoEvent(
        string nomeArquivo, string contentType, string urlOuBase64)
    {

        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        var eventoCriacao = candidato.DomainEvents.OfType<CandidatoCriadoEvent>().Single();

        candidato.AdicionarCurriculo(nomeArquivo, contentType, urlOuBase64);
        var eventoCurriculo = candidato.DomainEvents.OfType<CurriculoAdicionadoEvent>().Single();


        Assert.True(eventoCurriculo.OcorridoEm >= eventoCriacao.OcorridoEm);
    }

    [Theory]
    [InlineData("curriculo.pdf", _contentTypePdf, "base64==")]
    public void DeveOcorridoEmEstarDentroDoIntervaloEsperadoQuandoDisparadoPeloAgregado(
        string nomeArquivo, string contentType, string urlOuBase64)
    {

        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.ClearDomainEvents();


        var antes = DateTime.UtcNow;


        candidato.AdicionarCurriculo(nomeArquivo, contentType, urlOuBase64);


        var evento = candidato.DomainEvents.OfType<CurriculoAdicionadoEvent>().Single();
        Assert.Equal(DateTimeKind.Utc, evento.OcorridoEm.Kind);
        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow);
    }
}
