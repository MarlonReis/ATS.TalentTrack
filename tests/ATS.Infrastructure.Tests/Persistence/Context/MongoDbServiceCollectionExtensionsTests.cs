using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATS.Infrastructure.Tests.Persistence.Context;

public class MongoDbServiceCollectionExtensionsTests
{
    [Fact]
    public void DeveRegistrarMongoDbContextComoSingletonAoReceberSettings()
    {
        var services = new ServiceCollection();
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "AtsDbSettings",
            MaxPoolSize = 35
        };

        var retorno = services.AddMongoDb(settings);

        Assert.Same(services, retorno);

        var descriptor = Assert.Single(
            services,
            s => s.ServiceType == typeof(IMongoDbContext));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        var context = Assert.IsType<MongoDbContext>(descriptor.ImplementationInstance);
        var collection = context.GetCollection<DocumentoTeste>("vagas");

        Assert.Equal("AtsDbSettings", collection.Database.DatabaseNamespace.DatabaseName);
        Assert.Equal(35, collection.Database.Client.Settings.MaxConnectionPoolSize);
    }

    [Fact]
    public void DeveResolverSempreAMesmaInstanciaRegistradaComoSingleton()
    {
        var services = new ServiceCollection();

        services.AddMongoDb(new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "AtsDbSingleton",
            MaxPoolSize = 20
        });

        using var provider = services.BuildServiceProvider();

        var primeiroContexto = provider.GetRequiredService<IMongoDbContext>();
        var segundoContexto = provider.GetRequiredService<IMongoDbContext>();

        Assert.Same(primeiroContexto, segundoContexto);
    }

    [Fact]
    public void DeveRegistrarMongoDbContextComValoresDaConfiguracao()
    {
        var services = new ServiceCollection();
        var configuration = CriarConfiguracao(new Dictionary<string, string?>
        {
            ["MongoDB:ConnectionString"] = "mongodb://localhost:27018",
            ["MongoDB:DatabaseName"] = "AtsDbConfigurado",
            ["MongoDB:MaxConnectionPoolSize"] = "77"
        });

        var retorno = services.AddMongoDb(configuration);

        Assert.Same(services, retorno);

        var context = ObterContextoRegistrado(services);
        var collection = context.GetCollection<DocumentoTeste>("candidatos");

        Assert.Equal("AtsDbConfigurado", collection.Database.DatabaseNamespace.DatabaseName);
        Assert.Equal(77, collection.Database.Client.Settings.MaxConnectionPoolSize);
    }

    [Fact]
    public void DeveUsarValoresPadraoQuandoConfiguracaoMongoDbEstiverAusente()
    {
        var services = new ServiceCollection();
        var configuration = CriarConfiguracao(new Dictionary<string, string?>());

        services.AddMongoDb(configuration);

        var context = ObterContextoRegistrado(services);
        var collection = context.GetCollection<DocumentoTeste>("vagas");

        Assert.Equal("AtsDb", collection.Database.DatabaseNamespace.DatabaseName);
        Assert.Equal(100, collection.Database.Client.Settings.MaxConnectionPoolSize);
    }

    [Fact]
    public void DeveAceitarMaxPoolSizeComoAliasDeConfiguracao()
    {
        var services = new ServiceCollection();
        var configuration = CriarConfiguracao(new Dictionary<string, string?>
        {
            ["MongoDB:DatabaseName"] = "AtsDbAlias",
            ["MongoDB:MaxPoolSize"] = "44"
        });

        services.AddMongoDb(configuration);

        var context = ObterContextoRegistrado(services);
        var collection = context.GetCollection<DocumentoTeste>("vagas");

        Assert.Equal("AtsDbAlias", collection.Database.DatabaseNamespace.DatabaseName);
        Assert.Equal(44, collection.Database.Client.Settings.MaxConnectionPoolSize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("valor-invalido")]
    public void DeveUsarMaxPoolSizePadraoQuandoConfiguracaoForInvalida(
        string maxConnectionPoolSize)
    {
        var services = new ServiceCollection();
        var configuration = CriarConfiguracao(new Dictionary<string, string?>
        {
            ["MongoDB:ConnectionString"] = "mongodb://localhost:27017",
            ["MongoDB:DatabaseName"] = "AtsDbMaxPoolInvalido",
            ["MongoDB:MaxConnectionPoolSize"] = maxConnectionPoolSize
        });

        services.AddMongoDb(configuration);

        var context = ObterContextoRegistrado(services);
        var collection = context.GetCollection<DocumentoTeste>("vagas");

        Assert.Equal("AtsDbMaxPoolInvalido", collection.Database.DatabaseNamespace.DatabaseName);
        Assert.Equal(100, collection.Database.Client.Settings.MaxConnectionPoolSize);
    }

    private static IConfiguration CriarConfiguracao(
        Dictionary<string, string?> valores)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();

    private static MongoDbContext ObterContextoRegistrado(IServiceCollection services)
    {
        var descriptor = Assert.Single(
            services,
            s => s.ServiceType == typeof(IMongoDbContext));

        return Assert.IsType<MongoDbContext>(descriptor.ImplementationInstance);
    }

    private sealed class DocumentoTeste
    {
        public Guid Id { get; init; }
        public string? Nome { get; init; }
    }
}
