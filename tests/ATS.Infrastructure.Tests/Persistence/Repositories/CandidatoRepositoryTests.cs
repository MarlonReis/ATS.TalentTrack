using ATS.Domain.Candidatos.Entities;
using ATS.Infrastructure.Persistence.Context;
using ATS.Infrastructure.Persistence.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;

namespace ATS.Infrastructure.Tests.Persistence.Repositories;

public class CandidatoRepositoryTests
{
    private readonly Mock<IMongoDbContext> _contextMock;
    private readonly Mock<IMongoCollection<Candidato>> _collectionMock;
    private readonly Mock<IMongoIndexManager<Candidato>> _indexManagerMock;
    private readonly CandidatoRepository _repository;

    public CandidatoRepositoryTests()
    {
        _ = new MongoDbContext(new MongoDbSettings());

        _contextMock = new Mock<IMongoDbContext>(MockBehavior.Strict);
        _collectionMock = new Mock<IMongoCollection<Candidato>>(MockBehavior.Strict);
        _indexManagerMock = new Mock<IMongoIndexManager<Candidato>>(MockBehavior.Strict);

        _collectionMock
            .SetupGet(c => c.Indexes)
            .Returns(_indexManagerMock.Object);

        _contextMock
            .Setup(c => c.GetCollection<Candidato>("candidatos"))
            .Returns(_collectionMock.Object);

        _indexManagerMock
            .Setup(i => i.CreateOne(
                It.IsAny<CreateIndexModel<Candidato>>(),
                It.IsAny<CreateOneIndexOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns("email_1");

        _repository = new CandidatoRepository(_contextMock.Object);
    }

    [Fact]
    public void DeveObterCollectionCandidatosECriarIndiceUnicoPorEmailNoConstrutor()
    {
        CreateIndexModel<Candidato>? indexModel = null;
        var contextMock = new Mock<IMongoDbContext>(MockBehavior.Strict);
        var collectionMock = new Mock<IMongoCollection<Candidato>>(MockBehavior.Strict);
        var indexManagerMock = new Mock<IMongoIndexManager<Candidato>>(MockBehavior.Strict);

        collectionMock
            .SetupGet(c => c.Indexes)
            .Returns(indexManagerMock.Object);

        contextMock
            .Setup(c => c.GetCollection<Candidato>("candidatos"))
            .Returns(collectionMock.Object);

        indexManagerMock
            .Setup(i => i.CreateOne(
                It.IsAny<CreateIndexModel<Candidato>>(),
                It.IsAny<CreateOneIndexOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<CreateIndexModel<Candidato>, CreateOneIndexOptions, CancellationToken>(
                (model, _, _) => indexModel = model)
            .Returns("email_1");

        _ = new CandidatoRepository(contextMock.Object);

        contextMock.Verify(c => c.GetCollection<Candidato>("candidatos"), Times.Once);
        indexManagerMock.Verify(
            i => i.CreateOne(
                It.IsAny<CreateIndexModel<Candidato>>(),
                It.IsAny<CreateOneIndexOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(indexModel);
        Assert.True(indexModel.Options.Unique);

        var indexKeys = Render(indexModel.Keys);
        var key = Assert.Single(indexKeys.Elements);
        Assert.Equal("email.value", key.Name.ToLowerInvariant());
        Assert.Equal(1, key.Value.AsInt32);
    }

    [Fact]
    public async Task ObterPorIdAsyncDeveBuscarPeloIdERetornarPrimeiroCandidato()
    {
        var candidato = CriarCandidato();
        var ct = new CancellationTokenSource().Token;
        FilterDefinition<Candidato>? filter = null;

        SetupFindAsync(new[] { candidato }, (f, _, _) => filter = f);

        var resultado = await _repository.ObterPorIdAsync(candidato.Id, ct);

        Assert.Same(candidato, resultado);
        Assert.NotNull(filter);

        var renderedFilter = Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("_id", element.Name);
        Assert.Equal(candidato.Id, element.Value.AsGuid);
    }

    [Theory]
    [InlineData("MARIA.SILVA@EXAMPLE.COM", "maria.silva@example.com")]
    [InlineData("joao@example.com", "joao@example.com")]
    public async Task ObterPorEmailAsyncDeveBuscarPorEmailEmMinusculo(
        string email,
        string emailEsperadoNoFiltro)
    {
        var candidato = CriarCandidato(email: emailEsperadoNoFiltro);
        FilterDefinition<Candidato>? filter = null;

        SetupFindAsync(new[] { candidato }, (f, _, _) => filter = f);

        var resultado = await _repository.ObterPorEmailAsync(email);

        Assert.Same(candidato, resultado);
        Assert.NotNull(filter);

        var renderedFilter = Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("email.value", element.Name.ToLowerInvariant());
        Assert.Equal(emailEsperadoNoFiltro, element.Value.AsString);
    }

    [Fact]
    public async Task ListarAsyncDeveAplicarPaginacaoERetornarCandidatos()
    {
        var candidatos = new[]
        {
            CriarCandidato("Maria Silva", "maria@example.com"),
            CriarCandidato("Joao Souza", "joao@example.com")
        };
        var ct = new CancellationTokenSource().Token;
        FilterDefinition<Candidato>? filter = null;
        FindOptions<Candidato, Candidato>? options = null;
        CancellationToken cancellationTokenRecebido = default;

        SetupFindAsync(
            candidatos,
            (f, o, token) =>
            {
                filter = f;
                options = o;
                cancellationTokenRecebido = token;
            });

        var resultado = await _repository.ListarAsync(3, 10, ct);

        Assert.Equal(candidatos, resultado);
        Assert.NotNull(filter);
        Assert.Empty(Render(filter));
        Assert.NotNull(options);
        Assert.Equal(20, options.Skip);
        Assert.Equal(10, options.Limit);
        Assert.Equal(ct, cancellationTokenRecebido);
    }

    [Fact]
    public async Task ContarAsyncDeveContarTodosOsCandidatos()
    {
        var ct = new CancellationTokenSource().Token;
        FilterDefinition<Candidato>? filter = null;

        _collectionMock
            .Setup(c => c.CountDocumentsAsync(
                It.IsAny<FilterDefinition<Candidato>>(),
                It.IsAny<CountOptions>(),
                ct))
            .Callback<FilterDefinition<Candidato>, CountOptions, CancellationToken>(
                (f, _, _) => filter = f)
            .ReturnsAsync(42);

        var total = await _repository.ContarAsync(ct);

        Assert.Equal(42, total);
        Assert.NotNull(filter);
        Assert.Empty(Render(filter));
    }

    [Fact]
    public async Task AdicionarAsyncDeveInserirCandidato()
    {
        var candidato = CriarCandidato();
        var ct = new CancellationTokenSource().Token;

        _collectionMock
            .Setup(c => c.InsertOneAsync(candidato, It.IsAny<InsertOneOptions>(), ct))
            .Returns(Task.CompletedTask);

        await _repository.AdicionarAsync(candidato, ct);

        _collectionMock.Verify(
            c => c.InsertOneAsync(candidato, It.IsAny<InsertOneOptions>(), ct),
            Times.Once);
    }

    [Fact]
    public async Task AtualizarAsyncDeveSubstituirCandidatoPeloId()
    {
        var candidato = CriarCandidato();
        var ct = new CancellationTokenSource().Token;
        FilterDefinition<Candidato>? filter = null;

        _collectionMock
            .Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Candidato>>(),
                candidato,
                It.IsAny<ReplaceOptions>(),
                ct))
            .Callback<FilterDefinition<Candidato>, Candidato, ReplaceOptions, CancellationToken>(
                (f, _, _, _) => filter = f)
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

        await _repository.AtualizarAsync(candidato, ct);

        Assert.NotNull(filter);

        var renderedFilter = Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("_id", element.Name);
        Assert.Equal(candidato.Id, element.Value.AsGuid);
    }

    [Fact]
    public async Task RemoverAsyncDeveExcluirCandidatoPeloId()
    {
        var candidatoId = Guid.Parse("df388f5e-c8b3-4a42-8598-3730166a1ac8");
        var ct = new CancellationTokenSource().Token;
        FilterDefinition<Candidato>? filter = null;

        _collectionMock
            .Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Candidato>>(),
                ct))
            .Callback<FilterDefinition<Candidato>, CancellationToken>(
                (f, _) => filter = f)
            .ReturnsAsync(new DeleteResult.Acknowledged(1));

        await _repository.RemoverAsync(candidatoId, ct);

        Assert.NotNull(filter);

        var renderedFilter = Render(filter);
        var element = Assert.Single(renderedFilter.Elements);
        Assert.Equal("_id", element.Name);
        Assert.Equal(candidatoId, element.Value.AsGuid);
    }

    private void SetupFindAsync(
        IReadOnlyCollection<Candidato> candidatos,
        Action<FilterDefinition<Candidato>, FindOptions<Candidato, Candidato>, CancellationToken>? callback = null)
    {
        _collectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Candidato>>(),
                It.IsAny<FindOptions<Candidato, Candidato>>(),
                It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<Candidato>, FindOptions<Candidato, Candidato>, CancellationToken>(
                (filter, options, ct) => callback?.Invoke(filter, options, ct))
            .ReturnsAsync(CriarCursor(candidatos));
    }

    private static IAsyncCursor<T> CriarCursor<T>(IEnumerable<T> documentos)
    {
        var cursorMock = new Mock<IAsyncCursor<T>>();
        var lote = documentos.ToList();

        cursorMock
            .SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        cursorMock
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorMock
            .SetupGet(c => c.Current)
            .Returns(lote);

        return cursorMock.Object;
    }

    private static Candidato CriarCandidato(
        string nome = "Maria Silva",
        string email = "maria.silva@example.com",
        string telefone = "11999998888")
        => Candidato.Criar(nome, email, telefone);

    private static BsonDocument Render(FilterDefinition<Candidato> filter)
        => filter.Render(CriarRenderArgs());

    private static BsonDocument Render(IndexKeysDefinition<Candidato> keys)
        => keys.Render(CriarRenderArgs());

    private static RenderArgs<Candidato> CriarRenderArgs()
        => new(
            BsonSerializer.SerializerRegistry.GetSerializer<Candidato>(),
            BsonSerializer.SerializerRegistry);
}
