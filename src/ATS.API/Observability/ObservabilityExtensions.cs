namespace ATS.API.Observability;

using ATS.Application.Observability;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var settings = builder.Configuration
            .GetSection("Observability")
            .Get<ObservabilitySettings>() ?? new();

        // ── Serilog ───────────────────────────────────────────────────────────
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();

            if (context.HostingEnvironment.IsDevelopment())
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                loggerConfig.WriteTo.Console(new CompactJsonFormatter());
            }
        });

        // ── OpenTelemetry ─────────────────────────────────────────────────────
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(
                    serviceName: settings.ServiceName,
                    serviceVersion: settings.ServiceVersion)
                .AddAttributes([
                    new("deployment.environment", builder.Environment.EnvironmentName)
                ]))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(ConfigurarAspNetCoreTracing)
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrEmpty(settings.OtlpEndpoint))
                {
                    var tracingEndpoint = settings.OtlpEndpoint!;
                    tracing.AddOtlpExporter(o => AplicarOtlpEndpoint(o, tracingEndpoint));
                }

                if (settings.EnableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(AtsMetrics.MeterName);

                if (!string.IsNullOrEmpty(settings.OtlpEndpoint))
                {
                    var metricsEndpoint = settings.OtlpEndpoint!;
                    metrics.AddOtlpExporter(o => AplicarOtlpEndpoint(o, metricsEndpoint));
                }

                if (settings.EnableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }

                if (settings.EnablePrometheusEndpoint)
                {
                    metrics.AddPrometheusExporter();
                }
            });

        return builder;
    }

    public static WebApplication UseObservability(this WebApplication app)
    {
        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = EnrichRequestLog;
            opts.GetLevel = GetRequestLogLevel;
        });

        var settings = app.Configuration
            .GetSection("Observability")
            .Get<ObservabilitySettings>() ?? new();

        if (settings.EnablePrometheusEndpoint)
        {
            app.MapPrometheusScrapingEndpoint("/metrics");
        }

        return app;
    }

    // ── Métodos internos extraídos para testabilidade ─────────────────────────

    internal static bool TracingFilter(HttpContext ctx) =>
        !ctx.Request.Path.StartsWithSegments("/health") &&
        !ctx.Request.Path.StartsWithSegments("/metrics");

    internal static LogEventLevel GetRequestLogLevel(
        HttpContext ctx, double elapsed, Exception? ex) =>
        ctx.Request.Path.StartsWithSegments("/health") ||
        ctx.Request.Path.StartsWithSegments("/metrics")
            ? LogEventLevel.Verbose
            : LogEventLevel.Information;

    internal static void EnrichRequestLog(IDiagnosticContext diag, HttpContext ctx)
    {
        diag.Set("RequestHost", ctx.Request.Host.Value);
        diag.Set("RequestScheme", ctx.Request.Scheme);
        diag.Set("RemoteIp", ctx.Connection.RemoteIpAddress?.ToString());
    }

    internal static void ConfigurarAspNetCoreTracing(AspNetCoreTraceInstrumentationOptions opts)
    {
        opts.RecordException = true;
        opts.Filter = TracingFilter;
    }

    internal static void AplicarOtlpEndpoint(OtlpExporterOptions opts, string endpoint) =>
        opts.Endpoint = new Uri(endpoint);
}
