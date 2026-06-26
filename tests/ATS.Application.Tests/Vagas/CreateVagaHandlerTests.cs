using ATS.Application.Vagas.Commands.CreateVaga;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Vagas;

public class CreateVagaHandlerTests
{
    private readonly Mock<IVagaRepository> _repoMock;
    private readonly CreateVagaHandler _handler;

    public CreateVagaHandlerTests()
    {
        _repoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new CreateVagaHandler(_repoMock.Object, NullLogger<CreateVagaHandler>.Instance);
    }

    [Theory]
    [InlineData("Dev Back-end Sênior", "Descrição da vaga", "5+ anos .NET", 12000)]
    [InlineData("Tech Lead", "Liderar time", "10+ anos", 18000)]
    public async Task DeveCriarVagaERetornarDtoComStatusAberta(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        _repoMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateVagaCommand(titulo, descricao, requisitos, salario);


        var dto = await _handler.HandleAsync(command);


        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal(titulo.Trim(), dto.Titulo);
        Assert.Equal(StatusVaga.Aberta, dto.Status);
        Assert.Equal("Aberta", dto.StatusDescricao);
        Assert.Null(dto.DataEncerramento);
    }

    [Theory]
    [InlineData("Dev Back-end", "Descrição", "Req", 10000)]
    public async Task DeveChamarAdicionarAsyncComVagaNoStatusAberta(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        _repoMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        await _handler.HandleAsync(
            new CreateVagaCommand(titulo, descricao, requisitos, salario));


        _repoMock.Verify(
            r => r.AdicionarAsync(
                It.Is<Vaga>(v =>
                    v.Titulo == titulo.Trim() &&
                    v.Status == StatusVaga.Aberta),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Dev Back-end", "Descrição", null, 10000)]
    public async Task DeveUsarStringVaziaParaRequisitosNulos(
        string titulo, string descricao, string? requisitos, decimal salario)
    {

        _repoMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        var dto = await _handler.HandleAsync(
            new CreateVagaCommand(titulo, descricao, requisitos, salario));


        Assert.Equal(string.Empty, dto.Requisitos);
    }

    [Theory]
    [InlineData("Dev Back-end", "Descrição", "Req", 10000)]
    public async Task DeveRepassarCancellationTokenParaAdicionarAsync(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _repoMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Vaga>(), ct))
            .Returns(Task.CompletedTask);


        await _handler.HandleAsync(
            new CreateVagaCommand(titulo, descricao, requisitos, salario), ct);


        _repoMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Vaga>(), ct), Times.Once);
    }

    [Theory]
    [InlineData("", "Descrição", "Req", 10000)]
    [InlineData("   ", "Descrição", "Req", 10000)]
    public async Task DevePropagrarExcecaoDeDominioQuandoTituloForVazio(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(
                new CreateVagaCommand(titulo, descricao, requisitos, salario)));

        Assert.Equal("Título da vaga é obrigatório.", excecao.Message);
        _repoMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
