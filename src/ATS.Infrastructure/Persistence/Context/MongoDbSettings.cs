namespace ATS.Infrastructure.Persistence.Context;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "AtsDb";
    public int MaxPoolSize { get; set; } = 100;
}
