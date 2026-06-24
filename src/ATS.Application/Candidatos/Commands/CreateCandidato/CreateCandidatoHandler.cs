namespace ATS.Application.Candidatos.Commands.CreateCandidato;

using ATS.Application.Candidatos.DTOs;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;

public class CreateCandidatoHandler
{
    private readonly ICandidatoRepository _repository;

    public CreateCandidatoHandler(ICandidatoRepository repository)
    {
        _repository = repository;
    }

    public async Task<CandidatoDto> HandleAsync(
        CreateCandidatoCommand command,
        CancellationToken ct = default)
    {
        var existente = await _repository.ObterPorEmailAsync(command.Email, ct);
        if (existente is not null)
        {
            throw new DomainException($"Já existe um candidato com o e-mail '{command.Email}'.");
        }

        var candidato = Candidato.Criar(command.Nome, command.Email, command.Telefone);

        await _repository.AdicionarAsync(candidato, ct);

        return CandidatoDto.FromDomain(candidato);
    }
}
