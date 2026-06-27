namespace ATS.Application.Tests.Vagas.Events;

using ATS.Application.Vagas.Events;
using ATS.Domain.Candidaturas.Events;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class FecharVagaAposPrimeiraAprovacaoHandlerTests
{
    private readonly Mock<IVagaRepository> _repoMock;
    private readonly FecharVagaAposPrimeiraAprovacaoHandler _handler;

    private static readonly Guid _candidaturaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid _candidatoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _vagaId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public FecharVagaAposPrimeiraAprovacaoHandlerTests()
    {
        _repoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new FecharVagaAposPrimeiraAprovacaoHandler(
            _repoMock.Object,
            NullLogger<FecharVagaAposPrimeiraAprovacaoHandler>.Instance);
    }

    private static Vaga CriarVagaAberta() =>
        Vaga.Criar("Dev Back-end", "Descrição", "Requisitos", 12000);

    private static CandidaturaAprovadaEvent CriarEvento(string? obs = null) =>
        new(_candidaturaId, _candidatoId, _vagaId, obs);

    [Fact]
    public async Task DeveFecharVagaQuandoElaEstaAbertaEAtualizarNoRepositorio()
    {
        var vaga = CriarVagaAberta();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        _repoMock
            .Setup(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(CriarEvento(), CancellationToken.None);

        Assert.Equal(StatusVaga.Fechada, vaga.Status);
        Assert.NotNull(vaga.DataEncerramento);
        _repoMock.Verify(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveIgnorarSemExcecaoQuandoVagaJaEstaFechada()
    {
        var vaga = CriarVagaAberta();
        vaga.Fechar();
        vaga.ClearDomainEvents();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);

        await _handler.Handle(CriarEvento(), CancellationToken.None);

        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveRetornarSemErroQuandoVagaNaoForEncontrada()
    {
        _repoMock
            .Setup(r => r.ObterPorIdAsync(_vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vaga?)null);

        await _handler.Handle(CriarEvento(), CancellationToken.None);

        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveRepassarCancellationTokenParaORepositorio()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var vaga = CriarVagaAberta();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_vagaId, ct))
            .ReturnsAsync(vaga);
        _repoMock
            .Setup(r => r.AtualizarAsync(vaga, ct))
            .Returns(Task.CompletedTask);

        await _handler.Handle(CriarEvento(), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(_vagaId, ct), Times.Once);
        _repoMock.Verify(r => r.AtualizarAsync(vaga, ct), Times.Once);
    }

    [Theory]
    [InlineData("Aprovado com distinção")]
    [InlineData(null)]
    public async Task DeveFuncionarIndependentementeDeObservacoes(string? observacoes)
    {
        var vaga = CriarVagaAberta();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        _repoMock
            .Setup(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(CriarEvento(observacoes), CancellationToken.None);

        Assert.Equal(StatusVaga.Fechada, vaga.Status);
    }
}
