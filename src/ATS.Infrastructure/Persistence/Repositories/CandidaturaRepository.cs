namespace ATS.Infrastructure.Persistence.Repositories;

using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Infrastructure.Persistence.Context;
using MongoDB.Driver;

public sealed class CandidaturaRepository : ICandidaturaRepository
{
    private const string _candidatoIdField = "candidatoId";
    private const string _vagaIdField = "vagaId";

    private readonly IMongoCollection<Candidatura> _collection;

    public CandidaturaRepository(IMongoDbContext context)
    {
        _collection = context.GetCollection<Candidatura>("candidaturas");

        var indexModel = new CreateIndexModel<Candidatura>(
            Builders<Candidatura>.IndexKeys
                .Ascending(_candidatoIdField)
                .Ascending(_vagaIdField),
            new CreateIndexOptions { Unique = true });

        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task<Candidatura?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _collection.Find(candidatura => candidatura.Id == id).FirstOrDefaultAsync(ct);

    public async Task<bool> ExisteAsync(
        Guid candidatoId,
        Guid vagaId,
        CancellationToken ct = default)
    {
        var filter = CriarFiltroCandidatoVaga(candidatoId, vagaId);
        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: ct);

        return count > 0;
    }

    public async Task<IEnumerable<Candidatura>> ListarPorVagaAsync(
        Guid vagaId,
        CancellationToken ct = default) =>
        await _collection.Find(Builders<Candidatura>.Filter.Eq(_vagaIdField, vagaId))
            .ToListAsync(ct);

    public async Task<IEnumerable<Candidatura>> ListarPorCandidatoAsync(
        Guid candidatoId,
        CancellationToken ct = default) =>
        await _collection.Find(Builders<Candidatura>.Filter.Eq(_candidatoIdField, candidatoId))
            .ToListAsync(ct);

    public async Task AdicionarAsync(Candidatura candidatura, CancellationToken ct = default) =>
        await _collection.InsertOneAsync(candidatura, cancellationToken: ct);

    public async Task AtualizarAsync(Candidatura candidatura, CancellationToken ct = default) =>
        await _collection.ReplaceOneAsync(
            item => item.Id == candidatura.Id,
            candidatura,
            cancellationToken: ct);

    private static FilterDefinition<Candidatura> CriarFiltroCandidatoVaga(
        Guid candidatoId,
        Guid vagaId) =>
        Builders<Candidatura>.Filter.And(
            Builders<Candidatura>.Filter.Eq(_candidatoIdField, candidatoId),
            Builders<Candidatura>.Filter.Eq(_vagaIdField, vagaId));
}
