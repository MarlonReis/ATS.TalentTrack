using ATS.Domain.Candidaturas.Entities;
using ATS.Infrastructure.Persistence.Context;
using ATS.Infrastructure.Persistence.Repositories;
using MongoDB.Driver;
using Moq;

namespace ATS.Infrastructure.Tests.Persistence.Repositories;

public class CandidaturaRepositoryTests
{
    private readonly Mock<IMongoCollection<Candidatura>> _collectionMock;
    private readonly CandidaturaRepository _repository;

    public CandidaturaRepositoryTests()
    {
        _ = new MongoDbContext(new MongoDbSettings { ConnectionString = "mongodb://localhost:27017" });

        var contextMock = new Mock<IMongoDbContext>(MockBehavior.Strict);
        _collectionMock = new Mock<IMongoCollection<Candidatura>>(MockBehavior.Strict);

        contextMock
            .Setup(context => context.GetCollection<Candidatura>("candidaturas"))
            .Returns(_collectionMock.Object);

        _repository = new CandidaturaRepository(contextMock.Object);
    }

    [Fact]
    public void DeveBuscarCollectionCandidaturasNoConstrutor()
    {
        var contextMock = new Mock<IMongoDbContext>(MockBehavior.Strict);
        var collectionMock = new Mock<IMongoCollection<Candidatura>>(MockBehavior.Strict);

        contextMock
            .Setup(context => context.GetCollection<Candidatura>("candidaturas"))
            .Returns(collectionMock.Object);

        _ = new CandidaturaRepository(contextMock.Object);

        contextMock.Verify(context => context.GetCollection<Candidatura>("candidaturas"), Times.Once);
        // Índices são criados pelo MongoIndexInitializer (IHostedService), não no construtor
        collectionMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ObterPorIdAsyncDeveBuscarPeloId()
    {
        var candidatura = CriarCandidatura();
        FilterDefinition<Candidatura>? filter = null;

        SetupFindAsync(new[] { candidatura }, (f, _, _) => filter = f);

        var resultado = await _repository.ObterPorIdAsync(candidatura.Id);

        Assert.Same(candidatura, resultado);
        Assert.NotNull(filter);

        var renderedFilter = MongoRepositoryTestHelpers.Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("_id", element.Name);
        Assert.Equal(candidatura.Id, element.Value.AsGuid);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task ExisteAsyncDeveRetornarSeCandidaturaExiste(long total, bool esperado)
    {
        var candidatoId = Guid.Parse("b034343a-b15d-4efd-832f-23218cceab7d");
        var vagaId = Guid.Parse("d39db602-087e-460d-8df9-aae16ac9ced5");
        FilterDefinition<Candidatura>? filter = null;

        _collectionMock
            .Setup(collection => collection.CountDocumentsAsync(
                It.IsAny<FilterDefinition<Candidatura>>(),
                It.IsAny<CountOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<Candidatura>, CountOptions, CancellationToken>(
                (f, _, _) => filter = f)
            .ReturnsAsync(total);

        var existe = await _repository.ExisteAsync(candidatoId, vagaId);

        Assert.Equal(esperado, existe);
        Assert.NotNull(filter);

        var renderedFilter = MongoRepositoryTestHelpers.Render(filter);
        Assert.Equal(candidatoId, renderedFilter["candidatoId"].AsGuid);
        Assert.Equal(vagaId, renderedFilter["vagaId"].AsGuid);
    }

    [Fact]
    public async Task ListarPorVagaAsyncDeveBuscarPorVagaId()
    {
        var candidatura = CriarCandidatura();
        FilterDefinition<Candidatura>? filter = null;

        SetupFindAsync(new[] { candidatura }, (f, _, _) => filter = f);

        var resultado = await _repository.ListarPorVagaAsync(candidatura.VagaId);

        Assert.Equal(new[] { candidatura }, resultado);
        Assert.NotNull(filter);
        Assert.Equal(candidatura.VagaId, MongoRepositoryTestHelpers.Render(filter)["vagaId"].AsGuid);
    }

    [Fact]
    public async Task ListarPorCandidatoAsyncDeveBuscarPorCandidatoId()
    {
        var candidatura = CriarCandidatura();
        FilterDefinition<Candidatura>? filter = null;

        SetupFindAsync(new[] { candidatura }, (f, _, _) => filter = f);

        var resultado = await _repository.ListarPorCandidatoAsync(candidatura.CandidatoId);

        Assert.Equal(new[] { candidatura }, resultado);
        Assert.NotNull(filter);
        Assert.Equal(
            candidatura.CandidatoId,
            MongoRepositoryTestHelpers.Render(filter)["candidatoId"].AsGuid);
    }

    [Fact]
    public async Task AdicionarAsyncDeveInserirCandidatura()
    {
        var candidatura = CriarCandidatura();

        _collectionMock
            .Setup(collection => collection.InsertOneAsync(
                candidatura,
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _repository.AdicionarAsync(candidatura);

        _collectionMock.Verify(
            collection => collection.InsertOneAsync(
                candidatura,
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AtualizarAsyncDeveSubstituirCandidaturaPeloId()
    {
        var candidatura = CriarCandidatura();
        FilterDefinition<Candidatura>? filter = null;

        _collectionMock
            .Setup(collection => collection.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Candidatura>>(),
                candidatura,
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<Candidatura>, Candidatura, ReplaceOptions, CancellationToken>(
                (f, _, _, _) => filter = f)
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

        await _repository.AtualizarAsync(candidatura);

        Assert.NotNull(filter);

        var renderedFilter = MongoRepositoryTestHelpers.Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("_id", element.Name);
        Assert.Equal(candidatura.Id, element.Value.AsGuid);
    }

    private void SetupFindAsync(
        IReadOnlyCollection<Candidatura> candidaturas,
        Action<FilterDefinition<Candidatura>, FindOptions<Candidatura, Candidatura>, CancellationToken>? callback = null)
    {
        _collectionMock
            .Setup(collection => collection.FindAsync(
                It.IsAny<FilterDefinition<Candidatura>>(),
                It.IsAny<FindOptions<Candidatura, Candidatura>>(),
                It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<Candidatura>, FindOptions<Candidatura, Candidatura>, CancellationToken>(
                (filter, options, ct) => callback?.Invoke(filter, options, ct))
            .ReturnsAsync(MongoRepositoryTestHelpers.CriarCursor(candidaturas));
    }

    private static Candidatura CriarCandidatura() =>
        Candidatura.Criar(
            Guid.Parse("25495767-c4b9-4a99-98a7-16335a9b8fa3"),
            Guid.Parse("ec085f7d-b924-42ef-9e62-30e5a42bd07d"));
}
