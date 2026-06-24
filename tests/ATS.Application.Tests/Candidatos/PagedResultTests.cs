using ATS.Application.Common.Pagination;
using Xunit;

namespace ATS.Application.Tests.Common;

public class PagedResultTests
{

    [Theory]
    [InlineData(100, 20, 5)]   // exato
    [InlineData(101, 20, 6)]   // arredonda para cima
    [InlineData(1, 20, 1)]   // menos itens que o tamanho da página
    [InlineData(20, 20, 1)]   // exatamente uma página
    [InlineData(21, 20, 2)]   // um item a mais gera nova página
    public void DeveCalcularTotalDePaginasCorretamente(
        long total, int tamanhoPagina, int totalPaginasEsperado)
    {
        var result = new PagedResult<string>(
            Items: Enumerable.Empty<string>(),
            Total: total,
            Pagina: 1,
            TamanhoPagina: tamanhoPagina);

        Assert.Equal(totalPaginasEsperado, result.TotalPaginas);
    }

    [Theory]
    [InlineData(0)]
    public void DeveRetornarZeroQuandoTamanhoPaginaForZero(int tamanhoPagina)
    {
        var result = new PagedResult<string>(
            Items: Enumerable.Empty<string>(),
            Total: 100,
            Pagina: 1,
            TamanhoPagina: tamanhoPagina);

        Assert.Equal(0, result.TotalPaginas);
    }

    [Theory]
    [InlineData(1, 100, 20, true)]    // página 1 de 5 → tem próxima
    [InlineData(4, 100, 20, true)]    // página 4 de 5 → tem próxima
    [InlineData(5, 100, 20, false)]   // última página → não tem próxima
    [InlineData(1, 5, 20, false)]   // única página → não tem próxima
    public void DeveIndicarSeExisteProximaPagina(
        int pagina, long total, int tamanhoPagina, bool esperado)
    {
        var result = new PagedResult<string>(
            Items: Enumerable.Empty<string>(),
            Total: total,
            Pagina: pagina,
            TamanhoPagina: tamanhoPagina);

        Assert.Equal(esperado, result.TemProxima);
    }

    [Theory]
    [InlineData(1, false)]   // primeira página → não tem anterior
    [InlineData(2, true)]    // segunda página → tem anterior
    [InlineData(5, true)]    // qualquer página > 1 → tem anterior
    public void DeveIndicarSeExistePaginaAnterior(int pagina, bool esperado)
    {
        var result = new PagedResult<string>(
            Items: Enumerable.Empty<string>(),
            Total: 100,
            Pagina: pagina,
            TamanhoPagina: 20);

        Assert.Equal(esperado, result.TemAnterior);
    }

    [Theory]
    [InlineData(2, 50, 10)]
    [InlineData(1, 0, 20)]
    public void DeveExporPropriedadesComValoresDefinidos(
        int pagina, long total, int tamanhoPagina)
    {
        var items = new[] { "a", "b" };
        var result = new PagedResult<string>(items, total, pagina, tamanhoPagina);

        Assert.Equal(items, result.Items);
        Assert.Equal(total, result.Total);
        Assert.Equal(pagina, result.Pagina);
        Assert.Equal(tamanhoPagina, result.TamanhoPagina);
    }

    [Theory]
    [InlineData(2, 50, 10)]
    public void DeveSerIgualAOutroResultadoComMesmosValores(
        int pagina, long total, int tamanho)
    {
        var a = new PagedResult<string>(Array.Empty<string>(), total, pagina, tamanho);
        var b = new PagedResult<string>(Array.Empty<string>(), total, pagina, tamanho);

        Assert.Equal(a, b);
    }
}
