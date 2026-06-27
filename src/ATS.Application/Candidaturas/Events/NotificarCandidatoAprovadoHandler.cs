namespace ATS.Application.Candidaturas.Events;

using ATS.Domain.Candidaturas.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed partial class NotificarCandidatoAprovadoHandler
    : INotificationHandler<CandidaturaAprovadaEvent>
{
    private readonly ILogger<NotificarCandidatoAprovadoHandler> _logger;

    public NotificarCandidatoAprovadoHandler(ILogger<NotificarCandidatoAprovadoHandler> logger)
        => _logger = logger;

    public Task Handle(CandidaturaAprovadaEvent notification, CancellationToken cancellationToken)
    {
        LogCandidatoAprovado(notification.CandidaturaId, notification.CandidatoId, notification.VagaId);
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 4001, Level = LogLevel.Information,
        Message = "[Evento] Candidatura {CandidaturaId} aprovada — notificar candidato {CandidatoId} (vaga {VagaId})")]
    private partial void LogCandidatoAprovado(Guid candidaturaId, Guid candidatoId, Guid vagaId);
}
