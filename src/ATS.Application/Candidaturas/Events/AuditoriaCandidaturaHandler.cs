namespace ATS.Application.Candidaturas.Events;

using ATS.Domain.Candidaturas.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed partial class AuditoriaCandidaturaHandler
    : INotificationHandler<CandidaturaRealizadaEvent>,
      INotificationHandler<CandidaturaAprovadaEvent>,
      INotificationHandler<CandidaturaReprovadaEvent>,
      INotificationHandler<CandidaturaCanceladaEvent>
{
    private readonly ILogger<AuditoriaCandidaturaHandler> _logger;

    public AuditoriaCandidaturaHandler(ILogger<AuditoriaCandidaturaHandler> logger)
        => _logger = logger;

    public Task Handle(CandidaturaRealizadaEvent notification, CancellationToken cancellationToken)
    {
        LogAuditoria(notification.CandidaturaId, "Realizada");
        return Task.CompletedTask;
    }

    public Task Handle(CandidaturaAprovadaEvent notification, CancellationToken cancellationToken)
    {
        LogAuditoria(notification.CandidaturaId, "Aprovada");
        return Task.CompletedTask;
    }

    public Task Handle(CandidaturaReprovadaEvent notification, CancellationToken cancellationToken)
    {
        LogAuditoria(notification.CandidaturaId, "Reprovada");
        return Task.CompletedTask;
    }

    public Task Handle(CandidaturaCanceladaEvent notification, CancellationToken cancellationToken)
    {
        LogAuditoria(notification.CandidaturaId, "Cancelada");
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 4010, Level = LogLevel.Information,
        Message = "[Auditoria] Candidatura {CandidaturaId} — status: {Status}")]
    private partial void LogAuditoria(Guid candidaturaId, string status);
}
