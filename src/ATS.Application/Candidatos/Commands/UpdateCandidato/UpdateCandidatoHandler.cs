namespace ATS.Application.Candidatos.Commands.UpdateCandidato;

using ATS.Application.Candidatos.DTOs;
using ATS.Application.Common.Events;
using ATS.Application.Common.Validation;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

public sealed partial class UpdateCandidatoHandler
{
    private readonly ICandidatoRepository _repository;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly IValidator<UpdateCandidatoCommand> _validator;
    private readonly ILogger<UpdateCandidatoHandler> _logger;

    public UpdateCandidatoHandler(
        ICandidatoRepository repository,
        IDomainEventDispatcher dispatcher,
        IValidator<UpdateCandidatoCommand> validator,
        ILogger<UpdateCandidatoHandler> logger)
    {
        _repository = repository;
        _dispatcher = dispatcher;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CandidatoDto> HandleAsync(
        UpdateCandidatoCommand command,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            throw new Common.Validation.ValidationException(validation.Errors);
        }

        var candidato = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Candidato não encontrado.");

        var comMesmoEmail = await _repository.ObterPorEmailAsync(command.Email, ct);
        if (comMesmoEmail is not null && comMesmoEmail.Id != command.Id)
        {
            throw new DomainException(
                $"Já existe outro candidato com o e-mail '{command.Email}'.");
        }

        candidato.AtualizarContato(command.Nome, command.Email, command.Telefone);

        await _repository.AtualizarAsync(candidato, ct);
        await _dispatcher.DispatchAndClearAsync(candidato, ct);

        LogCandidatoAtualizado(candidato.Id);

        return CandidatoDto.FromDomain(candidato);
    }

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information,
        Message = "Candidato {CandidatoId} atualizado com sucesso")]
    private partial void LogCandidatoAtualizado(Guid candidatoId);
}
