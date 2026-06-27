namespace ATS.Application.Candidaturas.Commands.AprovarCandidatura;

using ATS.Application.Candidaturas.DTOs;
using ATS.Application.Common.Events;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging;

public sealed partial class AprovarCandidaturaHandler
{
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ICandidatoRepository _candidatoRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogger<AprovarCandidaturaHandler> _logger;

    public AprovarCandidaturaHandler(
        ICandidaturaRepository candidaturaRepository,
        ICandidatoRepository candidatoRepository,
        IVagaRepository vagaRepository,
        IDomainEventDispatcher dispatcher,
        ILogger<AprovarCandidaturaHandler> logger)
    {
        _candidaturaRepository = candidaturaRepository;
        _candidatoRepository = candidatoRepository;
        _vagaRepository = vagaRepository;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task<CandidaturaDto> HandleAsync(
        AprovarCandidaturaCommand command,
        CancellationToken ct = default)
    {
        var candidatura = await _candidaturaRepository.ObterPorIdAsync(command.CandidaturaId, ct)
            ?? throw new DomainException("Candidatura não encontrada.");

        candidatura.Aprovar(command.Observacoes);

        await _candidaturaRepository.AtualizarAsync(candidatura, ct);
        await _dispatcher.DispatchAndClearAsync(candidatura, ct);

        LogCandidaturaAprovada(candidatura.Id, candidatura.CandidatoId, candidatura.VagaId);

        var candidato = await _candidatoRepository.ObterPorIdAsync(candidatura.CandidatoId, ct)
            ?? throw new DomainException("Candidato vinculado à candidatura não encontrado.");

        var vaga = await _vagaRepository.ObterPorIdAsync(candidatura.VagaId, ct)
            ?? throw new DomainException("Vaga vinculada à candidatura não encontrada.");

        return CandidaturaDto.FromDomain(candidatura, candidato.Nome, vaga.Titulo);
    }

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information,
        Message = "Candidatura {CandidaturaId} aprovada (candidato {CandidatoId}, vaga {VagaId})")]
    private partial void LogCandidaturaAprovada(Guid candidaturaId, Guid candidatoId, Guid vagaId);
}
