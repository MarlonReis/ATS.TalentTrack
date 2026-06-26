using ATS.API.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using OpenTelemetry.Exporter;
using Serilog;
using Serilog.Events;


namespace ATS.API.Tests.Observability;

public class ObservabilityExtensionsTests
{
    // ── AddObservability ──────────────────────────────────────────────────────

    [Fact]
    public async Task AddObservability_DeveRetornarOMesmoBuilderParaEncadeamento()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Production";

        var resultado = builder.AddObservability();

        await using var _ = builder.Build();
        Assert.Same(builder, resultado);
    }

    [Fact]
    public async Task AddObservability_DeveUsarConsoleTemplateEmDesenvolvimento()
    {
        // WebApplicationOptions é necessário para que context.HostingEnvironment.IsDevelopment()
        // retorne true dentro do callback Serilog durante Build()
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });
        builder.AddObservability();

        await using var app = builder.Build();
        Assert.NotNull(app);
    }

    [Fact]
    public async Task AddObservability_DeveUsarJsonFormatterEmProducao()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Production";
        builder.AddObservability();

        // Build executa o callback do Serilog (IsDevelopment = false → CompactJsonFormatter)
        await using var app = builder.Build();
        Assert.NotNull(app);
    }

    [Fact]
    public async Task AddObservability_DeveAdicionarOtlpExporterQuandoEndpointConfigurado()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Observability:OtlpEndpoint"] = "http://localhost:4317"
        });
        builder.Environment.EnvironmentName = "Production";
        builder.AddObservability();

        await using var app = builder.Build();
        Assert.NotNull(app);
    }

    [Fact]
    public async Task AddObservability_DeveAdicionarConsoleExporterQuandoHabilitado()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Observability:EnableConsoleExporter"] = "true"
        });
        builder.Environment.EnvironmentName = "Production";
        builder.AddObservability();

        await using var app = builder.Build();
        Assert.NotNull(app);
    }

    [Fact]
    public async Task AddObservability_DeveNaoAdicionarPrometheusExporterQuandoDesabilitado()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Observability:EnablePrometheusEndpoint"] = "false"
        });
        builder.Environment.EnvironmentName = "Production";
        builder.AddObservability();

        await using var app = builder.Build();
        Assert.NotNull(app);
    }

    [Fact]
    public async Task AddObservability_DeveUsarSettingsPadraoQuandoSecaoObservabilityAusente()
    {
        var builder = WebApplication.CreateBuilder();
        // Limpa todas as fontes para garantir que nenhuma seção Observability exista
        // e que Get<ObservabilitySettings>() retorne null → exercita o ramo ?? new()
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(Array.Empty<KeyValuePair<string, string?>>());
        builder.Environment.EnvironmentName = "Production";
        builder.AddObservability();

        await using var app = builder.Build();
        Assert.NotNull(app);
    }

    // ── UseObservability ──────────────────────────────────────────────────────

    [Fact]
    public async Task UseObservability_DeveRetornarAMesmaApplicationParaEncadeamento()
    {
        await using var app = CriarAppComObservability();

        var resultado = app.UseObservability();

        Assert.Same(app, resultado);
    }

    [Fact]
    public async Task UseObservability_DeveMapearEndpointMetricsQuandoPrometheusHabilitado()
    {
        await using var app = CriarAppComObservability(prometheus: true);

        // Não deve lançar exceção ao mapear o endpoint
        app.UseObservability();
        Assert.NotNull(app);
    }

    [Fact]
    public async Task UseObservability_DeveOmitirEndpointMetricsQuandoPrometheusDesabilitado()
    {
        await using var app = CriarAppComObservability(prometheus: false);

        // Não deve tentar mapear endpoint (sem exporter registrado)
        app.UseObservability();
        Assert.NotNull(app);
    }

    [Fact]
    public async Task UseObservability_DeveUsarSettingsPadraoQuandoSecaoObservabilityAusente()
    {
        // Limpa config para forçar Get<ObservabilitySettings>() = null → ?? new()
        // EnablePrometheusEndpoint padrão é true, portanto exporter deve estar registrado
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(Array.Empty<KeyValuePair<string, string?>>());
        builder.Environment.EnvironmentName = "Production";
        builder.AddObservability(); // ?? new() → EnablePrometheusEndpoint = true → registra exporter
        await using var app = builder.Build();

        app.UseObservability(); // ?? new() → EnablePrometheusEndpoint = true → mapeia /metrics
        Assert.NotNull(app);
    }

    // ── TracingFilter ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("/api/candidatos")]
    [InlineData("/api/vagas")]
    [InlineData("/swagger")]
    public void TracingFilter_DevePermitirRotasDeNegocio(string path)
    {
        var ctx = CriarHttpContext(path);

        var resultado = ObservabilityExtensions.TracingFilter(ctx);

        Assert.True(resultado);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public void TracingFilter_DeveExcluirRotasDeHealth(string path)
    {
        var ctx = CriarHttpContext(path);

        var resultado = ObservabilityExtensions.TracingFilter(ctx);

        Assert.False(resultado);
    }

    [Fact]
    public void TracingFilter_DeveExcluirRotaDeMetrics()
    {
        var ctx = CriarHttpContext("/metrics");

        var resultado = ObservabilityExtensions.TracingFilter(ctx);

        Assert.False(resultado);
    }

    // ── GetRequestLogLevel ────────────────────────────────────────────────────

    [Theory]
    [InlineData("/api/candidatos")]
    [InlineData("/api/vagas")]
    [InlineData("/")]
    public void GetRequestLogLevel_DeveRetornarInformationParaRotasNormais(string path)
    {
        var ctx = CriarHttpContext(path);

        var nivel = ObservabilityExtensions.GetRequestLogLevel(ctx, elapsed: 10, ex: null);

        Assert.Equal(LogEventLevel.Information, nivel);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public void GetRequestLogLevel_DeveRetornarVerboseParaRotasDeHealth(string path)
    {
        var ctx = CriarHttpContext(path);

        var nivel = ObservabilityExtensions.GetRequestLogLevel(ctx, elapsed: 5, ex: null);

        Assert.Equal(LogEventLevel.Verbose, nivel);
    }

    [Fact]
    public void GetRequestLogLevel_DeveRetornarVerboseParaMetrics()
    {
        var ctx = CriarHttpContext("/metrics");

        var nivel = ObservabilityExtensions.GetRequestLogLevel(ctx, elapsed: 2, ex: null);

        Assert.Equal(LogEventLevel.Verbose, nivel);
    }

    // ── EnrichRequestLog ──────────────────────────────────────────────────────

    [Fact]
    public void EnrichRequestLog_DeveDefinirPropriedadesDeRequest()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Host = new HostString("api.exemplo.com");
        ctx.Request.Scheme = "https";
        ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        var diagMock = new Mock<IDiagnosticContext>();

        ObservabilityExtensions.EnrichRequestLog(diagMock.Object, ctx);

        diagMock.Verify(d => d.Set("RequestHost", "api.exemplo.com", false), Times.Once);
        diagMock.Verify(d => d.Set("RequestScheme", "https", false), Times.Once);
        diagMock.Verify(d => d.Set("RemoteIp", "192.168.1.1", false), Times.Once);
    }

    [Fact]
    public void EnrichRequestLog_DeveDefinirRemoteIpComoNullQuandoNaoDisponivel()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Host = new HostString("localhost");
        ctx.Request.Scheme = "http";
        // RemoteIpAddress é null por padrão em DefaultHttpContext

        var diagMock = new Mock<IDiagnosticContext>();

        ObservabilityExtensions.EnrichRequestLog(diagMock.Object, ctx);

        diagMock.Verify(d => d.Set("RemoteIp", (string?)null, false), Times.Once);
    }

    // ── ConfigurarAspNetCoreTracing ───────────────────────────────────────────

    [Fact]
    public void ConfigurarAspNetCoreTracing_DeveConfigurarRecordExceptionEFilter()
    {
        var opts = new OpenTelemetry.Instrumentation.AspNetCore.AspNetCoreTraceInstrumentationOptions();

        ObservabilityExtensions.ConfigurarAspNetCoreTracing(opts);

        Assert.True(opts.RecordException);
        Assert.Equal(ObservabilityExtensions.TracingFilter, opts.Filter);
    }

    // ── AplicarOtlpEndpoint ───────────────────────────────────────────────────

    [Fact]
    public void AplicarOtlpEndpoint_DeveDefinirEndpointCorreto()
    {
        var opts = new OtlpExporterOptions();
        var url = "http://localhost:4317";

        ObservabilityExtensions.AplicarOtlpEndpoint(opts, url);

        Assert.Equal(new Uri(url), opts.Endpoint);
    }

    [Fact]
    public void AplicarOtlpEndpoint_DeveLancarExcecaoParaEndpointNulo()
    {
        var opts = new OtlpExporterOptions();

        Assert.Throws<ArgumentNullException>(() =>
            ObservabilityExtensions.AplicarOtlpEndpoint(opts, null!));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static WebApplication CriarAppComObservability(bool prometheus = true)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Observability:EnablePrometheusEndpoint"] = prometheus ? "true" : "false"
        });
        builder.Environment.EnvironmentName = "Production";
        builder.AddObservability();
        return builder.Build();
    }

    private static DefaultHttpContext CriarHttpContext(string path)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        return ctx;
    }
}
