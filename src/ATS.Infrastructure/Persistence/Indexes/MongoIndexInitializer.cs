namespace ATS.Infrastructure.Persistence.Indexes;

using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Vagas.Entities;
using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

/// <summary>
/// Cria todos os índices MongoDB ao iniciar a aplicação, de forma idempotente.
/// Falhas são registradas em log e não interrompem a inicialização do host.
/// </summary>
public sealed partial class MongoIndexInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(
        IServiceScopeFactory scopeFactory,
        ILogger<MongoIndexInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();

            await CriarIndicesCandidatosAsync(context.GetCollection<Candidato>("candidatos"), ct);
            await CriarIndicesVagasAsync(context.GetCollection<Vaga>("vagas"), ct);
            await CriarIndicesCandidaturasAsync(context.GetCollection<Candidatura>("candidaturas"), ct);

            LogIndicesCriados();
        }
        catch (Exception ex)
        {
            LogFalhaAoCriarIndices(ex);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private static async Task CriarIndicesCandidatosAsync(
        IMongoCollection<Candidato> col, CancellationToken ct)
    {
        // Unicidade de e-mail
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Candidato>(
                Builders<Candidato>.IndexKeys.Ascending("email.value"),
                new CreateIndexOptions { Unique = true, Name = "ix_candidatos_email_unique" }),
            cancellationToken: ct);

        // Ordenação por nome para listagens
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Candidato>(
                Builders<Candidato>.IndexKeys.Ascending("nome"),
                new CreateIndexOptions { Name = "ix_candidatos_nome" }),
            cancellationToken: ct);
    }

    private static async Task CriarIndicesVagasAsync(
        IMongoCollection<Vaga> col, CancellationToken ct)
    {
        // Ordenação por data de abertura (desc) — cursor pagination e listagens
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Vaga>(
                Builders<Vaga>.IndexKeys.Descending("dataAbertura"),
                new CreateIndexOptions { Name = "ix_vagas_dataAbertura_desc" }),
            cancellationToken: ct);

        // Filtro por status
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Vaga>(
                Builders<Vaga>.IndexKeys.Ascending("status"),
                new CreateIndexOptions { Name = "ix_vagas_status" }),
            cancellationToken: ct);

        // Composto status + dataAbertura para listagens filtradas e ordenadas
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Vaga>(
                Builders<Vaga>.IndexKeys
                    .Ascending("status")
                    .Descending("dataAbertura"),
                new CreateIndexOptions { Name = "ix_vagas_status_dataAbertura" }),
            cancellationToken: ct);
    }

    private static async Task CriarIndicesCandidaturasAsync(
        IMongoCollection<Candidatura> col, CancellationToken ct)
    {
        // Unicidade candidato + vaga (evita candidatura duplicada)
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Candidatura>(
                Builders<Candidatura>.IndexKeys
                    .Ascending("candidatoId")
                    .Ascending("vagaId"),
                new CreateIndexOptions { Unique = true, Name = "ix_candidaturas_candidato_vaga_unique" }),
            cancellationToken: ct);

        // Filtro por vagaId (para ListarPorVagaAsync)
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Candidatura>(
                Builders<Candidatura>.IndexKeys.Ascending("vagaId"),
                new CreateIndexOptions { Name = "ix_candidaturas_vagaId" }),
            cancellationToken: ct);

        // Filtro por candidatoId
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Candidatura>(
                Builders<Candidatura>.IndexKeys.Ascending("candidatoId"),
                new CreateIndexOptions { Name = "ix_candidaturas_candidatoId" }),
            cancellationToken: ct);

        // Filtro por status (para CancelarCandidaturasPendentes)
        await col.Indexes.CreateOneAsync(
            new CreateIndexModel<Candidatura>(
                Builders<Candidatura>.IndexKeys.Ascending("status"),
                new CreateIndexOptions { Name = "ix_candidaturas_status" }),
            cancellationToken: ct);
    }

    [LoggerMessage(EventId = 5001, Level = LogLevel.Information,
        Message = "[MongoDB] Índices criados/verificados com sucesso.")]
    private partial void LogIndicesCriados();

    [LoggerMessage(EventId = 5002, Level = LogLevel.Warning,
        Message = "[MongoDB] Falha ao criar índices — a aplicação continuará sem eles. Verifique permissões e string de conexão.")]
    private partial void LogFalhaAoCriarIndices(Exception ex);
}
