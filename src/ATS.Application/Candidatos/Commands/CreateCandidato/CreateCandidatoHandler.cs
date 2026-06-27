namespace ATS.Application.Candidatos.Commands.CreateCandidato;

using ATS.Application.Candidatos.DTOs;
using ATS.Application.Common.Events;
using ATS.Application.Observability;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Microsoft.Extensions.Logging;

public partial class CreateCandidatoHandler
{
    private readonly ICandidatoRepository _repository;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogger<CreateCandidatoHandler> _logger;

    public CreateCandidatoHandler(
        ICandidatoRepository repository,
        IDomainEventDispatcher dispatcher,
        ILogger<CreateCandidatoHandler> logger)
    {
        _repository = repository;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task<CandidatoDto> HandleAsync(
        CreateCandidatoCommand command,
        CancellationToken ct = default)
    {
        var existente = await _repository.ObterPorEmailAsync(command.Email, ct);
        if (existente is not null)
        {
            throw new DomainException($"Já existe um candidato com o e-mail '{command.Email}'.");
        }

        var candidato = Candidato.Criar(command.Nome, command.Email, command.Telefone);

        await _repository.AdicionarAsync(candidato, ct);
        await _dispatcher.DispatchAndClearAsync(candidato, ct);

        AtsMetrics.CandidatosCriados.Add(1);

        LogCandidatoCriado(candidato.Id);

        return CandidatoDto.FromDomain(candidato);
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Candidato {CandidatoId} criado com sucesso")]
    private partial void LogCandidatoCriado(Guid candidatoId);
}
