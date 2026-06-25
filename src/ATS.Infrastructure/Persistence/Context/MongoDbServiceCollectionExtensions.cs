namespace ATS.Infrastructure.Persistence.Context;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class MongoDbServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = FromConfiguration(configuration);
        return services.AddMongoDb(settings);
    }

    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        MongoDbSettings settings)
    {
        services.AddSingleton<IMongoDbContext>(new MongoDbContext(settings));
        return services;
    }

    private static MongoDbSettings FromConfiguration(IConfiguration cfg)
    {
        var s = cfg.GetSection("MongoDB");
        return new MongoDbSettings
        {
            ConnectionString = s["ConnectionString"] ?? "mongodb://localhost:27017",
            DatabaseName = s["DatabaseName"] ?? "AtsDb",
            MaxPoolSize = ParseInt(s["MaxConnectionPoolSize"], 100)
        };
    }

    private static int ParseInt(string? value, int fallback)
        => int.TryParse(value, out var n) ? n : fallback;
}
