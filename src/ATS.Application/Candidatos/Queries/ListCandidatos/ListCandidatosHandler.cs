namespace ATS.Application.Candidatos.Queries.ListCandidatos;

using ATS.Application.Candidatos.DTOs;
using ATS.Application.Common.Pagination;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;

public sealed class ListCandidatosHandler
{
    private readonly ICandidatoRepository _repository;

    public ListCandidatosHandler(ICandidatoRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<CandidatoDto>> HandleAsync(
        ListCandidatosQuery query,
        CancellationToken ct = default)
    {
        if (query.Pagina < 1)
        {
            throw new DomainException("Número da página deve ser maior que zero.");
        }

        if (query.TamanhoPagina is < 1 or > 100)
        {
            throw new DomainException("Tamanho da página deve estar entre 1 e 100.");
        }

        var candidatos = await _repository.ListarAsync(query.Pagina, query.TamanhoPagina, ct);
        var total = await _repository.ContarAsync(ct);

        var items = candidatos.Select(CandidatoDto.FromDomain);

        return new PagedResult<CandidatoDto>(
            Items: items,
            Total: total,
            Pagina: query.Pagina,
            TamanhoPagina: query.TamanhoPagina);
    }
}
