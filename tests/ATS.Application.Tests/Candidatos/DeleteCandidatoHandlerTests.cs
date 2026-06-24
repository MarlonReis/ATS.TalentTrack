using ATS.Application.Candidatos.Commands.DeleteCandidato;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Candidatos;

public class DeleteCandidatoHandlerTests
{
    private readonly Mock<ICandidatoRepository> _repoMock;
    private readonly DeleteCandidatoHandler _handler;

    public DeleteCandidatoHandlerTests()
    {
        _repoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _handler = new DeleteCandidatoHandler(_repoMock.Object);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public async Task DeveRemoverCandidatoQuandoEncontrado(string idStr)
    {

        var id = Guid.Parse(idStr);
        var candidato = Candidato.Criar("João Silva", "joao@email.com", "11912345678");

        _repoMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _repoMock
            .Setup(r => r.RemoverAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        await _handler.HandleAsync(new DeleteCandidatoCommand(id));


        _repoMock.Verify(r => r.RemoverAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveChamarRemoverAsyncComIdCorreto(string idStr)
    {

        var id = Guid.Parse(idStr);
        var candidato = Candidato.Criar("João", "joao@email.com", "11912345678");

        _repoMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _repoMock
            .Setup(r => r.RemoverAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        await _handler.HandleAsync(new DeleteCandidatoCommand(id));


        _repoMock.Verify(
            r => r.RemoverAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveRepassarCancellationTokenParaAmbosOsMetodos(string idStr)
    {
        var id = Guid.Parse(idStr);
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        var candidato = Candidato.Criar("João", "joao@email.com", "11912345678");

        _repoMock.Setup(r => r.ObterPorIdAsync(id, ct)).ReturnsAsync(candidato);
        _repoMock.Setup(r => r.RemoverAsync(id, ct)).Returns(Task.CompletedTask);

        await _handler.HandleAsync(new DeleteCandidatoCommand(id), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(id, ct), Times.Once);
        _repoMock.Verify(r => r.RemoverAsync(id, ct), Times.Once);
    }


    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public async Task DeveLancarExcecaoQuandoCandidatoNaoForEncontrado(string idStr)
    {

        var id = Guid.Parse(idStr);

        _repoMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new DeleteCandidatoCommand(id)));


        Assert.Equal("Candidato não encontrado.", excecao.Message);
        _repoMock.Verify(
            r => r.RemoverAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
