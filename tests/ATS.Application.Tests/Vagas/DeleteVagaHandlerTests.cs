using ATS.Application.Vagas.Commands.DeleteVaga;
using ATS.Application.Vagas.Commands.UpdateVaga;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ATS.Application.Tests.Vagas;

public class DeleteVagaHandlerTests
{
    private readonly Mock<IVagaRepository> _repoMock;
    private readonly DeleteVagaHandler _handler;

    public DeleteVagaHandlerTests()
    {
        _repoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new DeleteVagaHandler(_repoMock.Object, NullLogger<DeleteVagaHandler>.Instance);
    }

    private static Vaga CriarVaga() =>
        Vaga.Criar("Dev Back-end", "Descrição", "Requisitos", 12000);

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveRemoverVagaQuandoEncontrada(string idStr)
    {
        var id = Guid.Parse(idStr);
        var vaga = CriarVaga();

        _repoMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(vaga);
        _repoMock.Setup(r => r.RemoverAsync(id, It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        await _handler.HandleAsync(new DeleteVagaCommand(id));

        _repoMock.Verify(r => r.RemoverAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveRepassarCancellationTokenParaAmbosOsMetodos(string idStr)
    {
        var id = Guid.Parse(idStr);
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _repoMock.Setup(r => r.ObterPorIdAsync(id, ct)).ReturnsAsync(CriarVaga());
        _repoMock.Setup(r => r.RemoverAsync(id, ct)).Returns(Task.CompletedTask);

        await _handler.HandleAsync(new DeleteVagaCommand(id), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(id, ct), Times.Once);
        _repoMock.Verify(r => r.RemoverAsync(id, ct), Times.Once);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontrada(string idStr)
    {
        var id = Guid.Parse(idStr);

        _repoMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Vaga?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new DeleteVagaCommand(id)));

        Assert.Equal("Vaga não encontrada.", excecao.Message);
        _repoMock.Verify(
            r => r.RemoverAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
