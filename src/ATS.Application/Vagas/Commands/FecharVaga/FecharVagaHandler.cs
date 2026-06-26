namespace ATS.Application.Vagas.Commands.FecharVaga;

using ATS.Application.Vagas.DTOs;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging;

public sealed partial class FecharVagaHandler
{
    private readonly IVagaRepository _repository;
    private readonly ILogger<FecharVagaHandler> _logger;

    public FecharVagaHandler(
        IVagaRepository repository,
        ILogger<FecharVagaHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<VagaDto> HandleAsync(
        FecharVagaCommand command,
        CancellationToken ct = default)
    {
        var vaga = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        vaga.Fechar();

        await _repository.AtualizarAsync(vaga, ct);

        LogVagaFechada(vaga.Id);

        return VagaDto.FromDomain(vaga);
    }

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information,
        Message = "Vaga {VagaId} fechada com sucesso")]
    private partial void LogVagaFechada(Guid vagaId);
}
