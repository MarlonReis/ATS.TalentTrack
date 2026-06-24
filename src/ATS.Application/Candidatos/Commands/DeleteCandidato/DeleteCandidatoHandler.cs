namespace ATS.Application.Candidatos.Commands.DeleteCandidato;

using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;

public sealed class DeleteCandidatoHandler
{
    private readonly ICandidatoRepository _repository;

    public DeleteCandidatoHandler(ICandidatoRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        DeleteCandidatoCommand command,
        CancellationToken ct = default)
    {
        _ = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Candidato não encontrado.");

        await _repository.RemoverAsync(command.Id, ct);
    }
}
