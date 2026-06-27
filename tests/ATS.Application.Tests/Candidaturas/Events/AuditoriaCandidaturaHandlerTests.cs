namespace ATS.Application.Tests.Candidaturas.Events;

using ATS.Application.Candidaturas.Events;
using ATS.Domain.Candidaturas.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class AuditoriaCandidaturaHandlerTests
{
    private readonly AuditoriaCandidaturaHandler _handler;

    private static readonly Guid _candidaturaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid _candidatoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _vagaId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public AuditoriaCandidaturaHandlerTests()
    {
        _handler = new AuditoriaCandidaturaHandler(
            NullLogger<AuditoriaCandidaturaHandler>.Instance);
    }

    [Fact]
    public async Task DeveTratarCandidaturaRealizadaEventSemExcecao()
    {
        var notification = new CandidaturaRealizadaEvent(_candidaturaId, _candidatoId, _vagaId);

        await _handler.Handle(notification, CancellationToken.None);
    }

    [Fact]
    public async Task DeveTratarCandidaturaAprovadaEventSemExcecao()
    {
        var notification = new CandidaturaAprovadaEvent(
            _candidaturaId, _candidatoId, _vagaId, "Ótimo candidato");

        await _handler.Handle(notification, CancellationToken.None);
    }

    [Fact]
    public async Task DeveTratarCandidaturaReprovadaEventSemExcecao()
    {
        var notification = new CandidaturaReprovadaEvent(
            _candidaturaId, _candidatoId, _vagaId, null);

        await _handler.Handle(notification, CancellationToken.None);
    }

    [Fact]
    public async Task DeveTratarCandidaturaCanceladaEventSemExcecao()
    {
        var notification = new CandidaturaCanceladaEvent(_candidaturaId, _candidatoId, _vagaId);

        await _handler.Handle(notification, CancellationToken.None);
    }

    [Fact]
    public async Task TodosOsHandlesDevemRetornarTaskConcluida()
    {
        var realizada = new CandidaturaRealizadaEvent(_candidaturaId, _candidatoId, _vagaId);
        var aprovada = new CandidaturaAprovadaEvent(_candidaturaId, _candidatoId, _vagaId, null);
        var reprovada = new CandidaturaReprovadaEvent(_candidaturaId, _candidatoId, _vagaId, null);
        var cancelada = new CandidaturaCanceladaEvent(_candidaturaId, _candidatoId, _vagaId);

        var t1 = _handler.Handle(realizada, CancellationToken.None);
        var t2 = _handler.Handle(aprovada, CancellationToken.None);
        var t3 = _handler.Handle(reprovada, CancellationToken.None);
        var t4 = _handler.Handle(cancelada, CancellationToken.None);

        Assert.True(t1.IsCompleted);
        Assert.True(t2.IsCompleted);
        Assert.True(t3.IsCompleted);
        Assert.True(t4.IsCompleted);

        await Task.WhenAll(t1, t2, t3, t4);
    }

    [Fact]
    public async Task DeveProcessarCancellationTokenEmTodosOsHandles()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        await _handler.Handle(
            new CandidaturaRealizadaEvent(_candidaturaId, _candidatoId, _vagaId), ct);
        await _handler.Handle(
            new CandidaturaAprovadaEvent(_candidaturaId, _candidatoId, _vagaId, null), ct);
        await _handler.Handle(
            new CandidaturaReprovadaEvent(_candidaturaId, _candidatoId, _vagaId, null), ct);
        await _handler.Handle(
            new CandidaturaCanceladaEvent(_candidaturaId, _candidatoId, _vagaId), ct);
    }
}
