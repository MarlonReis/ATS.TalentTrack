namespace ATS.Application.Vagas.Queries.ListVagas;

using ATS.Application.Common.Pagination;
using ATS.Application.Vagas.DTOs;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;

public sealed class ListVagasHandler
{
    private readonly IVagaRepository _repository;

    public ListVagasHandler(IVagaRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<VagaDto>> HandleAsync(
        ListVagasQuery query,
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

        var listarTask = _repository.ListarAsync(query.Pagina, query.TamanhoPagina, ct);
        var contarTask = _repository.ContarAsync(ct);

        await Task.WhenAll(listarTask, contarTask);

        var vagas = await listarTask;
        var total = await contarTask;

        var filtradas = query.Status.HasValue
            ? vagas.Where(v => v.Status == query.Status.Value)
            : vagas;

        var items = filtradas.Select(VagaDto.FromDomain);

        return new PagedResult<VagaDto>(
            Items: items,
            Total: total,
            Pagina: query.Pagina,
            TamanhoPagina: query.TamanhoPagina);
    }
}
