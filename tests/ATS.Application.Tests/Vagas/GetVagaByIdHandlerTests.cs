using ATS.Application.Vagas.Queries.GetVagaById;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Vagas;

public class GetVagaByIdHandlerTests
{
    private readonly Mock<IVagaRepository> _repoMock;
    private readonly GetVagaByIdHandler _handler;
    private static readonly Guid _guidVaga = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public GetVagaByIdHandlerTests()
    {
        _repoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new GetVagaByIdHandler(_repoMock.Object);
    }

    [Theory]
    [InlineData("Dev Back-end Sênior", "Descrição da vaga", "5+ anos", 12000)]
    [InlineData("Tech Lead", "Liderar equipe", "10+ anos", 18000)]
    public async Task DeveRetornarDtoComTodosOsDadosDaVaga(
        string titulo, string descricao, string requisitos, decimal salario)
    {
        // Arrange
        var vaga = Vaga.Criar(titulo, descricao, requisitos, salario);
        _repoMock.Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(vaga);

        // Act
        var dto = await _handler.HandleAsync(new GetVagaByIdQuery(_guidVaga));

        // Assert
        Assert.Equal(vaga.Id, dto.Id);
        Assert.Equal(titulo.Trim(), dto.Titulo);
        Assert.Equal(descricao.Trim(), dto.Descricao);
        Assert.Equal(requisitos.Trim(), dto.Requisitos);
        Assert.Equal(salario, dto.Salario);
        Assert.Equal(StatusVaga.Aberta, dto.Status);
        Assert.Equal("Aberta", dto.StatusDescricao);
    }

    [Fact]
    public async Task DeveNaoChamarAtualizarAsyncPorSerUmaQuery()
    {
        var vaga = Vaga.Criar("Titulo", "Desc", "Req", 10000);
        _repoMock.Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(vaga);

        await _handler.HandleAsync(new GetVagaByIdQuery(_guidVaga));

        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveRepassarCancellationToken()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var vaga = Vaga.Criar("Titulo", "Desc", "Req", 10000);

        _repoMock.Setup(r => r.ObterPorIdAsync(_guidVaga, ct)).ReturnsAsync(vaga);

        await _handler.HandleAsync(new GetVagaByIdQuery(_guidVaga), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(_guidVaga, ct), Times.Once);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontrada(string idStr)
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(Guid.Parse(idStr), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Vaga?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new GetVagaByIdQuery(Guid.Parse(idStr))));

        Assert.Equal("Vaga não encontrada.", excecao.Message);
    }
}
