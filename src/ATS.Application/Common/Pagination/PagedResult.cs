namespace ATS.Application.Common.Pagination;

/// <summary>
/// Resultado paginado genérico retornado pelas queries de listagem.
/// </summary>
public record PagedResult<T>(
    IEnumerable<T> Items,
    long Total,
    int Pagina,
    int TamanhoPagina
)
{
    /// <summary>Total de páginas calculado a partir de Total / TamanhoPagina.</summary>
    public int TotalPaginas => TamanhoPagina > 0
        ? (int)Math.Ceiling((double)Total / TamanhoPagina)
        : 0;

    public bool TemProxima => Pagina < TotalPaginas;
    public bool TemAnterior => Pagina > 1;
}
