using ATS.Infrastructure.Health;
using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace ATS.Infrastructure.Tests.Health;

public class MongoDbHealthCheckTests
{
    private readonly Mock<IMongoDbContext> _contextMock = new();
    private readonly Mock<IMongoDatabase> _databaseMock = new();
    private readonly Mock<IMongoCollection<BsonDocument>> _collectionMock = new();

    public MongoDbHealthCheckTests()
    {
        _contextMock
            .Setup(c => c.GetCollection<BsonDocument>("_health"))
            .Returns(_collectionMock.Object);

        _collectionMock
            .SetupGet(c => c.Database)
            .Returns(_databaseMock.Object);
    }

    private static HealthCheckContext CriarContexto() =>
        new()
        {
            Registration = new HealthCheckRegistration(
                "mongodb",
                Mock.Of<IHealthCheck>(),
                HealthStatus.Unhealthy,
                tags: null)
        };

    [Fact]
    public async Task DeveRetornarSaudavelQuandoPingMongoDbRetornarSucesso()
    {
        _databaseMock
            .Setup(d => d.RunCommandAsync<BsonDocument>(
                It.IsAny<Command<BsonDocument>>(),
                It.IsAny<ReadPreference>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BsonDocument("ok", 1));

        var healthCheck = new MongoDbHealthCheck(_contextMock.Object);

        var resultado = await healthCheck.CheckHealthAsync(CriarContexto());

        Assert.Equal(HealthStatus.Healthy, resultado.Status);
    }

    [Fact]
    public async Task DeveRetornarDoenteSeSeLancarExcecao()
    {
        var excecao = new TimeoutException("Conexão com MongoDB expirou.");

        _databaseMock
            .Setup(d => d.RunCommandAsync<BsonDocument>(
                It.IsAny<Command<BsonDocument>>(),
                It.IsAny<ReadPreference>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(excecao);

        var healthCheck = new MongoDbHealthCheck(_contextMock.Object);

        var resultado = await healthCheck.CheckHealthAsync(CriarContexto());

        Assert.Equal(HealthStatus.Unhealthy, resultado.Status);
        Assert.Same(excecao, resultado.Exception);
    }

    [Fact]
    public async Task DevePassarCancellationTokenParaMongoDB()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _databaseMock
            .Setup(d => d.RunCommandAsync<BsonDocument>(
                It.IsAny<Command<BsonDocument>>(),
                It.IsAny<ReadPreference>(),
                token))
            .ReturnsAsync(new BsonDocument("ok", 1));

        var healthCheck = new MongoDbHealthCheck(_contextMock.Object);

        var resultado = await healthCheck.CheckHealthAsync(CriarContexto(), token);

        Assert.Equal(HealthStatus.Healthy, resultado.Status);

        _databaseMock.Verify(d => d.RunCommandAsync<BsonDocument>(
            It.IsAny<Command<BsonDocument>>(),
            It.IsAny<ReadPreference>(),
            token), Times.Once);
    }
}
