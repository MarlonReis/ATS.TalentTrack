namespace ATS.Application.Vagas.Queries.GetVagaById;

using ATS.Application.Vagas.DTOs;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;

public sealed class GetVagaByIdHandler
{
    private readonly IVagaRepository _repository;

    public GetVagaByIdHandler(IVagaRepository repository)
    {
        _repository = repository;
    }

    public async Task<VagaDto> HandleAsync(
        GetVagaByIdQuery query,
        CancellationToken ct = default)
    {
        var vaga = await _repository.ObterPorIdAsync(query.Id, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        return VagaDto.FromDomain(vaga);
    }
}
