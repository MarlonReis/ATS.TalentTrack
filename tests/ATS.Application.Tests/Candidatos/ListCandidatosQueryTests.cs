using ATS.Application.Candidatos.Queries.ListCandidatos;

public class ListCandidatosQueryTests
{
    [Theory]
    [InlineData(1, 20)]
    [InlineData(3, 10)]
    public void DeveCriarQueryComPropriedadesDefinidas(int pagina, int tamanhoPagina)
    {
        var query = new ListCandidatosQuery(pagina, tamanhoPagina);

        Assert.Equal(pagina, query.Pagina);
        Assert.Equal(tamanhoPagina, query.TamanhoPagina);
    }

    [Theory]
    [InlineData(1, 20)]
    public void DeveUsarValoresPadraoQuandoNaoInformados(int paginaEsperada, int tamanhoEsperado)
    {
        var query = new ListCandidatosQuery();

        Assert.Equal(paginaEsperada, query.Pagina);
        Assert.Equal(tamanhoEsperado, query.TamanhoPagina);
    }

    [Theory]
    [InlineData(2, 10)]
    public void DeveSerIgualAOutraQueryComMesmosValores(int pagina, int tamanho)
    {
        Assert.Equal(
            new ListCandidatosQuery(pagina, tamanho),
            new ListCandidatosQuery(pagina, tamanho));
    }
}
