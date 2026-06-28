namespace ATS.Application.Vagas.Queries.ListVagas;

using ATS.Application.Common.Pagination;
using ATS.Application.Vagas.DTOs;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;

public sealed class ListVagasComCursorHandler
{
    private readonly IVagaRepository _repository;

    public ListVagasComCursorHandler(IVagaRepository repository)
        => _repository = repository;

    public async Task<CursorPagedResult<VagaDto>> HandleAsync(
        ListVagasComCursorQuery query,
        CancellationToken ct = default)
    {
        if (query.Limite is < 1 or > 100)
        {
            throw new DomainException("Limite deve estar entre 1 e 100.");
        }

        var afterId = ParseCursor(query.Cursor);

        // Solicita limite+1 para detectar se há próxima página
        var items = (await _repository.ListarComCursorAsync(afterId, query.Limite + 1, ct)).ToList();

        if (query.Status.HasValue)
        {
            items = items.Where(v => v.Status == query.Status.Value).ToList();
        }

        var temMais = items.Count > query.Limite;
        if (temMais)
        {
            items.RemoveAt(items.Count - 1);
        }

        var proximoCursor = temMais ? EncodeCursor(items[^1].Id) : null;

        return new CursorPagedResult<VagaDto>(
            Items: items.Select(VagaDto.FromDomain),
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
