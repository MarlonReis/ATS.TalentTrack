namespace ATS.Application.Candidaturas.Events;

using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Vagas.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed partial class CancelarCandidaturasPendentesHandler
    : INotificationHandler<VagaFechadaEvent>
{
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ILogger<CancelarCandidaturasPendentesHandler> _logger;

    public CancelarCandidaturasPendentesHandler(
        ICandidaturaRepository candidaturaRepository,
        ILogger<CancelarCandidaturasPendentesHandler> logger)
    {
        _candidaturaRepository = candidaturaRepository;
        _logger = logger;
    }

    public async Task Handle(VagaFechadaEvent notification, CancellationToken cancellationToken)
    {
        var pendentes = await _candidaturaRepository.ListarPorVagaAsync(
            notification.VagaId, cancellationToken);

        var emAnalise = pendentes
            .Where(c => c.Status == StatusCandidatura.EmAnalise)
            .ToList();

        foreach (var candidatura in emAnalise)
        {
            candidatura.Cancelar();
            await _candidaturaRepository.AtualizarAsync(candidatura, cancellationToken);
        }

        if (emAnalise.Count > 0)
        {
            LogCandidaturasCanceladas(notification.VagaId, emAnalise.Count);
        }
    }

    [LoggerMessage(EventId = 4030, Level = LogLevel.Information,
        Message = "[Evento] {Total} candidatura(s) cancelada(s) automaticamente ao fechar a vaga {VagaId}")]
    private partial void LogCandidaturasCanceladas(Guid vagaId, int total);
}
