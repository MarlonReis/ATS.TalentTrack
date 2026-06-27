using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Events;
using Xunit;

namespace ATS.Domain.Tests.Candidaturas;

public class CandidaturaRealizadaEventTests
{
    [Theory]
    [InlineData(
        "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData(
        "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "1fa25a64-3317-1562-c3fc-1c863f66afa6",
        "2bc35b74-4427-2673-d4fd-2d974f77bfb7")]
    public void DeveCriarEventoComTodosOsIdsCorretamenteDefinidos(
        string candidaturaIdStr, string candidatoIdStr, string vagaIdStr)
    {

        var candidaturaId = Guid.Parse(candidaturaIdStr);
        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);


        var evento = new CandidaturaRealizadaEvent(candidaturaId, candidatoId, vagaId);


        Assert.Equal(candidaturaId, evento.CandidaturaId);
        Assert.Equal(candidatoId, evento.CandidatoId);
        Assert.Equal(vagaId, evento.VagaId);
    }


    [Theory]
    [InlineData(
        "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveAtribuirCadaIdAPropriedadeCorrespondenteSemInverter(
        string candidaturaIdStr, string candidatoIdStr, string vagaIdStr)
    {

        var candidaturaId = Guid.Parse(candidaturaIdStr);
        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);


        Assert.NotEqual(candidaturaId, candidatoId);
        Assert.NotEqual(candidaturaId, vagaId);
        Assert.NotEqual(candidatoId, vagaId);


        var evento = new CandidaturaRealizadaEvent(candidaturaId, candidatoId, vagaId);


        Assert.Equal(candidaturaId, evento.CandidaturaId);
        Assert.Equal(candidatoId, evento.CandidatoId);
        Assert.Equal(vagaId, evento.VagaId);

        Assert.NotEqual(evento.CandidaturaId, evento.CandidatoId);
        Assert.NotEqual(evento.CandidaturaId, evento.VagaId);
        Assert.NotEqual(evento.CandidatoId, evento.VagaId);
    }

    [Theory]
    [InlineData(
        "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveDefinirOcorridoEmComoDataUtcNaCriacao(
        string candidaturaIdStr, string candidatoIdStr, string vagaIdStr)
    {

        var antes = DateTime.UtcNow;
        var evento = new CandidaturaRealizadaEvent(
            Guid.Parse(candidaturaIdStr),
            Guid.Parse(candidatoIdStr),
            Guid.Parse(vagaIdStr));


        Assert.Equal(DateTimeKind.Utc, evento.OcorridoEm.Kind);
        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(
        "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveEventosDistintosTeremOcorridoEmIndependentes(
        string candidaturaIdStr, string candidatoIdStr, string vagaIdStr)
    {

        var eventoA = new CandidaturaRealizadaEvent(
            Guid.Parse(candidaturaIdStr),
            Guid.Parse(candidatoIdStr),
            Guid.Parse(vagaIdStr));

        var eventoB = new CandidaturaRealizadaEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());


        Assert.NotSame(eventoA, eventoB);
        Assert.True(eventoB.OcorridoEm >= eventoA.OcorridoEm);
    }

    [Theory]
    [InlineData(
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData(
        "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public void DeveDispararExatamenteUmEventoAoCriarCandidatura(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        var evento = Assert.Single(candidatura.DomainEvents);
        Assert.IsType<CandidaturaRealizadaEvent>(evento);
    }

    [Theory]
    [InlineData(
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData(
        "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public void DeveEventoConterIdsDaCandidaturaQuandoDisparadoPeloAgregado(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);


        var candidatura = Candidatura.Criar(candidatoId, vagaId);
        var evento = candidatura.DomainEvents
            .OfType<CandidaturaRealizadaEvent>()
            .Single();


        Assert.Equal(candidatura.Id, evento.CandidaturaId);
        Assert.Equal(candidatoId, evento.CandidatoId);
        Assert.Equal(vagaId, evento.VagaId);
    }

    [Theory]
    [InlineData(
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveOcorridoEmEstarDentroDoIntervaloEsperadoQuandoDisparadoPeloAgregado(
        string candidatoIdStr, string vagaIdStr)
    {

        var antes = DateTime.UtcNow;


        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        var evento = candidatura.DomainEvents
            .OfType<CandidaturaRealizadaEvent>()
            .Single();


        Assert.Equal(DateTimeKind.Utc, evento.OcorridoEm.Kind);
        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveOcorridoEmDoEventoSerPosteriorOuIgualADataCandidatura(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        var evento = candidatura.DomainEvents
            .OfType<CandidaturaRealizadaEvent>()
            .Single();


        Assert.True(evento.OcorridoEm >= candidatura.DataCandidatura);
    }
}
