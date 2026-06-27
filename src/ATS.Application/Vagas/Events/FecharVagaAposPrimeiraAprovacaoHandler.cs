namespace ATS.Application.Vagas.Events;

using ATS.Domain.Candidaturas.Events;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed partial class FecharVagaAposPrimeiraAprovacaoHandler
    : INotificationHandler<CandidaturaAprovadaEvent>
{
    private readonly IVagaRepository _vagaRepository;
    private readonly ILogger<FecharVagaAposPrimeiraAprovacaoHandler> _logger;

    public FecharVagaAposPrimeiraAprovacaoHandler(
        IVagaRepository vagaRepository,
        ILogger<FecharVagaAposPrimeiraAprovacaoHandler> logger)
    {
        _vagaRepository = vagaRepository;
        _logger = logger;
    }

    public async Task Handle(CandidaturaAprovadaEvent notification, CancellationToken cancellationToken)
    {
        var vaga = await _vagaRepository.ObterPorIdAsync(notification.VagaId, cancellationToken);
        if (vaga is null)
        {
            LogVagaNaoEncontrada(notification.VagaId);
            return;
        }

        try
        {
            vaga.Fechar();
            await _vagaRepository.AtualizarAsync(vaga, cancellationToken);
            LogVagaFechadaAutomaticamente(vaga.Id, notification.CandidaturaId);
        }
        catch (DomainException)
        {
            // Vaga já estava fechada — nenhuma ação necessária
        }
    }

    [LoggerMessage(EventId = 4020, Level = LogLevel.Information,
        Message = "[Evento] Vaga {VagaId} fechada automaticamente após aprovação da candidatura {CandidaturaId}")]
    private partial void LogVagaFechadaAutomaticamente(Guid vagaId, Guid candidaturaId);

    [LoggerMessage(EventId = 4021, Level = LogLevel.Warning,
        Message = "[Evento] Vaga {VagaId} não encontrada ao tentar fechar após aprovação")]
    private partial void LogVagaNaoEncontrada(Guid vagaId);
}
