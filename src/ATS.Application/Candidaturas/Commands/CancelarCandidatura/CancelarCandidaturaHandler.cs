namespace ATS.Application.Candidaturas.Commands.CancelarCandidatura;

using ATS.Application.Candidaturas.DTOs;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging;

public sealed partial class CancelarCandidaturaHandler
{
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ICandidatoRepository _candidatoRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly ILogger<CancelarCandidaturaHandler> _logger;

    public CancelarCandidaturaHandler(
        ICandidaturaRepository candidaturaRepository,
        ICandidatoRepository candidatoRepository,
        IVagaRepository vagaRepository,
        ILogger<CancelarCandidaturaHandler> logger)
    {
        _candidaturaRepository = candidaturaRepository;
        _candidatoRepository = candidatoRepository;
        _vagaRepository = vagaRepository;
        _logger = logger;
    }

    public async Task<CandidaturaDto> HandleAsync(
        CancelarCandidaturaCommand command,
        CancellationToken ct = default)
    {
        var candidatura = await _candidaturaRepository.ObterPorIdAsync(command.CandidaturaId, ct)
            ?? throw new DomainException("Candidatura não encontrada.");

        candidatura.Cancelar();

        await _candidaturaRepository.AtualizarAsync(candidatura, ct);

        LogCandidaturaCancelada(candidatura.Id, candidatura.CandidatoId, candidatura.VagaId);

        var candidato = await _candidatoRepository.ObterPorIdAsync(candidatura.CandidatoId, ct)
            ?? throw new DomainException("Candidato vinculado à candidatura não encontrado.");

        var vaga = await _vagaRepository.ObterPorIdAsync(candidatura.VagaId, ct)
            ?? throw new DomainException("Vaga vinculada à candidatura não encontrada.");

        return CandidaturaDto.FromDomain(candidatura, candidato.Nome, vaga.Titulo);
    }

    [LoggerMessage(EventId = 3004, Level = LogLevel.Information,
        Message = "Candidatura {CandidaturaId} cancelada (candidato {CandidatoId}, vaga {VagaId})")]
    private partial void LogCandidaturaCancelada(Guid candidaturaId, Guid candidatoId, Guid vagaId);
}
