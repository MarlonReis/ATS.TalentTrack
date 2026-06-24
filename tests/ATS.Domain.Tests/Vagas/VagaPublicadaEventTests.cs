using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Events;
using Xunit;

namespace ATS.Domain.Tests.Vagas;

public class VagaPublicadaEventTests
{
    private const string _descricaoValida = "Vaga para desenvolvedor .NET com experiência em DDD.";
    private const string _requisitosValidos = "5+ anos, .NET 8+, MongoDB";
    private const decimal _salarioValido = 12000m;

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "Dev Back-end Sênior")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Tech Lead")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Analista de Dados")]
    public void DeveCriarEventoComVagaIdETituloCorretamenteDefinidos(
            string vagaIdStr, string titulo)
    {
        var vagaId = Guid.Parse(vagaIdStr);
        var evento = new VagaPublicadaEvent(vagaId, titulo);
        Assert.Equal(vagaId, evento.VagaId);
        Assert.Equal(titulo, evento.Titulo);
    }
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "Dev Back-end Sênior")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Tech Lead")]
    public void DeveAtribuirVagaIdETituloAsPropriedadesCorrespondentes(
            string vagaIdStr, string titulo)
    {
        var vagaId = Guid.Parse(vagaIdStr);
        var evento = new VagaPublicadaEvent(vagaId, titulo);
        Assert.Equal(vagaId, evento.VagaId);
        Assert.Equal(titulo, evento.Titulo);
        Assert.NotEqual(vagaId.ToString(), evento.Titulo);
    }
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "Dev Back-end Sênior")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Tech Lead")]
    public void DeveDefinirOcorridoEmComoDataUtcNaCriacao(
            string vagaIdStr, string titulo)
    {
        var antes = DateTime.UtcNow;
        var evento = new VagaPublicadaEvent(Guid.Parse(vagaIdStr), titulo);
        Assert.Equal(DateTimeKind.Utc, evento.OcorridoEm.Kind);
        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("Dev Back-end Sênior", "Tech Lead")]
    [InlineData("Analista QA", "Product Manager")]
    public void DeveEventosDistintosTeremOcorridoEmIndependentes(
            string tituloA, string tituloB)
    {
        var eventoA = new VagaPublicadaEvent(Guid.NewGuid(), tituloA);
        var eventoB = new VagaPublicadaEvent(Guid.NewGuid(), tituloB);
        Assert.NotSame(eventoA, eventoB);
        Assert.True(eventoB.OcorridoEm >= eventoA.OcorridoEm);
    }
    [Theory]
    [InlineData("Dev Back-end Sênior", _descricaoValida, _requisitosValidos, 12000)]
    [InlineData("Tech Lead", _descricaoValida, _requisitosValidos, 18000)]
    public void DeveDispararExatamenteUmEventoVagaPublicadaAoCriarVaga(
            string titulo, string descricao, string requisitos, decimal salario)
    {
        var vaga = Vaga.Criar(titulo, descricao, requisitos, salario);
        var evento = Assert.Single(vaga.DomainEvents);
        Assert.IsType<VagaPublicadaEvent>(evento);
    }
    [Theory]
    [InlineData("Dev Back-end Sênior", _descricaoValida, _requisitosValidos, 12000)]
    [InlineData("Tech Lead", _descricaoValida, _requisitosValidos, 18000)]
    public void DeveEventoConterIdETituloDoAgregadoQuandoDisparado(
        string titulo, string descricao, string requisitos, decimal salario)
    {
        var vaga = Vaga.Criar(titulo, descricao, requisitos, salario);
        var evento = vaga.DomainEvents.OfType<VagaPublicadaEvent>().Single();
        Assert.Equal(vaga.Id, evento.VagaId);
        Assert.Equal(vaga.Titulo, evento.Titulo);
    }
    [Theory]
    [InlineData("  Dev Back-end Sênior  ", "Dev Back-end Sênior")]
    [InlineData("   Tech Lead", "Tech Lead")]
    public void DeveEventoConterTituloTrimadoQuandoTituloTiverEspacos(
        string tituloComEspacos, string tituloEsperado)
    {
        var vaga = Vaga.Criar(tituloComEspacos, _descricaoValida, _requisitosValidos, _salarioValido);
        var evento = vaga.DomainEvents.OfType<VagaPublicadaEvent>().Single();
        Assert.Equal(tituloEsperado, evento.Titulo);
        Assert.Equal(vaga.Titulo, evento.Titulo);
    }
    [Theory]
    [InlineData("Dev Back-end Sênior")]
    [InlineData("Tech Lead")]
    public void DeveOcorridoEmEstarDentroDoIntervaloEsperadoQuandoDisparadoPeloAgregado(
        string titulo)
    {
        var antes = DateTime.UtcNow;
        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);
        var evento = vaga.DomainEvents.OfType<VagaPublicadaEvent>().Single();
        Assert.Equal(DateTimeKind.Utc, evento.OcorridoEm.Kind);
        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow);
    }
    [Theory]
    [InlineData("Dev Back-end Sênior")]
    public void DeveOcorridoEmDoEventoSerPosteriorOuIgualADataAberturaDoAgregado(
        string titulo)
    {
        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);
        var evento = vaga.DomainEvents.OfType<VagaPublicadaEvent>().Single();
        Assert.True(evento.OcorridoEm >= vaga.DataAbertura);
    }
}
