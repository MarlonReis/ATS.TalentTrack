namespace ATS.Application.Common.Pagination;

/// <summary>
/// Resultado de listagem por cursor. O cliente usa <see cref="ProximoCursor"/>
/// como parâmetro "cursor" na próxima requisição para obter a página seguinte.
/// </summary>
public record CursorPagedResult<T>(
    IEnumerable<T> Items,
    string? ProximoCursor,
    bool TemMais
);
