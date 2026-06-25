using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Vagas.Repositories;
using ATS.Infrastructure.Persistence.Context;
using ATS.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATS.Infrastructure.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructureDeveRegistrarMongoDbERepositorios()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDB:ConnectionString"] = "mongodb://localhost:27017",
                ["MongoDB:DatabaseName"] = "AtsDbTests",
                ["MongoDB:MaxConnectionPoolSize"] = "30"
            })
            .Build();

        var retorno = services.AddInfrastructure(configuration);

        Assert.Same(services, retorno);
        AssertRegistration<IMongoDbContext, MongoDbContext>(services, ServiceLifetime.Singleton);
        AssertRegistration<ICandidatoRepository, CandidatoRepository>(services, ServiceLifetime.Scoped);
        AssertRegistration<IVagaRepository, VagaRepository>(services, ServiceLifetime.Scoped);
        AssertRegistration<ICandidaturaRepository, CandidaturaRepository>(services, ServiceLifetime.Scoped);
    }

    private static void AssertRegistration<TService, TImplementation>(
        IServiceCollection services,
        ServiceLifetime lifetime)
    {
        var descriptor = Assert.Single(
            services,
            service => service.ServiceType == typeof(TService));

        Assert.Equal(lifetime, descriptor.Lifetime);

        if (descriptor.ImplementationInstance is not null)
        {
            Assert.IsType<TImplementation>(descriptor.ImplementationInstance);
            return;
        }

        Assert.Equal(typeof(TImplementation), descriptor.ImplementationType);
    }
}
