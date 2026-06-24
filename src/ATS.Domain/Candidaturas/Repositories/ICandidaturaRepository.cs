namespace ATS.Domain.Candidaturas.Repositories;

using ATS.Domain.Candidaturas.Entities;

public interface ICandidaturaRepository
{
    Task<Candidatura?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteAsync(Guid candidatoId, Guid vagaId, CancellationToken ct = default);
    Task<IEnumerable<Candidatura>> ListarPorVagaAsync(Guid vagaId, CancellationToken ct = default);
    Task<IEnumerable<Candidatura>> ListarPorCandidatoAsync(Guid candidatoId, CancellationToken ct = default);
    Task AdicionarAsync(Candidatura candidatura, CancellationToken ct = default);
    Task AtualizarAsync(Candidatura candidatura, CancellationToken ct = default);
}
