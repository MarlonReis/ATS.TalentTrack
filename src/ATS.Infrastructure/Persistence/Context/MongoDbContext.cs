namespace ATS.Infrastructure.Persistence.Context;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

public sealed class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        ConventionRegistry.Register("ATS.Conventions", new ConventionPack
        {
            new CamelCaseElementNameConvention(),    // nomes em camelCase no MongoDB
            new IgnoreIfNullConvention(true),        // não salva campos null
            new IgnoreExtraElementsConvention(true), // tolerante a campos extras no documento
            new EnumRepresentationConvention(BsonType.String) // enum como "Aberta", não 1
        }, _ => true);
    }

    public MongoDbContext(MongoDbSettings settings)
    {
        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        clientSettings.MaxConnectionPoolSize = settings.MaxPoolSize;

        _database = new MongoClient(clientSettings).GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        _database.GetCollection<T>(name);
}
