namespace ATS.E2E.Tests.API;

using System.Net;
using Alba;
using ATS.Application.Candidaturas.Commands.CandidatarSe;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using ATS.E2E.Tests.Infrastructure;
using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

[Collection(nameof(AlbaHostCollection))]
public sealed class CandidaturasControllerE2ETests : IAsyncLifetime
{
    private readonly IAlbaHost _host;
    private readonly ICandidaturaRepository _candidaturaRepository;
    private readonly ICandidatoRepository _candidatoRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly IMongoDbContext _dbContext;

    public CandidaturasControllerE2ETests(AtsAlbaHostFixture fixture)
    {
        _host = fixture.Host;
        _candidaturaRepository = _host.Services.GetRequiredService<ICandidaturaRepository>();
        _candidatoRepository = _host.Services.GetRequiredService<ICandidatoRepository>();
        _vagaRepository = _host.Services.GetRequiredService<IVagaRepository>();
        _dbContext = _host.Services.GetRequiredService<IMongoDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.GetCollection<Candidatura>("candidaturas")
            .DeleteManyAsync(FilterDefinition<Candidatura>.Empty);
        await _dbContext.GetCollection<Candidato>("candidatos")
            .DeleteManyAsync(FilterDefinition<Candidato>.Empty);
        await _dbContext.GetCollection<Vaga>("vagas")
            .DeleteManyAsync(FilterDefinition<Vaga>.Empty);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostCandidaturasDeveCriarCandidatura()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var command = new CandidatarSeCommand(candidato.Id, vaga.Id);

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/candidaturas");
            _.StatusCodeShouldBe(HttpStatusCode.Created);
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain(candidato.Id.ToString());
            _.ContentShouldContain(vaga.Id.ToString());
            _.ContentShouldContain(candidato.Nome);
            _.ContentShouldContain(vaga.Titulo);
            _.ContentShouldContain("\"statusDescricao\":\"Em An");
        });

        var existe = await _candidaturaRepository.ExisteAsync(candidato.Id, vaga.Id);
        Assert.True(existe);
    }

    [Fact]
    public async Task PostCandidaturasDeveRetornarNotFoundQuandoCandidatoNaoExiste()
    {
        var vaga = await SeedVagaAsync();
        var command = new CandidatarSeCommand(Guid.NewGuid(), vaga.Id);

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/candidaturas");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task PostCandidaturasDeveRetornarNotFoundQuandoVagaNaoExiste()
    {
        var candidato = await SeedCandidatoAsync();
        var command = new CandidatarSeCommand(candidato.Id, Guid.NewGuid());

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/candidaturas");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task PostCandidaturasDeveRetornarConflictQuandoVagaFechada()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        vaga.Fechar();
        await _vagaRepository.AtualizarAsync(vaga);

        var command = new CandidatarSeCommand(candidato.Id, vaga.Id);

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/candidaturas");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("vaga fechada");
        });
    }

    [Fact]
    public async Task PostCandidaturasDeveRetornarConflictQuandoCandidatoJaCandidatou()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        _ = await SeedCandidaturaAsync(candidato, vaga);

        var command = new CandidatarSeCommand(candidato.Id, vaga.Id);

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/candidaturas");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("se candidatou");
        });
    }

    // ─── GET /api/v1/candidaturas/{id} ───────────────────────────────────────

    [Fact]
    public async Task GetCandidaturaPorIdDeveRetornarCandidaturaDetalhada()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var candidatura = await SeedCandidaturaAsync(candidato, vaga);

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/candidaturas/{candidatura.Id}");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain(candidatura.Id.ToString());
            _.ContentShouldContain(candidato.Nome);
            _.ContentShouldContain(candidato.Email.Value);
            _.ContentShouldContain(candidato.Telefone.Value);
            _.ContentShouldContain(vaga.Titulo);
            _.ContentShouldContain("\"possuiCurriculo\":false");
        });
    }

    [Fact]
    public async Task GetCandidaturaPorIdDeveRetornarNotFoundQuandoNaoExiste()
    {
        var id = Guid.NewGuid();

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/candidaturas/{id}");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    // ─── GET /api/v1/candidaturas/vagas/{vagaId}/candidatos ──────────────────

    [Fact]
    public async Task GetListarPorVagaDeveRetornarCandidaturas()
    {
        var candidato1 = await SeedCandidatoAsync("Ana Lima", "ana.lima@example.com");
        var candidato2 = await SeedCandidatoAsync("Bruno Melo", "bruno.melo@example.com");
        var vaga = await SeedVagaAsync();
        _ = await SeedCandidaturaAsync(candidato1, vaga);
        _ = await SeedCandidaturaAsync(candidato2, vaga);

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/candidaturas/vagas/{vaga.Id}/candidatos");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("Ana Lima");
            _.ContentShouldContain("Bruno Melo");
            _.ContentShouldContain(vaga.Titulo);
        });
    }

    [Fact]
    public async Task GetListarPorVagaDeveRetornarNotFoundQuandoVagaNaoExiste()
    {
        var vagaId = Guid.NewGuid();

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/candidaturas/vagas/{vagaId}/candidatos");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task GetListarPorVagaDeveRetornarListaVaziaQuandoNaoHaCandidaturas()
    {
        var vaga = await SeedVagaAsync();

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/candidaturas/vagas/{vaga.Id}/candidatos");
            _.StatusCodeShouldBeOk();
            _.ContentShouldBe("[]");
        });
    }

    // ─── PATCH /api/v1/candidaturas/{id}/aprovar ─────────────────────────────

    [Fact]
    public async Task PatchAprovarDeveAprovarCandidatura()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var candidatura = await SeedCandidaturaAsync(candidato, vaga);

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { observacoes = (string?)null }).ToUrl($"/api/v1/candidaturas/{candidatura.Id}/aprovar");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("\"statusDescricao\":\"Aprovado\"");
        });

        var atualizada = await _candidaturaRepository.ObterPorIdAsync(candidatura.Id);
        Assert.Equal(ATS.Domain.Candidaturas.Enums.StatusCandidatura.Aprovado, atualizada!.Status);
    }

    [Fact]
    public async Task PatchAprovarDeveAprovarCandidaturaComObservacoes()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var candidatura = await SeedCandidaturaAsync(candidato, vaga);
        const string observacoes = "Excelente perfil!";

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { observacoes }).ToUrl($"/api/v1/candidaturas/{candidatura.Id}/aprovar");
            _.StatusCodeShouldBeOk();
            _.ContentShouldContain("Excelente perfil!");
        });
    }

    [Fact]
    public async Task PatchAprovarDeveRetornarNotFoundQuandoCandidaturaNaoExiste()
    {
        var id = Guid.NewGuid();

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { observacoes = (string?)null }).ToUrl($"/api/v1/candidaturas/{id}/aprovar");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task PatchAprovarDeveRetornarConflictQuandoCandidaturaJaAprovada()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var candidatura = await SeedCandidaturaAsync(candidato, vaga);
        candidatura.Aprovar();
        await _candidaturaRepository.AtualizarAsync(candidatura);

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { observacoes = (string?)null }).ToUrl($"/api/v1/candidaturas/{candidatura.Id}/aprovar");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("Somente candidaturas");
        });
    }

    // ─── PATCH /api/v1/candidaturas/{id}/reprovar ────────────────────────────

    [Fact]
    public async Task PatchReprovarDeveReprovarCandidatura()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var candidatura = await SeedCandidaturaAsync(candidato, vaga);

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { observacoes = "Perfil não aderente." }).ToUrl($"/api/v1/candidaturas/{candidatura.Id}/reprovar");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("\"statusDescricao\":\"Reprovado\"");
            _.ContentShouldContain("Perfil não aderente.");
        });

        var atualizada = await _candidaturaRepository.ObterPorIdAsync(candidatura.Id);
        Assert.Equal(ATS.Domain.Candidaturas.Enums.StatusCandidatura.Reprovado, atualizada!.Status);
    }

    [Fact]
    public async Task PatchReprovarDeveRetornarNotFoundQuandoCandidaturaNaoExiste()
    {
        var id = Guid.NewGuid();

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { observacoes = (string?)null }).ToUrl($"/api/v1/candidaturas/{id}/reprovar");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task PatchReprovarDeveRetornarConflictQuandoCandidaturaJaReprovada()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var candidatura = await SeedCandidaturaAsync(candidato, vaga);
        candidatura.Reprovar();
        await _candidaturaRepository.AtualizarAsync(candidatura);

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { observacoes = (string?)null }).ToUrl($"/api/v1/candidaturas/{candidatura.Id}/reprovar");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("Somente candidaturas");
        });
    }

    // ─── PATCH /api/v1/candidaturas/{id}/cancelar ────────────────────────────

    [Fact]
    public async Task PatchCancelarDeveCancelarCandidatura()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var candidatura = await SeedCandidaturaAsync(candidato, vaga);

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { }).ToUrl($"/api/v1/candidaturas/{candidatura.Id}/cancelar");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("\"statusDescricao\":\"Cancelado\"");
        });

        var atualizada = await _candidaturaRepository.ObterPorIdAsync(candidatura.Id);
        Assert.Equal(ATS.Domain.Candidaturas.Enums.StatusCandidatura.Cancelado, atualizada!.Status);
    }

    [Fact]
    public async Task PatchCancelarDeveRetornarNotFoundQuandoCandidaturaNaoExiste()
    {
        var id = Guid.NewGuid();

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { }).ToUrl($"/api/v1/candidaturas/{id}/cancelar");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task PatchCancelarDeveRetornarConflictQuandoCandidaturaJaCancelada()
    {
        var candidato = await SeedCandidatoAsync();
        var vaga = await SeedVagaAsync();
        var candidatura = await SeedCandidaturaAsync(candidato, vaga);
        candidatura.Cancelar();
        await _candidaturaRepository.AtualizarAsync(candidatura);

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { }).ToUrl($"/api/v1/candidaturas/{candidatura.Id}/cancelar");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("foi cancelada");
        });
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<Candidato> SeedCandidatoAsync(
        string nome = "Maria Silva",
        string email = "maria.silva@example.com",
        string telefone = "11999998888")
    {
        var candidato = Candidato.Criar(nome, email, telefone);
        await _candidatoRepository.AdicionarAsync(candidato);
        return candidato;
    }

    private async Task<Vaga> SeedVagaAsync(
        string titulo = "Engenheiro de Software",
        string descricao = "Vaga para desenvolvedor backend",
        string requisitos = "C#, .NET",
        decimal salario = 10000m)
    {
        var vaga = Vaga.Criar(titulo, descricao, requisitos, salario);
        await _vagaRepository.AdicionarAsync(vaga);
        return vaga;
    }

    private async Task<Candidatura> SeedCandidaturaAsync(Candidato candidato, Vaga vaga)
    {
        var candidatura = Candidatura.Criar(candidato.Id, vaga.Id);
        await _candidaturaRepository.AdicionarAsync(candidatura);
        return candidatura;
    }
}
