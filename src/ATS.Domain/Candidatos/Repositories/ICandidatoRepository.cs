namespace ATS.Domain.Candidatos.Repositories;

using ATS.Domain.Candidatos.Entities;

public interface ICandidatoRepository
{
    Task<Candidato?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Candidato?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<Candidato>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task<IEnumerable<Candidato>> ListarComCursorAsync(Guid? afterId, int limite, CancellationToken ct = default);
    Task<long> ContarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Candidato candidato, CancellationToken ct = default);
    Task AtualizarAsync(Candidato candidato, CancellationToken ct = default);
    Task RemoverAsync(Guid id, CancellationToken ct = default);
}
