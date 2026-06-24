namespace ATS.Application.Candidaturas.Commands.CandidatarSe;

using ATS.Application.Candidaturas.DTOs;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;

public class CandidatarSeHandler
{
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ICandidatoRepository _candidatoRepository;
    private readonly IVagaRepository _vagaRepository;

    public CandidatarSeHandler(
        ICandidaturaRepository candidaturaRepository,
        ICandidatoRepository candidatoRepository,
        IVagaRepository vagaRepository)
    {
        _candidaturaRepository = candidaturaRepository;
        _candidatoRepository = candidatoRepository;
        _vagaRepository = vagaRepository;
    }

    public async Task<CandidaturaDto> HandleAsync(
        CandidatarSeCommand command,
        CancellationToken ct = default)
    {
        // Valida existência do candidato
        var candidato = await _candidatoRepository.ObterPorIdAsync(command.CandidatoId, ct)
            ?? throw new DomainException("Candidato não encontrado.");

        // Valida existência e status da vaga
        var vaga = await _vagaRepository.ObterPorIdAsync(command.VagaId, ct)
            ?? throw new DomainException("Vaga não encontrada.");

        if (vaga.Status == StatusVaga.Fechada)
        {
            throw new DomainException("Não é possível se candidatar a uma vaga fechada.");
        }

        // Regra: candidato não pode se candidatar duas vezes à mesma vaga
        var jaCandidatou = await _candidaturaRepository.ExisteAsync(
            command.CandidatoId, command.VagaId, ct);

        if (jaCandidatou)
        {
            throw new DomainException("Candidato já se candidatou a esta vaga.");
        }

        // Cria candidatura (lógica e validação no agregado)
        var candidatura = Candidatura.Criar(command.CandidatoId, command.VagaId);
        await _candidaturaRepository.AdicionarAsync(candidatura, ct);

        return CandidaturaDto.FromDomain(candidatura, candidato.Nome, vaga.Titulo);
    }
}
