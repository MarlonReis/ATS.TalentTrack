namespace ATS.Application.Vagas.Commands.UpdateVaga;

using ATS.Application.Common.Validation;
using ATS.Application.Vagas.DTOs;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

public sealed partial class UpdateVagaHandler
{
    private readonly IVagaRepository _repository;
    private readonly IValidator<UpdateVagaCommand> _validator;
    private readonly ILogger<UpdateVagaHandler> _logger;

    public UpdateVagaHandler(
        IVagaRepository repository,
        IValidator<UpdateVagaCommand> validator,
        ILogger<UpdateVagaHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<VagaDto> HandleAsync(
        UpdateVagaCommand command,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            throw new ATS.Application.Common.Validation.ValidationException(validation.Errors);
        }

        var vaga = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        vaga.Atualizar(
            command.Titulo,
            command.Descricao,
            command.Requisitos ?? string.Empty,
            command.Salario);

        await _repository.AtualizarAsync(vaga, ct);

        LogVagaAtualizada(vaga.Id);

        return VagaDto.FromDomain(vaga);
    }

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information,
        Message = "Vaga {VagaId} atualizada com sucesso")]
    private partial void LogVagaAtualizada(Guid vagaId);
}
