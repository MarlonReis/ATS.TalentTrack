namespace ATS.Infrastructure.Persistence.Repositories;

using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Infrastructure.Persistence.Context;
using MongoDB.Driver;

public class CandidatoRepository : ICandidatoRepository
{
    private const string _emailValueField = "email.value";

    private readonly IMongoCollection<Candidato> _collection;

    public CandidatoRepository(IMongoDbContext context)
    {
        _collection = context.GetCollection<Candidato>("candidatos");

        var indexModel = new CreateIndexModel<Candidato>(
            Builders<Candidato>.IndexKeys.Ascending(_emailValueField),
            new CreateIndexOptions { Unique = true });
        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task<Candidato?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _collection.Find(c => c.Id == id).FirstOrDefaultAsync(ct);

    public async Task<Candidato?> ObterPorEmailAsync(string email, CancellationToken ct = default) =>
        await _collection.Find(
            Builders<Candidato>.Filter.Eq(_emailValueField, email.ToLowerInvariant()))
            .FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<Candidato>> ListarAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default) =>
        await _collection.Find(FilterDefinition<Candidato>.Empty)
            .Skip((pagina - 1) * tamanhoPagina)
            .Limit(tamanhoPagina)
            .ToListAsync(ct);

    public async Task<long> ContarAsync(CancellationToken ct = default) =>
        await _collection.CountDocumentsAsync(FilterDefinition<Candidato>.Empty, cancellationToken: ct);

    public async Task AdicionarAsync(Candidato candidato, CancellationToken ct = default) =>
        await _collection.InsertOneAsync(candidato, cancellationToken: ct);

    public async Task AtualizarAsync(Candidato candidato, CancellationToken ct = default) =>
        await _collection.ReplaceOneAsync(
            c => c.Id == candidato.Id, candidato, cancellationToken: ct);

    public async Task RemoverAsync(Guid id, CancellationToken ct = default) =>
        await _collection.DeleteOneAsync(c => c.Id == id, ct);
}
