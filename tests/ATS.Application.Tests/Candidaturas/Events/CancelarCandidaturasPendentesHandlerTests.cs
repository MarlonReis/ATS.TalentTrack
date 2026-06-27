namespace ATS.Application.Tests.Candidaturas.Events;

using ATS.Application.Candidaturas.Events;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Vagas.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class CancelarCandidaturasPendentesHandlerTests
{
    private readonly Mock<ICandidaturaRepository> _repoMock;
    private readonly CancelarCandidaturasPendentesHandler _handler;

    private static readonly Guid _vagaId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public CancelarCandidaturasPendentesHandlerTests()
    {
        _repoMock = new Mock<ICandidaturaRepository>(MockBehavior.Strict);
        _handler = new CancelarCandidaturasPendentesHandler(
            _repoMock.Object,
            NullLogger<CancelarCandidaturasPendentesHandler>.Instance);
    }

    private static Candidatura CriarCandidaturaEmAnalise()
        => Candidatura.Criar(Guid.NewGuid(), _vagaId);

    [Fact]
    public async Task DeveCancelarTodasAsCandidaturasEmAnaliseAoReceberVagaFechada()
    {
        var c1 = CriarCandidaturaEmAnalise();
        var c2 = CriarCandidaturaEmAnalise();

        _repoMock
            .Setup(r => r.ListarPorVagaAsync(_vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { c1, c2 });
        _repoMock
            .Setup(r => r.AtualizarAsync(c1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock
            .Setup(r => r.AtualizarAsync(c2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var notification = new VagaFechadaEvent(_vagaId, "Dev Back-end");
        await _handler.Handle(notification, CancellationToken.None);

        Assert.Equal(StatusCandidatura.Cancelado, c1.Status);
        Assert.Equal(StatusCandidatura.Cancelado, c2.Status);
        _repoMock.Verify(r => r.AtualizarAsync(c1, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AtualizarAsync(c2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveIgnorarCandidaturasQueNaoEstejaEmAnalise()
    {
        var emAnalise = CriarCandidaturaEmAnalise();
        var aprovada = CriarCandidaturaEmAnalise();
        aprovada.Aprovar();

        _repoMock
            .Setup(r => r.ListarPorVagaAsync(_vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { emAnalise, aprovada });
        _repoMock
            .Setup(r => r.AtualizarAsync(emAnalise, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var notification = new VagaFechadaEvent(_vagaId, "Tech Lead");
        await _handler.Handle(notification, CancellationToken.None);

        Assert.Equal(StatusCandidatura.Cancelado, emAnalise.Status);
        Assert.Equal(StatusCandidatura.Aprovado, aprovada.Status);
        _repoMock.Verify(
            r => r.AtualizarAsync(aprovada, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeveNaoAtualizarNenhumaCandidaturaQuandoNaoHaCandidaturasEmAnalise()
    {
        var aprovada = CriarCandidaturaEmAnalise();
        aprovada.Aprovar();

        _repoMock
            .Setup(r => r.ListarPorVagaAsync(_vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { aprovada });

        var notification = new VagaFechadaEvent(_vagaId, "Tech Lead");
        await _handler.Handle(notification, CancellationToken.None);

        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidatura>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveNaoAtualizarNenhumaCandidaturaQuandoListaEstaVazia()
    {
        _repoMock
            .Setup(r => r.ListarPorVagaAsync(_vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Candidatura>());

        var notification = new VagaFechadaEvent(_vagaId, "Dev Back-end");
        await _handler.Handle(notification, CancellationToken.None);

        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidatura>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveRepassarCancellationTokenParaORepositorio()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _repoMock
            .Setup(r => r.ListarPorVagaAsync(_vagaId, ct))
            .ReturnsAsync(Array.Empty<Candidatura>());

        var notification = new VagaFechadaEvent(_vagaId, "Dev");
        await _handler.Handle(notification, ct);

        _repoMock.Verify(r => r.ListarPorVagaAsync(_vagaId, ct), Times.Once);
    }
}
