namespace ATS.Application.Candidatos.Queries.GetCandidatoById;

using ATS.Application.Candidatos.DTOs;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;

public sealed class GetCandidatoByIdHandler
{
    private readonly ICandidatoRepository _repository;

    public GetCandidatoByIdHandler(ICandidatoRepository repository)
    {
        _repository = repository;
    }

    public async Task<CandidatoDto> HandleAsync(
        GetCandidatoByIdQuery query,
        CancellationToken ct = default)
    {
        var candidato = await _repository.ObterPorIdAsync(query.Id, ct)
            ?? throw new DomainException("Candidato não encontrado.");

        return CandidatoDto.FromDomain(candidato);
    }
}
