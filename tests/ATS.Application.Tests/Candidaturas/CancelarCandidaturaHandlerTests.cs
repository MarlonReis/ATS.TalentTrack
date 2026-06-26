using ATS.Application.Candidaturas.Commands.CancelarCandidatura;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Candidaturas;

public class CancelarCandidaturaHandlerTests
{
    private readonly Mock<ICandidaturaRepository> _candidaturaRepoMock;
    private readonly Mock<ICandidatoRepository> _candidatoRepoMock;
    private readonly Mock<IVagaRepository> _vagaRepoMock;
    private readonly CancelarCandidaturaHandler _handler;

    private static readonly Guid _guidCandidatura = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid _guidCandidato = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _guidVaga = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public CancelarCandidaturaHandlerTests()
    {
        _candidaturaRepoMock = new Mock<ICandidaturaRepository>(MockBehavior.Strict);
        _candidatoRepoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _vagaRepoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new CancelarCandidaturaHandler(
            _candidaturaRepoMock.Object,
            _candidatoRepoMock.Object,
            _vagaRepoMock.Object,
            NullLogger<CancelarCandidaturaHandler>.Instance);
    }

    private static Candidato CriarCandidato() =>
        Candidato.Criar("João Silva", "joao@email.com", "11912345678");

    private static Vaga CriarVaga() =>
        Vaga.Criar("Dev Back-end", "Descrição", "Requisitos", 12000);

    private void SetupFluxoCompleto(Candidatura candidatura)
    {
        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);
        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidato());
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarVaga());
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveCancelarCandidaturaEmAnaliseERetornarDto(string _)
    {
        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);
        SetupFluxoCompleto(candidatura);

        var dto = await _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura));

        Assert.Equal(StatusCandidatura.Cancelado, dto.Status);
        Assert.Equal("Cancelado", dto.StatusDescricao);
    }

    [Fact]
    public async Task DeveCancelarCandidaturaAprovada()
    {
        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);
        candidatura.Aprovar("aprovada anteriormente");
        SetupFluxoCompleto(candidatura);

        var dto = await _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura));

        Assert.Equal(StatusCandidatura.Cancelado, dto.Status);
    }

    [Fact]
    public async Task DeveCancelarCandidaturaReprovada()
    {
        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);
        candidatura.Reprovar("reprovada anteriormente");
        SetupFluxoCompleto(candidatura);

        var dto = await _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura));

        Assert.Equal(StatusCandidatura.Cancelado, dto.Status);
    }

    [Fact]
    public async Task DeveChamarAtualizarAsyncUmaVez()
    {
        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);
        SetupFluxoCompleto(candidatura);

        await _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura));

        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveLancarExcecaoQuandoCandidaturaNaoForEncontrada()
    {
        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidatura?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura)));

        Assert.Equal("Candidatura não encontrada.", excecao.Message);
        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidatura>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveLancarExcecaoQuandoCandidaturaJaEstiverCancelada()
    {
        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);
        candidatura.Cancelar();

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura)));

        Assert.Equal("Candidatura já foi cancelada.", excecao.Message);
        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidatura>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveLancarExcecaoQuandoCandidatoNaoForEncontradoAposAtualizar()
    {

        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);

        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura)));


        Assert.Equal("Candidato vinculado à candidatura não encontrado.", excecao.Message);

        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()), Times.Once);

        _vagaRepoMock.Verify(
            r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontradaAposAtualizar()
    {

        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);

        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidato());

        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vaga?)null);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura)));


        Assert.Equal("Vaga vinculada à candidatura não encontrada.", excecao.Message);


        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveRepassarCancellationTokenParaTodosOsRepositorios()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, ct)).ReturnsAsync(candidatura);
        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, ct)).Returns(Task.CompletedTask);
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, ct)).ReturnsAsync(CriarCandidato());
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, ct)).ReturnsAsync(CriarVaga());

        await _handler.HandleAsync(new CancelarCandidaturaCommand(_guidCandidatura), ct);

        _candidaturaRepoMock.Verify(r => r.ObterPorIdAsync(_guidCandidatura, ct), Times.Once);
        _candidaturaRepoMock.Verify(r => r.AtualizarAsync(candidatura, ct), Times.Once);
        _candidatoRepoMock.Verify(r => r.ObterPorIdAsync(_guidCandidato, ct), Times.Once);
        _vagaRepoMock.Verify(r => r.ObterPorIdAsync(_guidVaga, ct), Times.Once);
    }
}
