namespace ATS.Application.Candidaturas.Queries.ListCandidatosPorVaga;

using ATS.Application.Candidaturas.DTOs;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;

public class ListCandidatosPorVagaHandler
{
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ICandidatoRepository _candidatoRepository;
    private readonly IVagaRepository _vagaRepository;

    public ListCandidatosPorVagaHandler(
        ICandidaturaRepository candidaturaRepository,
        ICandidatoRepository candidatoRepository,
        IVagaRepository vagaRepository)
    {
        _candidaturaRepository = candidaturaRepository;
        _candidatoRepository = candidatoRepository;
        _vagaRepository = vagaRepository;
    }

    public async Task<IEnumerable<CandidaturaDetalhadaDto>> HandleAsync(
        ListCandidatosPorVagaQuery query,
        CancellationToken ct = default)
    {
        var vaga = await _vagaRepository.ObterPorIdAsync(query.VagaId, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        var candidaturas = await _candidaturaRepository.ListarPorVagaAsync(query.VagaId, ct);
        var resultado = new List<CandidaturaDetalhadaDto>();

        foreach (var candidatura in candidaturas)
        {
            var candidato = await _candidatoRepository.ObterPorIdAsync(candidatura.CandidatoId, ct);
            if (candidato is not null)
            {
                resultado.Add(CandidaturaDetalhadaDto.FromDomain(
                    candidatura, candidato, vaga.Titulo));
            }
        }

        return resultado;
    }
}
