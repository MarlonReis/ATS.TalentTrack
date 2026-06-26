namespace ATS.Infrastructure.Persistence.Context;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "AtsDb";
    public int MaxPoolSize { get; set; } = 100;
}
