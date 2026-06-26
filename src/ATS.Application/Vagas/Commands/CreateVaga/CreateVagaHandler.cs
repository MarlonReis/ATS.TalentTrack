namespace ATS.Application.Vagas.Commands.CreateVaga;

using ATS.Application.Observability;
using ATS.Application.Vagas.DTOs;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging;

public sealed partial class CreateVagaHandler
{
    private readonly IVagaRepository _repository;
    private readonly ILogger<CreateVagaHandler> _logger;

    public CreateVagaHandler(
        IVagaRepository repository,
        ILogger<CreateVagaHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<VagaDto> HandleAsync(
        CreateVagaCommand command,
        CancellationToken ct = default)
    {
        var vaga = Vaga.Criar(
              command.Titulo,
              command.Descricao,
              command.Requisitos ?? string.Empty,
              command.Salario);

        await _repository.AdicionarAsync(vaga, ct);

        AtsMetrics.VagasCriadas.Add(1);

        LogVagaCriada(vaga.Id, vaga.Titulo);

        return VagaDto.FromDomain(vaga);
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information,
        Message = "Vaga {VagaId} '{Titulo}' criada com sucesso")]
    private partial void LogVagaCriada(Guid vagaId, string titulo);
}
