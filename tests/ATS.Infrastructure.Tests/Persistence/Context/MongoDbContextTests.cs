using ATS.Domain.Vagas.Enums;
using ATS.Infrastructure.Persistence.Context;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ATS.Infrastructure.Tests.Persistence.Context;

public class MongoDbContextTests
{
    [Theory]
    [InlineData("AtsDbTest", "vagas")]
    [InlineData("AtsDbHomologacao", "candidatos")]
    public void DeveRetornarCollectionComDatabaseENameConfigurados(
        string databaseName,
        string collectionName)
    {
        var context = new MongoDbContext(CriarSettings(databaseName));

        var collection = context.GetCollection<DocumentoTeste>(collectionName);

        Assert.Equal(databaseName, collection.Database.DatabaseNamespace.DatabaseName);
        Assert.Equal(collectionName, collection.CollectionNamespace.CollectionName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(250)]
    public void DeveConfigurarMaxPoolSizeNoMongoClient(int maxPoolSize)
    {
        var context = new MongoDbContext(CriarSettings(maxPoolSize: maxPoolSize));

        var collection = context.GetCollection<DocumentoTeste>("vagas");

        Assert.Equal(maxPoolSize, collection.Database.Client.Settings.MaxConnectionPoolSize);
    }

    [Fact]
    public void DeveRegistrarConvencoesBsonAoCriarContexto()
    {
        _ = new MongoDbContext(CriarSettings());

        var documento = new DocumentoTeste
        {
            Id = Guid.Parse("22f9ea4f-f148-4a49-a3f7-ff4f75d3e77a"),
            NomeCompleto = "Maria Silva",
            Status = StatusVaga.Aberta,
            CampoNulo = null
        };

        var bson = documento.ToBsonDocument();

        Assert.True(bson.Contains("_id"));
        Assert.True(bson.Contains("nomeCompleto"));
        Assert.True(bson.Contains("status"));
        Assert.False(bson.Contains("Id"));
        Assert.False(bson.Contains("NomeCompleto"));
        Assert.False(bson.Contains("campoNulo"));
        Assert.Equal("Aberta", bson["status"].AsString);
        Assert.Equal(BsonType.Binary, bson["_id"].BsonType);
        Assert.Equal(BsonBinarySubType.UuidStandard, bson["_id"].AsBsonBinaryData.SubType);
    }

    [Fact]
    public void DeveIgnorarCamposExtrasAoDesserializarDocumento()
    {
        _ = new MongoDbContext(CriarSettings());

        var bson = new BsonDocument
        {
            ["_id"] = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard),
            ["nomeCompleto"] = "Maria Silva",
            ["status"] = "Aberta",
            ["campoExtra"] = "valor ignorado"
        };

        var documento = BsonSerializer.Deserialize<DocumentoTeste>(bson);

        Assert.Equal("Maria Silva", documento.NomeCompleto);
        Assert.Equal(StatusVaga.Aberta, documento.Status);
    }

    private static MongoDbSettings CriarSettings(
        string databaseName = "AtsDbTests",
        int maxPoolSize = 50)
        => new()
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = databaseName,
            MaxPoolSize = maxPoolSize
        };

    private sealed class DocumentoTeste
    {
        public Guid Id { get; init; }
        public string? NomeCompleto { get; init; }
        public StatusVaga Status { get; init; }
        public string? CampoNulo { get; init; }
    }
}
