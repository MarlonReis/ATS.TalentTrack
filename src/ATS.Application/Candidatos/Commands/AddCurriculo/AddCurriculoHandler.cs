namespace ATS.Application.Candidatos.Commands.AddCurriculo;

using ATS.Application.Candidatos.DTOs;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;

public sealed class AddCurriculoHandler
{
    private readonly ICandidatoRepository _repository;

    public AddCurriculoHandler(ICandidatoRepository repository)
    {
        _repository = repository;
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

        return CandidatoDto.FromDomain(candidato);
    }
}
