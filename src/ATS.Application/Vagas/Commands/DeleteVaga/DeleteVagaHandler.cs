namespace ATS.Application.Vagas.Commands.DeleteVaga;

using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;

public sealed class DeleteVagaHandler
{
    private readonly IVagaRepository _repository;

    public DeleteVagaHandler(IVagaRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        DeleteVagaCommand command,
        CancellationToken ct = default)
    {
        _ = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        await _repository.RemoverAsync(command.Id, ct);
    }
}
