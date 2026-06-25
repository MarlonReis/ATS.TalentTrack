namespace ATS.Domain.Vagas.Repositories;

using ATS.Domain.Vagas.Entities;

public interface IVagaRepository
{
    Task<Vaga?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Vaga>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task AdicionarAsync(Vaga vaga, CancellationToken ct = default);
    Task AtualizarAsync(Vaga vaga, CancellationToken ct = default);
    Task RemoverAsync(Guid id, CancellationToken ct = default);
    Task<long> ContarAsync(CancellationToken ct = default);
}
