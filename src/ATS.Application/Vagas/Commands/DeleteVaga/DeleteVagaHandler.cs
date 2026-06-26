namespace ATS.Application.Vagas.Commands.DeleteVaga;

using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging;

public sealed partial class DeleteVagaHandler
{
    private readonly IVagaRepository _repository;
    private readonly ILogger<DeleteVagaHandler> _logger;

    public DeleteVagaHandler(
        IVagaRepository repository,
        ILogger<DeleteVagaHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(
        DeleteVagaCommand command,
        CancellationToken ct = default)
    {
        _ = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        await _repository.RemoverAsync(command.Id, ct);

        LogVagaRemovida(command.Id);
    }

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information,
        Message = "Vaga {VagaId} removida com sucesso")]
    private partial void LogVagaRemovida(Guid vagaId);
}
