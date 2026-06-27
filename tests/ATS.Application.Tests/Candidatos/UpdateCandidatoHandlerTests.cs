using ATS.Application.Candidatos.Commands.UpdateCandidato;
using ATS.Application.Common.Events;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Candidatos;

public class UpdateCandidatoHandlerTests
{
    private readonly Mock<ICandidatoRepository> _repoMock;
    private readonly Mock<IDomainEventDispatcher> _dispatcherMock;
    private readonly UpdateCandidatoHandler _handler;

    private static readonly Guid _guidCandidato = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public UpdateCandidatoHandlerTests()
    {
        _repoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _dispatcherMock = new Mock<IDomainEventDispatcher>();
        _dispatcherMock
            .Setup(d => d.DispatchAndClearAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _handler = new UpdateCandidatoHandler(
            _repoMock.Object,
            _dispatcherMock.Object,
            NullLogger<UpdateCandidatoHandler>.Instance);
    }

    private static Candidato CriarCandidato(
        string nome = "João Silva",
        string email = "joao@email.com",
        string tel = "11912345678") =>
        Candidato.Criar(nome, email, tel);


    [Theory]
    [InlineData("Novo Nome", "novo@email.com", "21987654321")]
    [InlineData("Ana Lima", "ana@corp.io", "1134567890")]
    public async Task DeveAtualizarContatoERetornarDtoAtualizado(
        string novoNome, string novoEmail, string novoTelefone)
    {

        var candidato = CriarCandidato();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _repoMock
            .Setup(r => r.ObterPorEmailAsync(novoEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);
        _repoMock
            .Setup(r => r.AtualizarAsync(candidato, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateCandidatoCommand(_guidCandidato, novoNome, novoEmail, novoTelefone);


        var dto = await _handler.HandleAsync(command);


        Assert.Equal(novoNome.Trim(), dto.Nome);
        Assert.Equal(novoEmail, dto.Email);
        Assert.Equal(novoTelefone, dto.Telefone);
    }

    [Theory]
    [InlineData("João Silva", "joao@email.com", "11912345678")]
    public async Task DevePermitirMesmoEmailDoPropioCandidato(
        string nome, string email, string tel)
    {

        var candidato = CriarCandidato(nome, email, tel);
        var candidatoId = candidato.Id;

        _repoMock
            .Setup(r => r.ObterPorIdAsync(candidatoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _repoMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _repoMock
            .Setup(r => r.AtualizarAsync(candidato, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        var command = new UpdateCandidatoCommand(candidatoId, "Novo Nome", email, tel);


        var dto = await _handler.HandleAsync(command);
        Assert.NotNull(dto);
    }

    [Theory]
    [InlineData("Novo Nome", "novo@email.com", "21987654321")]
    public async Task DeveChamarAtualizarAsyncUmaVez(
        string novoNome, string novoEmail, string novoTel)
    {

        var candidato = CriarCandidato();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _repoMock
            .Setup(r => r.ObterPorEmailAsync(novoEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);
        _repoMock
            .Setup(r => r.AtualizarAsync(candidato, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        await _handler.HandleAsync(
            new UpdateCandidatoCommand(_guidCandidato, novoNome, novoEmail, novoTel));


        _repoMock.Verify(
            r => r.AtualizarAsync(candidato, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Novo Nome", "novo@email.com", "21987654321")]
    public async Task DeveRepassarCancellationTokenParaTodosOsMetodos(
        string novoNome, string novoEmail, string novoTel)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var candidato = CriarCandidato();

        _repoMock.Setup(r => r.ObterPorIdAsync(_guidCandidato, ct)).ReturnsAsync(candidato);
        _repoMock.Setup(r => r.ObterPorEmailAsync(novoEmail, ct)).ReturnsAsync((Candidato?)null);
        _repoMock.Setup(r => r.AtualizarAsync(candidato, ct)).Returns(Task.CompletedTask);

        await _handler.HandleAsync(
            new UpdateCandidatoCommand(_guidCandidato, novoNome, novoEmail, novoTel), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(_guidCandidato, ct), Times.Once);
        _repoMock.Verify(r => r.ObterPorEmailAsync(novoEmail, ct), Times.Once);
        _repoMock.Verify(r => r.AtualizarAsync(candidato, ct), Times.Once);
    }

    [Theory]
    [InlineData("Novo Nome", "novo@email.com", "21987654321")]
    public async Task DeveLancarExcecaoQuandoCandidatoNaoForEncontrado(
        string nome, string email, string tel)
    {
        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(
                new UpdateCandidatoCommand(_guidCandidato, nome, email, tel)));

        Assert.Equal("Candidato não encontrado.", excecao.Message);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("João Novo", "duplicado@email.com", "11912345678")]
    public async Task DeveLancarExcecaoQuandoEmailJaUsadoPorOutroCandidato(
        string nome, string email, string tel)
    {

        var candidatoAtual = CriarCandidato();
        var outroCandidato = CriarCandidato("Outro", email, "21911111111");

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatoAtual);
        _repoMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(outroCandidato);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(
                new UpdateCandidatoCommand(_guidCandidato, nome, email, tel)));

        Assert.Contains(email, excecao.Message);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
