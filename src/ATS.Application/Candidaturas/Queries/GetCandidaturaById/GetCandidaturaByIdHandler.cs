namespace ATS.Application.Candidaturas.Queries.GetCandidaturaById;

using ATS.Application.Candidaturas.DTOs;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;

public sealed class GetCandidaturaByIdHandler
{
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ICandidatoRepository _candidatoRepository;
    private readonly IVagaRepository _vagaRepository;

    public GetCandidaturaByIdHandler(
        ICandidaturaRepository candidaturaRepository,
        ICandidatoRepository candidatoRepository,
        IVagaRepository vagaRepository)
    {
        _candidaturaRepository = candidaturaRepository;
        _candidatoRepository = candidatoRepository;
        _vagaRepository = vagaRepository;
    }

    public async Task<CandidaturaDetalhadaDto> HandleAsync(
        GetCandidaturaByIdQuery query,
        CancellationToken ct = default)
    {
        var candidatura = await _candidaturaRepository.ObterPorIdAsync(query.Id, ct)
            ?? throw new DomainException("Candidatura não encontrada.");

        var candidato = await _candidatoRepository.ObterPorIdAsync(candidatura.CandidatoId, ct)
            ?? throw new DomainException("Candidato vinculado à candidatura não encontrado.");

        var vaga = await _vagaRepository.ObterPorIdAsync(candidatura.VagaId, ct)
            ?? throw new DomainException("Vaga vinculada à candidatura não encontrada.");

        return CandidaturaDetalhadaDto.FromDomain(candidatura, candidato, vaga.Titulo);
    }
}
