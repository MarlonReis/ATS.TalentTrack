namespace ATS.Application.Candidatos.Commands.UpdateCandidato;

using ATS.Application.Candidatos.DTOs;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;

public sealed class UpdateCandidatoHandler
{
    private readonly ICandidatoRepository _repository;

    public UpdateCandidatoHandler(ICandidatoRepository repository)
    {
        _repository = repository;
    }

    public async Task<CandidatoDto> HandleAsync(
        UpdateCandidatoCommand command,
        CancellationToken ct = default)
    {
        var candidato = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Candidato não encontrado.");

        var comMesmoEmail = await _repository.ObterPorEmailAsync(command.Email, ct);
        if (comMesmoEmail is not null && comMesmoEmail.Id != command.Id)
        {
            throw new DomainException(
                $"Já existe outro candidato com o e-mail '{command.Email}'.");
        }

        candidato.AtualizarContato(command.Nome, command.Email, command.Telefone);

        await _repository.AtualizarAsync(candidato, ct);

        return CandidatoDto.FromDomain(candidato);
    }
}
