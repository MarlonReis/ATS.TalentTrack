namespace ATS.Infrastructure;

using ATS.Application.Candidatos.Commands.AddCurriculo;
using ATS.Application.Candidatos.Commands.CreateCandidato;
using ATS.Application.Candidatos.Commands.DeleteCandidato;
using ATS.Application.Candidatos.Commands.UpdateCandidato;
using ATS.Application.Candidatos.Queries.GetCandidatoById;
using ATS.Application.Candidatos.Queries.ListCandidatos;
using ATS.Application.Candidaturas.Commands.AprovarCandidatura;
using ATS.Application.Candidaturas.Commands.CancelarCandidatura;
using ATS.Application.Candidaturas.Commands.CandidatarSe;
using ATS.Application.Candidaturas.Commands.ReprovarCandidatura;
using ATS.Application.Candidaturas.Queries.GetCandidaturaById;
using ATS.Application.Candidaturas.Queries.ListCandidatosPorVaga;
using ATS.Application.Vagas.Commands.CreateVaga;
using ATS.Application.Vagas.Commands.DeleteVaga;
using ATS.Application.Vagas.Commands.FecharVaga;
using ATS.Application.Vagas.Commands.UpdateVaga;
using ATS.Application.Vagas.Queries.GetVagaById;
using ATS.Application.Vagas.Queries.ListVagas;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Vagas.Repositories;
using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // ── MongoDB ──────────────────────────────────────────────────────────
        services.AddSingleton<IMongoDbContext>(new MongoDbContext(ReadMongoSettings(configuration)));

        // ── Repositórios ──────────────────────────────────────────────────────
        // services.AddScoped<ICandidatoRepository,  CandidatoRepository>();
        // services.AddScoped<IVagaRepository,        VagaRepository>();
        // services.AddScoped<ICandidaturaRepository, CandidaturaRepository>();

        // ── Vagas – Commands ──────────────────────────────────────────────────
        services.AddScoped<CreateVagaHandler>();
        services.AddScoped<UpdateVagaHandler>();
        services.AddScoped<DeleteVagaHandler>();
        services.AddScoped<FecharVagaHandler>();


        // ── Vagas – Queries ───────────────────────────────────────────────────
        services.AddScoped<GetVagaByIdHandler>();
        services.AddScoped<ListVagasHandler>();

        // ── Candidatos – Commands ─────────────────────────────────────────────

        services.AddScoped<CreateCandidatoHandler>();
        services.AddScoped<UpdateCandidatoHandler>();
        services.AddScoped<DeleteCandidatoHandler>();
        services.AddScoped<AddCurriculoHandler>();

        // ── Candidatos – Queries ──────────────────────────────────────────────
        services.AddScoped<GetCandidatoByIdHandler>();
        services.AddScoped<ListCandidatosHandler>();

        // ── Candidaturas – Commands ───────────────────────────────────────────
        services.AddScoped<CandidatarSeHandler>();
        services.AddScoped<AprovarCandidaturaHandler>();
        services.AddScoped<ReprovarCandidaturaHandler>();
        services.AddScoped<CancelarCandidaturaHandler>();

        // ── Candidaturas – Queries ────────────────────────────────────────────
        services.AddScoped<GetCandidaturaByIdHandler>();
        services.AddScoped<ListCandidatosPorVagaHandler>();

        return services;
    }

    private static MongoDbSettings ReadMongoSettings(IConfiguration cfg) => new()
    {
        ConnectionString = cfg["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017",
        DatabaseName = cfg["MongoDB:DatabaseName"] ?? "AtsDb",
        MaxPoolSize = int.TryParse(cfg["MongoDB:MaxPoolSize"], out var n) ? n : 100,
    };

}
