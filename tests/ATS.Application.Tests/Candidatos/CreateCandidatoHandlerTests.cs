using ATS.Application.Candidatos.Commands.CreateCandidato;
using ATS.Application.Common.Events;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Candidatos;

public class CreateCandidatoHandlerTests
{
    private readonly Mock<ICandidatoRepository> _repositoryMock;
    private readonly Mock<IDomainEventDispatcher> _dispatcherMock;
    private readonly CreateCandidatoHandler _handler;

    public CreateCandidatoHandlerTests()
    {
        _repositoryMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _dispatcherMock = new Mock<IDomainEventDispatcher>();
        _dispatcherMock
            .Setup(d => d.DispatchAndClearAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _handler = new CreateCandidatoHandler(
            _repositoryMock.Object,
            _dispatcherMock.Object,
            NullLogger<CreateCandidatoHandler>.Instance);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    [InlineData("Maria Santos", "maria@empresa.com.br", "21987654321")]
    [InlineData("Ana Lima", "ana.lima@corp.io", "1134567890")]
    public async Task DeveCriarESalvarCandidatoQuandoEmailNaoEstiverEmUso(
        string nome, string email, string telefone)
    {

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCandidatoCommand(nome, email, telefone);

        var dto = await _handler.HandleAsync(command);

        _repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(dto);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    [InlineData("Maria Santos", "maria@empresa.com.br", "21987654321")]
    public async Task DeveRetornarDtoComDadosCorretosDoCandidatoCriado(
        string nome, string email, string telefone)
    {

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCandidatoCommand(nome, email, telefone);


        var dto = await _handler.HandleAsync(command);


        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal(nome.Trim(), dto.Nome);
        Assert.Equal(email, dto.Email);
        Assert.Equal(telefone, dto.Telefone);
        Assert.False(dto.PossuiCurriculo);
        Assert.Null(dto.NomeCurriculo);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    [InlineData("Ana Lima", "ana@corp.io", "1134567890")]
    public async Task DeveChamarObterPorEmailAsyncComOEmailExatoDoCComando(
        string nome, string email, string telefone)
    {

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCandidatoCommand(nome, email, telefone);


        await _handler.HandleAsync(command);


        _repositoryMock.Verify(
            r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    [InlineData("Maria Santos", "maria@corp.com", "21987654321")]
    public async Task DeveSalvarCandidatoComDadosDoComando(
        string nome, string email, string telefone)
    {

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCandidatoCommand(nome, email, telefone);


        await _handler.HandleAsync(command);


        _repositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<Candidato>(c =>
                    c.Nome == nome.Trim() &&
                    c.Email.Value == email &&
                    c.Telefone.Value == telefone),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    public async Task DeveRepassarCancellationTokenParaAmbosOsMetodosDoRepositorio(
        string nome, string email, string telefone)
    {

        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync(email, ct))
            .ReturnsAsync((Candidato?)null);

        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Candidato>(), ct))
            .Returns(Task.CompletedTask);

        var command = new CreateCandidatoCommand(nome, email, telefone);


        await _handler.HandleAsync(command, ct);


        _repositoryMock.Verify(
            r => r.ObterPorEmailAsync(email, ct), Times.Once);

        _repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Candidato>(), ct), Times.Once);
    }

    [Theory]
    [InlineData("João Duplicado", "joao@email.com", "11912345678")]
    [InlineData("Maria Duplicada", "maria@empresa.com.br", "21987654321")]
    public async Task DeveLancarExcecaoQuandoEmailJaEstiverEmUso(
        string nome, string email, string telefone)
    {

        var existente = Candidato.Criar("Candidato Existente", email, "11900000000");

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        var command = new CreateCandidatoCommand(nome, email, telefone);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(command));


        Assert.Contains(email, excecao.Message);
    }

    [Theory]
    [InlineData("João Duplicado", "joao@email.com", "11912345678")]
    [InlineData("Ana Duplicada", "ana@corp.io", "1134567890")]
    public async Task DeveNaoChamarAdicionarAsyncQuandoEmailJaEstiverEmUso(
        string nome, string email, string telefone)
    {

        var existente = Candidato.Criar("Candidato Existente", email, "11900000000");

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        var command = new CreateCandidatoCommand(nome, email, telefone);


        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(command));


        _repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("joao@email.com")]
    [InlineData("maria@empresa.com.br")]
    public async Task DeveMensagemDeErroConterEmailDuplicado(string email)
    {

        var existente = Candidato.Criar("Existente", email, "11900000000");

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        var command = new CreateCandidatoCommand("Novo Candidato", email, "11912345678");


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(command));


        Assert.Equal($"Já existe um candidato com o e-mail '{email}'.", excecao.Message);
    }


    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    public async Task DeveVerificarEmailAntesDeAdicionarAoRepositorio(
        string nome, string email, string telefone)
    {

        var sequencia = new MockSequence();

        _repositoryMock
            .InSequence(sequencia)
            .Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        _repositoryMock
            .InSequence(sequencia)
            .Setup(r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCandidatoCommand(nome, email, telefone);


        await _handler.HandleAsync(command);


        _repositoryMock.Verify(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
