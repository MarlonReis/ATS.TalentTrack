using ATS.Application.Vagas.Queries.ListVagas;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Vagas;

public class ListVagasHandlerTests
{
    private readonly Mock<IVagaRepository> _repoMock;
    private readonly ListVagasHandler _handler;

    public ListVagasHandlerTests()
    {
        _repoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new ListVagasHandler(_repoMock.Object);
    }

    private static Vaga CriarVaga(string titulo = "Dev", bool fechar = false)
    {
        var vaga = Vaga.Criar(titulo, "Descrição", "Requisitos", 12000);
        if (fechar)
        {
            vaga.Fechar();
        }

        return vaga;
    }

    private void SetupListar(int pagina, int tamanho,
        IEnumerable<Vaga> vagas, long total)
    {
        _repoMock
            .Setup(r => r.ListarAsync(pagina, tamanho, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vagas);
        _repoMock
            .Setup(r => r.ContarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(total);
    }

    [Theory]
    [InlineData(1, 20, 3, 3)]
    [InlineData(2, 10, 5, 45)]
    public async Task DeveRetornarPagedResultComItensETotalCorretos(
        int pagina, int tamanho, int quantidade, long total)
    {
        var vagas = Enumerable.Range(1, quantidade)
            .Select(i => CriarVaga($"Vaga {i}"))
            .ToList();

        SetupListar(pagina, tamanho, vagas, total);

        var result = await _handler.HandleAsync(new ListVagasQuery(pagina, tamanho));

        Assert.Equal(quantidade, result.Items.Count());
        Assert.Equal(total, result.Total);
        Assert.Equal(pagina, result.Pagina);
        Assert.Equal(tamanho, result.TamanhoPagina);
    }

    [Theory]
    [InlineData(1, 20)]
    public async Task DeveRetornarListaVaziaQuandoNaoHouverVagas(int pagina, int tamanho)
    {
        SetupListar(pagina, tamanho, Enumerable.Empty<Vaga>(), 0);

        var result = await _handler.HandleAsync(new ListVagasQuery(pagina, tamanho));

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }


    [Theory]
    [InlineData(StatusVaga.Aberta, 2, 1)]   // 2 abertas, 1 fechada → retorna 2
    [InlineData(StatusVaga.Fechada, 1, 2)]   // 1 aberta,  2 fechadas → retorna 2
    public async Task DeveFiltrarVagasPorStatusQuandoEspecificado(
        StatusVaga statusFiltro, int quantAbertas, int quantFechadas)
    {
        // Arrange
        var abertas = Enumerable.Range(1, quantAbertas).Select(i => CriarVaga($"Aberta {i}")).ToList();
        var fechadas = Enumerable.Range(1, quantFechadas).Select(i => CriarVaga($"Fechada {i}", fechar: true)).ToList();
        var todas = abertas.Concat(fechadas).ToList();
        var esperado = statusFiltro == StatusVaga.Aberta ? quantAbertas : quantFechadas;

        SetupListar(1, 20, todas, todas.Count);

        // Act
        var result = await _handler.HandleAsync(new ListVagasQuery(1, 20, statusFiltro));

        // Assert
        Assert.Equal(esperado, result.Items.Count());
        Assert.All(result.Items, dto => Assert.Equal(statusFiltro, dto.Status));
    }

    [Theory]
    [InlineData(1, 20)]
    public async Task DeveRetornarTodasAsVagasQuandoStatusForNulo(int pagina, int tamanho)
    {
        var vagas = new[]
        {
            CriarVaga("Aberta 1"),
            CriarVaga("Fechada 1", fechar: true),
            CriarVaga("Aberta 2")
        };

        SetupListar(pagina, tamanho, vagas, vagas.Length);

        var result = await _handler.HandleAsync(new ListVagasQuery(pagina, tamanho, null));

        Assert.Equal(3, result.Items.Count());
    }

    [Theory]
    [InlineData(1, 20)]
    public async Task DeveChamarListarAsyncEContarAsyncUmaVezCadaUm(int pagina, int tamanho)
    {
        SetupListar(pagina, tamanho, Enumerable.Empty<Vaga>(), 0);

        await _handler.HandleAsync(new ListVagasQuery(pagina, tamanho));

        _repoMock.Verify(r => r.ListarAsync(pagina, tamanho, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.ContarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, 20)]
    public async Task DeveRepassarCancellationTokenParaAmbosOsMetodos(int pagina, int tamanho)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _repoMock.Setup(r => r.ListarAsync(pagina, tamanho, ct)).ReturnsAsync(Enumerable.Empty<Vaga>());
        _repoMock.Setup(r => r.ContarAsync(ct)).ReturnsAsync(0L);

        await _handler.HandleAsync(new ListVagasQuery(pagina, tamanho), ct);

        _repoMock.Verify(r => r.ListarAsync(pagina, tamanho, ct), Times.Once);
        _repoMock.Verify(r => r.ContarAsync(ct), Times.Once);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    public async Task DeveLancarExcecaoQuandoPaginaForMenorQueUm(int pagina, int tamanho)
    {
        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListVagasQuery(pagina, tamanho)));

        Assert.Equal("Número da página deve ser maior que zero.", excecao.Message);
        _repoMock.Verify(
            r => r.ListarAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task DeveLancarExcecaoQuandoTamanhoPaginaForMenorQueUm(int pagina, int tamanho)
    {
        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListVagasQuery(pagina, tamanho)));

        Assert.Equal("Tamanho da página deve estar entre 1 e 100.", excecao.Message);
    }

    [Theory]
    [InlineData(1, 101)]
    [InlineData(1, 500)]
    public async Task DeveLancarExcecaoQuandoTamanhoPaginaExceder100(int pagina, int tamanho)
    {
        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListVagasQuery(pagina, tamanho)));

        Assert.Equal("Tamanho da página deve estar entre 1 e 100.", excecao.Message);
    }
}
