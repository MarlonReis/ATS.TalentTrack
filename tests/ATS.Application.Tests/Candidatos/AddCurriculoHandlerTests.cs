using ATS.Application.Candidatos.Commands.AddCurriculo;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ATS.Application.Tests.Candidatos;

public class AddCurriculoHandlerTests
{
    private readonly Mock<ICandidatoRepository> _repoMock;
    private readonly AddCurriculoHandler _handler;

    private static readonly Guid _guidCandidato = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public AddCurriculoHandlerTests()
    {
        _repoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _handler = new AddCurriculoHandler(_repoMock.Object, NullLogger<AddCurriculoHandler>.Instance);
    }

    private static Candidato CriarCandidato() =>
        Candidato.Criar("João Silva", "joao@email.com", "11912345678");

    [Theory]
    [InlineData("cv.pdf", "application/pdf", "base64==")]
    [InlineData("portfolio.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "url")]
    public async Task DeveAdicionarCurriculoERetornarDtoAtualizado(
        string nomeArquivo, string contentType, string url)
    {

        var candidato = CriarCandidato();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _repoMock
            .Setup(r => r.AtualizarAsync(candidato, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AddCurriculoCommand(_guidCandidato, nomeArquivo, contentType, url);


        var dto = await _handler.HandleAsync(command);


        Assert.True(dto.PossuiCurriculo);
        Assert.Equal(nomeArquivo, dto.NomeCurriculo);
        _repoMock.Verify(
            r => r.AtualizarAsync(candidato, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("cv.pdf", "application/pdf", "base64==")]
    public async Task DeveRepassarCancellationTokenParaAmbosOsMetodos(
        string nomeArquivo, string contentType, string url)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var candidato = CriarCandidato();

        _repoMock.Setup(r => r.ObterPorIdAsync(_guidCandidato, ct)).ReturnsAsync(candidato);
        _repoMock.Setup(r => r.AtualizarAsync(candidato, ct)).Returns(Task.CompletedTask);

        await _handler.HandleAsync(
            new AddCurriculoCommand(_guidCandidato, nomeArquivo, contentType, url), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(_guidCandidato, ct), Times.Once);
        _repoMock.Verify(r => r.AtualizarAsync(candidato, ct), Times.Once);
    }

    [Theory]
    [InlineData("cv.pdf", "application/pdf", "base64==")]
    public async Task DeveLancarExcecaoQuandoCandidatoNaoForEncontrado(
        string nomeArquivo, string contentType, string url)
    {
        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(
                new AddCurriculoCommand(_guidCandidato, nomeArquivo, contentType, url)));

        Assert.Equal("Candidato não encontrado.", excecao.Message);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("foto.jpg", "image/jpeg", "base64==")]
    [InlineData("virus.exe", "application/octet-stream", "data")]
    public async Task DevePropagrarExcecaoDeDominioQuandoExtensaoNaoForPermitida(
        string nomeArquivo, string contentType, string url)
    {
        var candidato = CriarCandidato();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(
                new AddCurriculoCommand(_guidCandidato, nomeArquivo, contentType, url)));

        Assert.Contains("não permitido", excecao.Message);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
