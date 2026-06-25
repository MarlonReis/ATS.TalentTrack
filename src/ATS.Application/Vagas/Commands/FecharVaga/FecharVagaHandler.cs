namespace ATS.Application.Vagas.Commands.FecharVaga;

using ATS.Application.Vagas.DTOs;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Repositories;

public sealed class FecharVagaHandler
{
    private readonly IVagaRepository _repository;

    public FecharVagaHandler(IVagaRepository repository)
    {
        _repository = repository;
    }

    public async Task<VagaDto> HandleAsync(
        FecharVagaCommand command,
        CancellationToken ct = default)
    {
        var vaga = await _repository.ObterPorIdAsync(command.Id, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        // Domínio lança DomainException se já estiver fechada
        vaga.Fechar();

        await _repository.AtualizarAsync(vaga, ct);

        return VagaDto.FromDomain(vaga);
    }
}
