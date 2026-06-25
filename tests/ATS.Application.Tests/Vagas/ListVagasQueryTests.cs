
using ATS.Application.Vagas.Queries.ListVagas;
using ATS.Domain.Vagas.Enums;

namespace ATS.Application.Tests.Vagas;

public class ListVagasQueryTests
{
    [Theory]
    [InlineData(1, 20, null)]
    [InlineData(2, 10, StatusVaga.Aberta)]
    [InlineData(3, 5, StatusVaga.Fechada)]
    public void DeveCriarQueryComPropriedadesDefinidas(
        int pagina, int tamanho, StatusVaga? status)
    {
        var query = new ListVagasQuery(pagina, tamanho, status);

        Assert.Equal(pagina, query.Pagina);
        Assert.Equal(tamanho, query.TamanhoPagina);
        Assert.Equal(status, query.Status);
    }

    [Theory]
    [InlineData(1, 20)]
    public void DeveUsarValoresPadraoQuandoNaoInformados(int paginaEsperada, int tamanhoEsperado)
    {
        var query = new ListVagasQuery();

        Assert.Equal(paginaEsperada, query.Pagina);
        Assert.Equal(tamanhoEsperado, query.TamanhoPagina);
        Assert.Null(query.Status);
    }
}
