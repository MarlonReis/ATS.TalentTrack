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
        var connectionString = s["ConnectionString"]
            ?? throw new InvalidOperationException(
                "MongoDB:ConnectionString não está configurada. " +
                "Defina a variável de ambiente MongoDB__ConnectionString ou adicione-a ao appsettings.");

        return new MongoDbSettings
        {
            ConnectionString = connectionString,
            DatabaseName = s["DatabaseName"] ?? "AtsDb",
            MaxPoolSize = ParseInt(s["MaxConnectionPoolSize"] ?? s["MaxPoolSize"], 100)
        };
    }

    private static int ParseInt(string? value, int fallback)
        => int.TryParse(value, out var n) ? n : fallback;
}
