namespace ATS.Application.Candidaturas.Events;

using ATS.Domain.Candidaturas.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed partial class NotificarCandidatoReprovadoHandler
    : INotificationHandler<CandidaturaReprovadaEvent>
{
    private readonly ILogger<NotificarCandidatoReprovadoHandler> _logger;

    public NotificarCandidatoReprovadoHandler(ILogger<NotificarCandidatoReprovadoHandler> logger)
        => _logger = logger;

    public Task Handle(CandidaturaReprovadaEvent notification, CancellationToken cancellationToken)
    {
        LogCandidatoReprovado(notification.CandidaturaId, notification.CandidatoId, notification.VagaId);
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 4002, Level = LogLevel.Information,
        Message = "[Evento] Candidatura {CandidaturaId} reprovada — notificar candidato {CandidatoId} (vaga {VagaId})")]
    private partial void LogCandidatoReprovado(Guid candidaturaId, Guid candidatoId, Guid vagaId);
}
