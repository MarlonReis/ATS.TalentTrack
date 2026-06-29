namespace ATS.Infrastructure.Tests.Persistence.Indexes;

using ATS.Infrastructure.Persistence.Context;
using ATS.Infrastructure.Persistence.Indexes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

public class MongoIndexInitializerTests
{
    [Fact]
    public async Task StartAsync_DeveLogarFalhaEContinuarQuandoContextoLancaExcecao()
    {
        var excecao = new InvalidOperationException("requires authentication");

        var contextMock = new Mock<IMongoDbContext>();
        contextMock
            .Setup(c => c.GetCollection<ATS.Domain.Candidatos.Entities.Candidato>("candidatos"))
            .Throws(excecao);

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock
            .Setup(s => s.GetService(typeof(IMongoDbContext)))
            .Returns(contextMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(servicesMock.Object);

        var asyncScopeMock = new AsyncServiceScopeMock(scopeMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(scopeMock.Object);

        // Usa um serviço real de DI para criar o IAsyncServiceScope via CreateAsyncScope()
        var services = new ServiceCollection();
        services.AddSingleton(contextMock.Object);
        var provider = services.BuildServiceProvider();

        var scopeFactoryReal = provider.GetRequiredService<IServiceScopeFactory>();

        var loggerMock = new Mock<ILogger<MongoIndexInitializer>>();
        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var initializer = new MongoIndexInitializer(scopeFactoryReal, loggerMock.Object);

        // Não deve lançar exceção — o catch absorve e loga
        await initializer.StartAsync(CancellationToken.None);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains("Falha", StringComparison.OrdinalIgnoreCase) ||
                    state.ToString()!.Contains("ndices", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_DeveRetornarCompletedTask()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<MongoIndexInitializer>>();

        var initializer = new MongoIndexInitializer(scopeFactory, loggerMock.Object);

        await initializer.StopAsync(CancellationToken.None); // não lança
    }

    private sealed class AsyncServiceScopeMock : IAsyncDisposable, IServiceScope
    {
        private readonly IServiceScope _inner;
        public AsyncServiceScopeMock(IServiceScope inner) => _inner = inner;
        public IServiceProvider ServiceProvider => _inner.ServiceProvider;
        public void Dispose() => _inner.Dispose();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
