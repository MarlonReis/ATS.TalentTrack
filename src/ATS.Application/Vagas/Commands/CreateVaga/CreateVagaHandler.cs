namespace ATS.Application.Vagas.Commands.CreateVaga;

using ATS.Application.Common.Events;
using ATS.Application.Common.Validation;
using ATS.Application.Observability;
using ATS.Application.Vagas.DTOs;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

public sealed partial class CreateVagaHandler
{
    private readonly IVagaRepository _repository;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly IValidator<CreateVagaCommand> _validator;
    private readonly ILogger<CreateVagaHandler> _logger;

    public CreateVagaHandler(
        IVagaRepository repository,
        IDomainEventDispatcher dispatcher,
        IValidator<CreateVagaCommand> validator,
        ILogger<CreateVagaHandler> logger)
    {
        _repository = repository;
        _dispatcher = dispatcher;
        _validator = validator;
        _logger = logger;
    }

    public async Task<VagaDto> HandleAsync(
        CreateVagaCommand command,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            throw new ATS.Application.Common.Validation.ValidationException(validation.Errors);
        }

        var vaga = Vaga.Criar(
              command.Titulo,
              command.Descricao,
              command.Requisitos ?? string.Empty,
              command.Salario);

        await _repository.AdicionarAsync(vaga, ct);
        await _dispatcher.DispatchAndClearAsync(vaga, ct);

        AtsMetrics.VagasCriadas.Add(1);

        LogVagaCriada(vaga.Id, vaga.Titulo);

        return VagaDto.FromDomain(vaga);
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information,
        Message = "Vaga {VagaId} '{Titulo}' criada com sucesso")]
    private partial void LogVagaCriada(Guid vagaId, string titulo);
}
