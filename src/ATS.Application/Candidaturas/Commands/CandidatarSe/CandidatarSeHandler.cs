namespace ATS.Application.Candidaturas.Commands.CandidatarSe;

using ATS.Application.Candidaturas.DTOs;
using ATS.Application.Common.Events;
using ATS.Application.Common.Validation;
using ATS.Application.Observability;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

public partial class CandidatarSeHandler
{
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ICandidatoRepository _candidatoRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly IValidator<CandidatarSeCommand> _validator;
    private readonly ILogger<CandidatarSeHandler> _logger;

    public CandidatarSeHandler(
        ICandidaturaRepository candidaturaRepository,
        ICandidatoRepository candidatoRepository,
        IVagaRepository vagaRepository,
        IDomainEventDispatcher dispatcher,
        IValidator<CandidatarSeCommand> validator,
        ILogger<CandidatarSeHandler> logger)
    {
        _candidaturaRepository = candidaturaRepository;
        _candidatoRepository = candidatoRepository;
        _vagaRepository = vagaRepository;
        _dispatcher = dispatcher;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CandidaturaDto> HandleAsync(
        CandidatarSeCommand command,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            throw new Application.Common.Validation.ValidationException(validation.Errors);
        }

        var candidato = await _candidatoRepository.ObterPorIdAsync(command.CandidatoId, ct)
            ?? throw new DomainException("Candidato não encontrado.");

        var vaga = await _vagaRepository.ObterPorIdAsync(command.VagaId, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        if (vaga.Status == StatusVaga.Fechada)
        {
            throw new DomainException("Não é possível se candidatar a uma vaga fechada.");
        }

        var jaCandidatou = await _candidaturaRepository.ExisteAsync(
            command.CandidatoId, command.VagaId, ct);

        if (jaCandidatou)
        {
            throw new DomainException("Candidato já se candidatou a esta vaga.");
        }

        var candidatura = Candidatura.Criar(command.CandidatoId, command.VagaId);
        await _candidaturaRepository.AdicionarAsync(candidatura, ct);
        await _dispatcher.DispatchAndClearAsync(candidatura, ct);

        AtsMetrics.CandidaturasCriadas.Add(1);

        LogCandidaturaCriada(candidatura.Id, command.CandidatoId, command.VagaId);

        return CandidaturaDto.FromDomain(candidatura, candidato.Nome, vaga.Titulo);
    }

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information,
        Message = "Candidatura {CandidaturaId} criada: candidato {CandidatoId} → vaga {VagaId}")]
    private partial void LogCandidaturaCriada(Guid candidaturaId, Guid candidatoId, Guid vagaId);
}
