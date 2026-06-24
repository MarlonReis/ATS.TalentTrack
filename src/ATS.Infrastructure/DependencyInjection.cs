namespace ATS.Infrastructure;

using ATS.Application.Candidatos.Commands.CreateCandidato;
using ATS.Application.Candidaturas.Commands.AprovarCandidatura;
using ATS.Application.Candidaturas.Commands.CancelarCandidatura;
using ATS.Application.Candidaturas.Commands.CandidatarSe;
using ATS.Application.Candidaturas.Commands.ReprovarCandidatura;
using ATS.Application.Candidaturas.Queries.GetCandidaturaById;
using ATS.Application.Candidaturas.Queries.ListCandidatosPorVaga;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // ── MongoDB ──────────────────────────────────────────────────────────
        // services.AddSingleton<MongoDbContext>();

        // ── Repositórios ─────────────────────────────────────────────────────
        // services.AddScoped<ICandidatoRepository,   CandidatoRepository>();
        // services.AddScoped<IVagaRepository,         VagaRepository>();
        // services.AddScoped<ICandidaturaRepository,  CandidaturaRepository>();

        // ── Candidatos – Commands ─────────────────────────────────────────────
        services.AddScoped<CreateCandidatoHandler>();

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
}
