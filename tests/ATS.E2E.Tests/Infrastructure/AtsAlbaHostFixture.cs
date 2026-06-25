using Microsoft.AspNetCore.Hosting;

namespace ATS.E2E.Tests.Infrastructure;

using Alba;
using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;
using Xunit;

public sealed class AtsAlbaHostFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:7.0")
        .Build();

    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Host = await AlbaHost.For<Program>(builder =>
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMongoDbContext));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<IMongoDbContext>(
                    new MongoDbContext(new MongoDbSettings
                    {
                        ConnectionString = _container.GetConnectionString(),
                        DatabaseName = $"AtsE2E_{Guid.NewGuid():N}",
                        MaxPoolSize = 5
                    }));
            });
        });
    }

    public async Task DisposeAsync()
    {
        await Host.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(AlbaHostCollection))]
public sealed class AlbaHostCollection : ICollectionFixture<AtsAlbaHostFixture> { }
