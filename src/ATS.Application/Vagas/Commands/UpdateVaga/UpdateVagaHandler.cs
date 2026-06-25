namespace ATS.Application.Vagas.Commands.UpdateVaga;

using ATS.Application.Vagas.DTOs;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;

public sealed class UpdateVagaHandler
{
    private readonly IVagaRepository _repository;

    public UpdateVagaHandler(IVagaRepository repository)
    {
        _repository = repository;
    }

    public async Task<VagaDto> HandleAsync(
        UpdateVagaCommand command,
        CancellationToken ct = default)
    {
        var vaga = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        vaga.Atualizar(
            command.Titulo,
            command.Descricao,
            command.Requisitos ?? string.Empty,
            command.Salario);

        await _repository.AtualizarAsync(vaga, ct);

        return VagaDto.FromDomain(vaga);
    }
}
