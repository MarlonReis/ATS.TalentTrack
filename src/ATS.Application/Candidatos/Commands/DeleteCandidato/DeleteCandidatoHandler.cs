namespace ATS.Application.Candidatos.Commands.DeleteCandidato;

using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Microsoft.Extensions.Logging;

public sealed partial class DeleteCandidatoHandler
{
    private readonly ICandidatoRepository _repository;
    private readonly ILogger<DeleteCandidatoHandler> _logger;

    public DeleteCandidatoHandler(
        ICandidatoRepository repository,
        ILogger<DeleteCandidatoHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(
        DeleteCandidatoCommand command,
        CancellationToken ct = default)
    {
        _ = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Candidato não encontrado.");

        await _repository.RemoverAsync(command.Id, ct);

        LogCandidatoRemovido(command.Id);
    }

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information,
        Message = "Candidato {CandidatoId} removido com sucesso")]
    private partial void LogCandidatoRemovido(Guid candidatoId);
}
