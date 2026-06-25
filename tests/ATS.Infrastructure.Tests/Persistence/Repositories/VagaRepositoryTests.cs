using ATS.Domain.Vagas.Entities;
using ATS.Infrastructure.Persistence.Context;
using ATS.Infrastructure.Persistence.Repositories;
using MongoDB.Driver;
using Moq;

namespace ATS.Infrastructure.Tests.Persistence.Repositories;

public class VagaRepositoryTests
{
    private readonly Mock<IMongoCollection<Vaga>> _collectionMock;
    private readonly VagaRepository _repository;

    public VagaRepositoryTests()
    {
        _ = new MongoDbContext(new MongoDbSettings());

        var contextMock = new Mock<IMongoDbContext>(MockBehavior.Strict);
        _collectionMock = new Mock<IMongoCollection<Vaga>>(MockBehavior.Strict);

        contextMock
            .Setup(context => context.GetCollection<Vaga>("vagas"))
            .Returns(_collectionMock.Object);

        _repository = new VagaRepository(contextMock.Object);
    }

    [Fact]
    public void DeveObterCollectionVagasNoConstrutor()
    {
        var contextMock = new Mock<IMongoDbContext>(MockBehavior.Strict);
        var collectionMock = new Mock<IMongoCollection<Vaga>>(MockBehavior.Strict);

        contextMock
            .Setup(context => context.GetCollection<Vaga>("vagas"))
            .Returns(collectionMock.Object);

        _ = new VagaRepository(contextMock.Object);

        contextMock.Verify(context => context.GetCollection<Vaga>("vagas"), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsyncDeveBuscarPeloId()
    {
        var vaga = CriarVaga();
        FilterDefinition<Vaga>? filter = null;

        SetupFindAsync(new[] { vaga }, (f, _, _) => filter = f);

        var resultado = await _repository.ObterPorIdAsync(vaga.Id);

        Assert.Same(vaga, resultado);
        Assert.NotNull(filter);

        var renderedFilter = MongoRepositoryTestHelpers.Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("_id", element.Name);
        Assert.Equal(vaga.Id, element.Value.AsGuid);
    }

    [Fact]
    public async Task ListarAsyncDeveAplicarPaginacao()
    {
        var vagas = new[]
        {
            CriarVaga("Desenvolvedor Back-end"),
            CriarVaga("Tech Lead")
        };
        FindOptions<Vaga, Vaga>? options = null;
        FilterDefinition<Vaga>? filter = null;

        SetupFindAsync(
            vagas,
            (f, o, _) =>
            {
                filter = f;
                options = o;
            });

        var resultado = await _repository.ListarAsync(2, 15);

        Assert.Equal(vagas, resultado);
        Assert.NotNull(filter);
        Assert.Empty(MongoRepositoryTestHelpers.Render(filter));
        Assert.NotNull(options);
        Assert.Equal(15, options.Skip);
        Assert.Equal(15, options.Limit);
    }

    [Fact]
    public async Task ContarAsyncDeveContarTodosOsDocumentos()
    {
        FilterDefinition<Vaga>? filter = null;

        _collectionMock
            .Setup(collection => collection.CountDocumentsAsync(
                It.IsAny<FilterDefinition<Vaga>>(),
                It.IsAny<CountOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<Vaga>, CountOptions, CancellationToken>(
                (f, _, _) => filter = f)
            .ReturnsAsync(12);

        var total = await _repository.ContarAsync();

        Assert.Equal(12, total);
        Assert.NotNull(filter);
        Assert.Empty(MongoRepositoryTestHelpers.Render(filter));
    }

    [Fact]
    public async Task AdicionarAsyncDeveInserirVaga()
    {
        var vaga = CriarVaga();
        var ct = new CancellationTokenSource().Token;

        _collectionMock
            .Setup(collection => collection.InsertOneAsync(vaga, It.IsAny<InsertOneOptions>(), ct))
            .Returns(Task.CompletedTask);

        await _repository.AdicionarAsync(vaga, ct);

        _collectionMock.Verify(
            collection => collection.InsertOneAsync(vaga, It.IsAny<InsertOneOptions>(), ct),
            Times.Once);
    }

    [Fact]
    public async Task AtualizarAsyncDeveSubstituirVagaPeloId()
    {
        var vaga = CriarVaga();
        FilterDefinition<Vaga>? filter = null;

        _collectionMock
            .Setup(collection => collection.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Vaga>>(),
                vaga,
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<Vaga>, Vaga, ReplaceOptions, CancellationToken>(
                (f, _, _, _) => filter = f)
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

        await _repository.AtualizarAsync(vaga);

        Assert.NotNull(filter);

        var renderedFilter = MongoRepositoryTestHelpers.Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("_id", element.Name);
        Assert.Equal(vaga.Id, element.Value.AsGuid);
    }

    [Fact]
    public async Task RemoverAsyncDeveExcluirVagaPeloId()
    {
        var vagaId = Guid.Parse("77fc970b-2bea-4ef0-aa0b-164a699d0c57");
        FilterDefinition<Vaga>? filter = null;

        _collectionMock
            .Setup(collection => collection.DeleteOneAsync(
                It.IsAny<FilterDefinition<Vaga>>(),
                It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<Vaga>, CancellationToken>((f, _) => filter = f)
            .ReturnsAsync(new DeleteResult.Acknowledged(1));

        await _repository.RemoverAsync(vagaId);

        Assert.NotNull(filter);

        var renderedFilter = MongoRepositoryTestHelpers.Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("_id", element.Name);
        Assert.Equal(vagaId, element.Value.AsGuid);
    }

    private void SetupFindAsync(
        IReadOnlyCollection<Vaga> vagas,
        Action<FilterDefinition<Vaga>, FindOptions<Vaga, Vaga>, CancellationToken>? callback = null)
    {
        _collectionMock
            .Setup(collection => collection.FindAsync(
                It.IsAny<FilterDefinition<Vaga>>(),
                It.IsAny<FindOptions<Vaga, Vaga>>(),
                It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<Vaga>, FindOptions<Vaga, Vaga>, CancellationToken>(
                (filter, options, ct) => callback?.Invoke(filter, options, ct))
            .ReturnsAsync(MongoRepositoryTestHelpers.CriarCursor(vagas));
    }

    private static Vaga CriarVaga(string titulo = "Desenvolvedor Back-end") =>
        Vaga.Criar(titulo, "Descricao da vaga", "Requisitos da vaga", 12000);
}
