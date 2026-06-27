namespace ATS.Domain.Tests.Candidaturas;

using ATS.Domain.Candidaturas.Events;
using Xunit;

public class CandidaturaCanceladaEventTests
{
    private static readonly Guid _candidaturaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid _candidatoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _vagaId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "1fa25a64-3317-1562-c3fc-1c863f66afa6",
                "2bc35b74-4427-2673-d4fd-2d974f77bfb7")]
    public void DeveCriarEventoComTodosOsIdsCorretos(
        string candidaturaIdStr, string candidatoIdStr, string vagaIdStr)
    {
        var candidaturaId = Guid.Parse(candidaturaIdStr);
        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        var evento = new CandidaturaCanceladaEvent(candidaturaId, candidatoId, vagaId);

        Assert.Equal(candidaturaId, evento.CandidaturaId);
        Assert.Equal(candidatoId, evento.CandidatoId);
        Assert.Equal(vagaId, evento.VagaId);
    }

    [Fact]
    public void DeveAtribuirCadaIdAPropriedadeCorrespondenteSemInverter()
    {
        var evento = new CandidaturaCanceladaEvent(_candidaturaId, _candidatoId, _vagaId);

        Assert.Equal(_candidaturaId, evento.CandidaturaId);
        Assert.Equal(_candidatoId, evento.CandidatoId);
        Assert.Equal(_vagaId, evento.VagaId);
        Assert.NotEqual(evento.CandidaturaId, evento.CandidatoId);
        Assert.NotEqual(evento.CandidatoId, evento.VagaId);
    }

    [Fact]
    public void OcorridoEmDeveSerProximoAoMomentoDeInstanciacao()
    {
        var antes = DateTime.UtcNow.AddSeconds(-1);

        var evento = new CandidaturaCanceladaEvent(_candidaturaId, _candidatoId, _vagaId);

        var depois = DateTime.UtcNow.AddSeconds(1);

        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= depois);
    }

    [Fact]
    public void DuasInstanciasComMesmosArgumentosDevemTerOcorridoEmIndependentes()
    {
        var ev1 = new CandidaturaCanceladaEvent(_candidaturaId, _candidatoId, _vagaId);
        var ev2 = new CandidaturaCanceladaEvent(_candidaturaId, _candidatoId, _vagaId);

        Assert.True(ev2.OcorridoEm >= ev1.OcorridoEm);
    }
}
