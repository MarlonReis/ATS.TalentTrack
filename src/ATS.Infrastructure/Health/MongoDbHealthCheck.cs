namespace ATS.Infrastructure.Health;

using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;

internal sealed class MongoDbHealthCheck : IHealthCheck
{
    private static readonly BsonDocument _pingCommand = new("ping", 1);

    private readonly IMongoDbContext _context;

    public MongoDbHealthCheck(IMongoDbContext context) => _context = context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var db = _context.GetCollection<BsonDocument>("_health").Database;
            await db.RunCommandAsync<BsonDocument>(_pingCommand, cancellationToken: ct);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
