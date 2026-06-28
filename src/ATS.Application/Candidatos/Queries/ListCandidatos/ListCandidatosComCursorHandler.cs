namespace ATS.Application.Candidatos.Queries.ListCandidatos;

using ATS.Application.Candidatos.DTOs;
using ATS.Application.Common.Pagination;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;

public sealed class ListCandidatosComCursorHandler
{
    private readonly ICandidatoRepository _repository;

    public ListCandidatosComCursorHandler(ICandidatoRepository repository)
        => _repository = repository;

    public async Task<CursorPagedResult<CandidatoDto>> HandleAsync(
        ListCandidatosComCursorQuery query,
        CancellationToken ct = default)
    {
        if (query.Limite is < 1 or > 100)
        {
            throw new DomainException("Limite deve estar entre 1 e 100.");
        }

        var afterId = ParseCursor(query.Cursor);

        // Solicita limite+1 para detectar se há próxima página
        var items = (await _repository.ListarComCursorAsync(afterId, query.Limite + 1, ct)).ToList();

        var temMais = items.Count > query.Limite;
        if (temMais)
        {
            items.RemoveAt(items.Count - 1);
        }

        var proximoCursor = temMais ? EncodeCursor(items[^1].Id) : null;

        return new CursorPagedResult<CandidatoDto>(
            Items: items.Select(CandidatoDto.FromDomain),
            ProximoCursor: proximoCursor,
            TemMais: temMais);
    }

    private static Guid? ParseCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            return new Guid(bytes);
        }
        catch
        {
            return null;
        }
    }

    internal static string EncodeCursor(Guid id) =>
        Convert.ToBase64String(id.ToByteArray());
}
