using ATS.Application.Common.Validation;
using ATS.Application.Vagas.Commands.DeleteVaga;
using ATS.Application.Vagas.Commands.UpdateVaga;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Vagas;

public class UpdateVagaHandlerTests
{
    private readonly Mock<IVagaRepository> _repoMock;
    private readonly UpdateVagaHandler _handler;

    private static readonly Guid _guidVaga = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public UpdateVagaHandlerTests()
    {
        _repoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new UpdateVagaHandler(_repoMock.Object, new UpdateVagaCommandValidator(), NullLogger<UpdateVagaHandler>.Instance);
    }

    private static Vaga CriarVaga(string titulo = "Dev Back-end") =>
        Vaga.Criar(titulo, "Descrição", "Requisitos", 12000);

    [Theory]
    [InlineData("Novo Título", "Nova descrição", "Novos req", 15000)]
    [InlineData("Outro Título", "Outra descrição", "Outros req", 9000)]
    public async Task DeveAtualizarVagaERetornarDtoAtualizado(
        string novoTitulo, string novaDescricao, string novosReq, decimal novoSalario)
    {

        var vaga = CriarVaga();
        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        _repoMock
            .Setup(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateVagaCommand(_guidVaga, novoTitulo, novaDescricao, novosReq, novoSalario);


        var dto = await _handler.HandleAsync(command);


        Assert.Equal(novoTitulo.Trim(), dto.Titulo);
        Assert.Equal(novaDescricao, dto.Descricao);
        Assert.Equal(novoSalario, dto.Salario);
        _repoMock.Verify(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Novo Título", "Nova descrição", 15000)]
    public async Task DeveUsarStringVaziaQuandoRequisitosForNulo(
        string titulo, string descricao, decimal salario)
    {

        var vaga = CriarVaga();
        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        _repoMock
            .Setup(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateVagaCommand(_guidVaga, titulo, descricao, null, salario);


        var dto = await _handler.HandleAsync(command);


        Assert.Equal(string.Empty, dto.Requisitos);
    }

    [Theory]
    [InlineData("Novo Título", "Nova descrição", "Req", 15000)]
    public async Task DeveConsultarVagaAntesDeAtualizar(
        string titulo, string descricao, string req, decimal salario)
    {
        var vaga = CriarVaga();
        var ordemChamadas = new List<string>();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga)
            .Callback(() => ordemChamadas.Add("ObterVaga"));
        _repoMock
            .Setup(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => ordemChamadas.Add("AtualizarVaga"));

        await _handler.HandleAsync(new UpdateVagaCommand(_guidVaga, titulo, descricao, req, salario));

        Assert.Equal(new[] { "ObterVaga", "AtualizarVaga" }, ordemChamadas);
    }

    [Theory]
    [InlineData("Novo Título", "Nova descrição", "Req", 15000)]
    public async Task DeveRepassarCancellationTokenParaAmbosOsMetodos(
        string titulo, string descricao, string req, decimal salario)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var vaga = CriarVaga();

        _repoMock.Setup(r => r.ObterPorIdAsync(_guidVaga, ct)).ReturnsAsync(vaga);
        _repoMock.Setup(r => r.AtualizarAsync(vaga, ct)).Returns(Task.CompletedTask);

        await _handler.HandleAsync(new UpdateVagaCommand(_guidVaga, titulo, descricao, req, salario), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(_guidVaga, ct), Times.Once);
        _repoMock.Verify(r => r.AtualizarAsync(vaga, ct), Times.Once);
    }

    [Theory]
    [InlineData("Novo Título", "Nova descrição", "Req", 15000)]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontrada(
        string titulo, string descricao, string req, decimal salario)
    {
        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vaga?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(
                new UpdateVagaCommand(_guidVaga, titulo, descricao, req, salario)));

        Assert.Equal("Vaga não encontrada.", excecao.Message);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("Novo Título", "Nova descrição", "Req", 15000)]
    public async Task DeveLancarExcecaoDeDominioQuandoVagaEstiverFechada(
        string titulo, string descricao, string req, decimal salario)
    {

        var vagaFechada = CriarVaga();
        vagaFechada.Fechar();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vagaFechada);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(
                new UpdateVagaCommand(_guidVaga, titulo, descricao, req, salario)));

        Assert.Equal("Não é possível editar uma vaga fechada.", excecao.Message);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("", "Descrição")]
    [InlineData("Titulo", "")]
    public async Task DeveLancarValidationExceptionQuandoComandoForInvalido(
        string titulo, string descricao)
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.HandleAsync(
                new UpdateVagaCommand(_guidVaga, titulo, descricao, null, 0m)));

        Assert.NotEmpty(ex.Errors);
        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
