namespace ATS.Application.Candidatos.Commands.AddCurriculo;

using ATS.Application.Candidatos.DTOs;
using ATS.Application.Common.Events;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Microsoft.Extensions.Logging;

public sealed partial class AddCurriculoHandler
{
    private readonly ICandidatoRepository _repository;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogger<AddCurriculoHandler> _logger;

    public AddCurriculoHandler(
        ICandidatoRepository repository,
        IDomainEventDispatcher dispatcher,
        ILogger<AddCurriculoHandler> logger)
    {
        _repository = repository;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task<CandidatoDto> HandleAsync(
        AddCurriculoCommand command,
        CancellationToken ct = default)
    {
        var candidato = await _repository.ObterPorIdAsync(command.CandidatoId, ct)
            ?? throw new DomainException("Candidato não encontrado.");

        candidato.AdicionarCurriculo(
            command.NomeArquivo,
            command.ContentType,
            command.UrlOuBase64);

        await _repository.AtualizarAsync(candidato, ct);
        await _dispatcher.DispatchAndClearAsync(candidato, ct);

        LogCurriculoAdicionado(candidato.Id, command.NomeArquivo);

        return CandidatoDto.FromDomain(candidato);
    }

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information,
        Message = "Currículo '{NomeArquivo}' adicionado ao candidato {CandidatoId}")]
    private partial void LogCurriculoAdicionado(Guid candidatoId, string nomeArquivo);
}
