namespace ATS.Domain.Tests.Candidaturas;

using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Events;
using Xunit;

public class CandidaturaEventosTests
{
    private static Candidatura CriarCandidatura(
        Guid? candidatoId = null,
        Guid? vagaId = null)
    {
        var c = Candidatura.Criar(
            candidatoId ?? Guid.NewGuid(),
            vagaId ?? Guid.NewGuid());
        c.ClearDomainEvents();
        return c;
    }

    private static readonly Guid _candidatoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _vagaId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public void AprovarDeveAdicionarCandidaturaAprovadaEventComDadosCorretos()
    {
        var candidatura = Candidatura.Criar(_candidatoId, _vagaId);

        candidatura.Aprovar("obs teste");

        Assert.Equal(2, candidatura.DomainEvents.Count);
        var events = candidatura.DomainEvents.ToList();
        Assert.IsType<CandidaturaRealizadaEvent>(events[0]);
        var evento = Assert.IsType<CandidaturaAprovadaEvent>(events[1]);
        Assert.Equal(candidatura.Id, evento.CandidaturaId);
        Assert.Equal(_candidatoId, evento.CandidatoId);
        Assert.Equal(_vagaId, evento.VagaId);
        Assert.Equal("obs teste", evento.Observacoes);
        Assert.True(evento.OcorridoEm >= DateTime.UtcNow.AddSeconds(-1));
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void AprovarDeveAdicionarCandidaturaAprovadaEventSemObservacoes()
    {
        var candidatura = CriarCandidatura(_candidatoId, _vagaId);

        candidatura.Aprovar(null);

        var evento = Assert.IsType<CandidaturaAprovadaEvent>(candidatura.DomainEvents.Single());
        Assert.Null(evento.Observacoes);
    }

    [Fact]
    public void ReprovarDeveAdicionarCandidaturaReprovadaEventComDadosCorretos()
    {
        var candidatura = Candidatura.Criar(_candidatoId, _vagaId);

        candidatura.Reprovar("motivo");

        Assert.Equal(2, candidatura.DomainEvents.Count);
        var evento = Assert.IsType<CandidaturaReprovadaEvent>(candidatura.DomainEvents.ToList()[1]);
        Assert.Equal(candidatura.Id, evento.CandidaturaId);
        Assert.Equal(_candidatoId, evento.CandidatoId);
        Assert.Equal(_vagaId, evento.VagaId);
        Assert.Equal("motivo", evento.Observacoes);
        Assert.True(evento.OcorridoEm >= DateTime.UtcNow.AddSeconds(-1));
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void ReprovarDeveAdicionarCandidaturaReprovadaEventSemObservacoes()
    {
        var candidatura = CriarCandidatura(_candidatoId, _vagaId);

        candidatura.Reprovar(null);

        var evento = Assert.IsType<CandidaturaReprovadaEvent>(candidatura.DomainEvents.Single());
        Assert.Null(evento.Observacoes);
    }

    [Fact]
    public void CancelarDeveAdicionarCandidaturaCanceladaEventComDadosCorretos()
    {
        var candidatura = Candidatura.Criar(_candidatoId, _vagaId);

        candidatura.Cancelar();

        Assert.Equal(2, candidatura.DomainEvents.Count);
        var evento = Assert.IsType<CandidaturaCanceladaEvent>(candidatura.DomainEvents.ToList()[1]);
        Assert.Equal(candidatura.Id, evento.CandidaturaId);
        Assert.Equal(_candidatoId, evento.CandidatoId);
        Assert.Equal(_vagaId, evento.VagaId);
    }

    [Fact]
    public void CancelarAposAprovacaoDeveAdicionarCandidaturaCanceladaEventAlemDosAnteriores()
    {
        var candidatura = Candidatura.Criar(_candidatoId, _vagaId);
        candidatura.Aprovar();
        candidatura.Cancelar();

        Assert.Equal(3, candidatura.DomainEvents.Count);
        var evts = candidatura.DomainEvents.ToList();
        Assert.IsType<CandidaturaRealizadaEvent>(evts[0]);
        Assert.IsType<CandidaturaAprovadaEvent>(evts[1]);
        Assert.IsType<CandidaturaCanceladaEvent>(evts[2]);
    }
}
