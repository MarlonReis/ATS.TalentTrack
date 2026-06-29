namespace ATS.Infrastructure.Persistence.Repositories;

using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using ATS.Infrastructure.Persistence.Context;
using MongoDB.Driver;

public sealed class VagaRepository : IVagaRepository
{
    private readonly IMongoCollection<Vaga> _collection;

    public VagaRepository(IMongoDbContext context)
    {
        _collection = context.GetCollection<Vaga>("vagas");
    }

    public async Task<Vaga?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _collection.Find(vaga => vaga.Id == id).FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<Vaga>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken ct = default) =>
        await _collection.Find(FilterDefinition<Vaga>.Empty)
            .Skip((pagina - 1) * tamanhoPagina)
            .Limit(tamanhoPagina)
            .ToListAsync(ct);

    public async Task<IEnumerable<Vaga>> ListarComCursorAsync(
        Guid? afterId, int limite, CancellationToken ct = default)
    {
        var filter = afterId.HasValue
            ? Builders<Vaga>.Filter.Gt(v => v.Id, afterId.Value)
            : FilterDefinition<Vaga>.Empty;

        return await _collection.Find(filter)
            .Sort(Builders<Vaga>.Sort.Ascending(v => v.Id))
            .Limit(limite)
            .ToListAsync(ct);
    }

    public async Task AdicionarAsync(Vaga vaga, CancellationToken ct = default) =>
        await _collection.InsertOneAsync(vaga, cancellationToken: ct);

    public async Task AtualizarAsync(Vaga vaga, CancellationToken ct = default) =>
        await _collection.ReplaceOneAsync(
            item => item.Id == vaga.Id,
            vaga,
            cancellationToken: ct);

    public async Task RemoverAsync(Guid id, CancellationToken ct = default) =>
        await _collection.DeleteOneAsync(vaga => vaga.Id == id, ct);

    public async Task<long> ContarAsync(CancellationToken ct = default) =>
        await _collection.CountDocumentsAsync(FilterDefinition<Vaga>.Empty, cancellationToken: ct);
}
