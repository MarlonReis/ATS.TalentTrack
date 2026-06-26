
using ATS.Application.Vagas.Commands.FecharVaga;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Vagas;

public class FecharVagaHandlerTests
{
    private readonly Mock<IVagaRepository> _repoMock;
    private readonly FecharVagaHandler _handler;
    private static readonly Guid _guidVaga = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public FecharVagaHandlerTests()
    {
        _repoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new FecharVagaHandler(_repoMock.Object, NullLogger<FecharVagaHandler>.Instance);
    }

    private static Vaga CriarVagaAberta(string titulo = "Dev Back-end") =>
        Vaga.Criar(titulo, "Descrição", "Requisitos", 12000);


    [Theory]
    [InlineData("Dev Back-end")]
    [InlineData("Tech Lead")]
    public async Task DeveFecharVagaERetornarDtoComStatusFechada(string titulo)
    {
        var vaga = CriarVagaAberta(titulo);
        _repoMock.Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>())).ReturnsAsync(vaga);
        _repoMock.Setup(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var dto = await _handler.HandleAsync(new FecharVagaCommand(_guidVaga));

        Assert.Equal(StatusVaga.Fechada, dto.Status);
        Assert.Equal("Fechada", dto.StatusDescricao);
        Assert.NotNull(dto.DataEncerramento);
        _repoMock.Verify(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    public async Task DeveRepassarCancellationTokenParaAmbosOsMetodos(string titulo)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var vaga = CriarVagaAberta(titulo);

        _repoMock.Setup(r => r.ObterPorIdAsync(_guidVaga, ct)).ReturnsAsync(vaga);
        _repoMock.Setup(r => r.AtualizarAsync(vaga, ct)).Returns(Task.CompletedTask);

        await _handler.HandleAsync(new FecharVagaCommand(_guidVaga), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(_guidVaga, ct), Times.Once);
        _repoMock.Verify(r => r.AtualizarAsync(vaga, ct), Times.Once);
    }

    [Fact]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontrada()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Vaga?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new FecharVagaCommand(_guidVaga)));

        Assert.Equal("Vaga não encontrada.", excecao.Message);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeveLancarExcecaoDeDominioQuandoVagaJaEstiverFechada()
    {
        var vagaFechada = CriarVagaAberta();
        vagaFechada.Fechar();

        _repoMock.Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(vagaFechada);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new FecharVagaCommand(_guidVaga)));

        Assert.Equal("Vaga já está fechada.", excecao.Message);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
