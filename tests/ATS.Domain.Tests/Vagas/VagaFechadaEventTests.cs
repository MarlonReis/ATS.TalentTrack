namespace ATS.Domain.Tests.Vagas;

using ATS.Domain.Vagas.Events;
using Xunit;

public class VagaFechadaEventTests
{
    private static readonly Guid _vagaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "Dev Back-end Sênior")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Tech Lead")]
    public void DeveCriarEventoComVagaIdETituloCorretos(string vagaIdStr, string titulo)
    {
        var vagaId = Guid.Parse(vagaIdStr);

        var evento = new VagaFechadaEvent(vagaId, titulo);

        Assert.Equal(vagaId, evento.VagaId);
        Assert.Equal(titulo, evento.Titulo);
    }

    [Theory]
    [InlineData("Engenheiro de Software")]
    [InlineData("Analista de Dados")]
    [InlineData("Gerente de Projetos")]
    public void DevePreservarTituloExatamenteComoPassado(string titulo)
    {
        var evento = new VagaFechadaEvent(_vagaId, titulo);

        Assert.Equal(titulo, evento.Titulo);
    }

    [Fact]
    public void OcorridoEmDeveSerProximoAoMomentoDeInstanciacao()
    {
        var antes = DateTime.UtcNow.AddSeconds(-1);

        var evento = new VagaFechadaEvent(_vagaId, "Dev Back-end");

        var depois = DateTime.UtcNow.AddSeconds(1);

        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= depois);
    }

    [Fact]
    public void DuasInstanciasComMesmosArgumentosDevemTerOcorridoEmIndependentes()
    {
        var ev1 = new VagaFechadaEvent(_vagaId, "Tech Lead");
        var ev2 = new VagaFechadaEvent(_vagaId, "Tech Lead");

        Assert.True(ev2.OcorridoEm >= ev1.OcorridoEm);
    }
}
