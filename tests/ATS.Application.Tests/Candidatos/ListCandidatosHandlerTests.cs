using ATS.Application.Candidatos.Queries.ListCandidatos;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Candidatos;

public class ListCandidatosHandlerTests
{
    private readonly Mock<ICandidatoRepository> _repoMock;
    private readonly ListCandidatosHandler _handler;

    public ListCandidatosHandlerTests()
    {
        _repoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _handler = new ListCandidatosHandler(_repoMock.Object);
    }

    private static Candidato CriarCandidato(int i) =>
        Candidato.Criar($"Candidato {i}", $"c{i}@email.com", "11912345678");

    private void SetupListar(int pagina, int tamanho,
        IEnumerable<Candidato> candidatos, long total)
    {
        _repoMock
            .Setup(r => r.ListarAsync(pagina, tamanho, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatos);
        _repoMock
            .Setup(r => r.ContarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(total);
    }

    [Theory]
    [InlineData(1, 20, 3, 3)]
    [InlineData(2, 10, 5, 45)]
    public async Task DeveRetornarPagedResultComItensETotalCorretos(
        int pagina, int tamanho, int quantidadeItens, long total)
    {
        // Arrange
        var candidatos = Enumerable.Range(1, quantidadeItens)
            .Select(CriarCandidato)
            .ToList();

        SetupListar(pagina, tamanho, candidatos, total);

        var query = new ListCandidatosQuery(pagina, tamanho);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(quantidadeItens, result.Items.Count());
        Assert.Equal(total, result.Total);
        Assert.Equal(pagina, result.Pagina);
        Assert.Equal(tamanho, result.TamanhoPagina);
    }

    [Theory]
    [InlineData(1, 20)]
    public async Task DeveRetornarListaVaziaQuandoNaoHouverCandidatos(
        int pagina, int tamanho)
    {
        SetupListar(pagina, tamanho, Enumerable.Empty<Candidato>(), 0);

        var result = await _handler.HandleAsync(new ListCandidatosQuery(pagina, tamanho));

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Theory]
    [InlineData(1, 20, 3)]
    public async Task DeveMapearCandidatosParaDtoCorretamente(
        int pagina, int tamanho, int quantidade)
    {
        var candidatos = Enumerable.Range(1, quantidade).Select(CriarCandidato).ToList();
        SetupListar(pagina, tamanho, candidatos, quantidade);

        var result = await _handler.HandleAsync(new ListCandidatosQuery(pagina, tamanho));

        var dtos = result.Items.ToList();
        Assert.Equal(quantidade, dtos.Count);

        for (var i = 0; i < quantidade; i++)
        {
            Assert.Equal(candidatos[i].Nome, dtos[i].Nome);
            Assert.Equal(candidatos[i].Email.Value, dtos[i].Email);
        }
    }

    [Theory]
    [InlineData(1, 20)]
    public async Task DeveChamarListarAsyncEContarAsyncUmaVezCadaUm(
        int pagina, int tamanho)
    {
        SetupListar(pagina, tamanho, Enumerable.Empty<Candidato>(), 0);

        await _handler.HandleAsync(new ListCandidatosQuery(pagina, tamanho));

        _repoMock.Verify(
            r => r.ListarAsync(pagina, tamanho, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(
            r => r.ContarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, 20)]
    public async Task DeveRepassarCancellationTokenParaAmbosOsMetodos(
        int pagina, int tamanho)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _repoMock
            .Setup(r => r.ListarAsync(pagina, tamanho, ct))
            .ReturnsAsync(Enumerable.Empty<Candidato>());
        _repoMock
            .Setup(r => r.ContarAsync(ct))
            .ReturnsAsync(0L);

        await _handler.HandleAsync(new ListCandidatosQuery(pagina, tamanho), ct);

        _repoMock.Verify(r => r.ListarAsync(pagina, tamanho, ct), Times.Once);
        _repoMock.Verify(r => r.ContarAsync(ct), Times.Once);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    public async Task DeveLancarExcecaoQuandoPaginaForMenorQueUm(
        int pagina, int tamanho)
    {
        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListCandidatosQuery(pagina, tamanho)));

        Assert.Equal("Número da página deve ser maior que zero.", excecao.Message);
        _repoMock.Verify(
            r => r.ListarAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task DeveLancarExcecaoQuandoTamanhoDaPaginaForMenorQueUm(
        int pagina, int tamanho)
    {
        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListCandidatosQuery(pagina, tamanho)));

        Assert.Equal("Tamanho da página deve estar entre 1 e 100.", excecao.Message);
    }

    [Theory]
    [InlineData(1, 101)]
    [InlineData(1, 500)]
    public async Task DeveLancarExcecaoQuandoTamanhoDaPaginaExceder100(
        int pagina, int tamanho)
    {
        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListCandidatosQuery(pagina, tamanho)));

        Assert.Equal("Tamanho da página deve estar entre 1 e 100.", excecao.Message);
    }
}
