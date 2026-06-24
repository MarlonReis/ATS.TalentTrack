namespace ATS.Application.Candidaturas.Commands.CancelarCandidatura;

using ATS.Application.Candidaturas.DTOs;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;

public sealed class CancelarCandidaturaHandler
{
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ICandidatoRepository _candidatoRepository;
    private readonly IVagaRepository _vagaRepository;

    public CancelarCandidaturaHandler(
        ICandidaturaRepository candidaturaRepository,
        ICandidatoRepository candidatoRepository,
        IVagaRepository vagaRepository)
    {
        _candidaturaRepository = candidaturaRepository;
        _candidatoRepository = candidatoRepository;
        _vagaRepository = vagaRepository;
    }

    public async Task<CandidaturaDto> HandleAsync(
        CancelarCandidaturaCommand command,
        CancellationToken ct = default)
    {
        var candidatura = await _candidaturaRepository.ObterPorIdAsync(command.CandidaturaId, ct)
            ?? throw new DomainException("Candidatura não encontrada.");

        // Regra de negócio no domínio: lança exceção se já estiver Cancelado
        candidatura.Cancelar();

        await _candidaturaRepository.AtualizarAsync(candidatura, ct);

        var candidato = await _candidatoRepository.ObterPorIdAsync(candidatura.CandidatoId, ct)
            ?? throw new DomainException("Candidato vinculado à candidatura não encontrado.");

        var vaga = await _vagaRepository.ObterPorIdAsync(candidatura.VagaId, ct)
            ?? throw new DomainException("Vaga vinculada à candidatura não encontrada.");

        return CandidaturaDto.FromDomain(candidatura, candidato.Nome, vaga.Titulo);
    }
}
