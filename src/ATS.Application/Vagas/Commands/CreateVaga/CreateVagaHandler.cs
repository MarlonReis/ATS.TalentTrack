namespace ATS.Application.Vagas.Commands.CreateVaga;

using ATS.Application.Vagas.DTOs;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;

public sealed class CreateVagaHandler
{
    private readonly IVagaRepository _repository;

    public CreateVagaHandler(IVagaRepository repository)
    {
        _repository = repository;
    }

    public async Task<VagaDto> HandleAsync(
        CreateVagaCommand command,
        CancellationToken ct = default)
    {
        var vaga = Vaga.Criar(
              command.Titulo,
              command.Descricao,
              command.Requisitos ?? string.Empty,
              command.Salario);

        await _repository.AdicionarAsync(vaga, ct);

        return VagaDto.FromDomain(vaga);
    }
}
