namespace ATS.E2E.Tests.API;

using System.Net;
using Alba;
using ATS.API.Requests.Vagas;
using ATS.Application.Vagas.Commands.CreateVaga;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using ATS.E2E.Tests.Infrastructure;
using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

[Collection(nameof(AlbaHostCollection))]
public sealed class VagasControllerE2ETests : IAsyncLifetime
{
    private readonly IAlbaHost _host;
    private readonly IVagaRepository _vagaRepository;
    private readonly IMongoDbContext _dbContext;

    public VagasControllerE2ETests(AtsAlbaHostFixture fixture)
    {
        _host = fixture.Host;
        _vagaRepository = _host.Services.GetRequiredService<IVagaRepository>();
        _dbContext = _host.Services.GetRequiredService<IMongoDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.GetCollection<Vaga>("vagas")
            .DeleteManyAsync(FilterDefinition<Vaga>.Empty);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ─── POST /api/v1/vagas ───────────────────────────────────────────────────

    [Fact]
    public async Task PostVagasDeveCriarVaga()
    {
        var command = new CreateVagaCommand(
            "Engenheiro de Software",
            "Vaga para desenvolvedor backend",
            "C#, .NET, MongoDB",
            12000m);

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/vagas");
            _.StatusCodeShouldBe(HttpStatusCode.Created);
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("Engenheiro de Software");
            _.ContentShouldContain("\"statusDescricao\":\"Aberta\"");
            _.ContentShouldContain("\"salario\":12000");
        });

        Assert.Equal(1, await _vagaRepository.ContarAsync());
    }

    [Fact]
    public async Task PostVagasDeveCriarVagaSemRequisitosESalario()
    {
        var command = new CreateVagaCommand(
            "Analista de Dados",
            "Vaga para analista de dados");

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/vagas");
            _.StatusCodeShouldBe(HttpStatusCode.Created);
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("Analista de Dados");
            _.ContentShouldContain("\"salario\":0");
        });
    }

    [Fact]
    public async Task PostVagasDeveRetornarBadRequestQuandoTituloVazio()
    {
        var command = new CreateVagaCommand("", "Descricao valida", null, 5000m);

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/vagas");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    [Fact]
    public async Task PostVagasDeveRetornarBadRequestQuandoTituloExcede200Caracteres()
    {
        var tituloLongo = new string('A', 201);
        var command = new CreateVagaCommand(tituloLongo, "Descricao valida", null, 5000m);

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/vagas");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    [Fact]
    public async Task PostVagasDeveRetornarBadRequestQuandoDescricaoVazia()
    {
        var command = new CreateVagaCommand("Titulo valido", "", null, 5000m);

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/vagas");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    // ─── GET /api/v1/vagas/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task GetVagaPorIdDeveRetornarVaga()
    {
        var vaga = await SeedVagaAsync();

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/vagas/{vaga.Id}");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain(vaga.Id.ToString());
            _.ContentShouldContain(vaga.Titulo);
            _.ContentShouldContain("\"statusDescricao\":\"Aberta\"");
        });
    }

    [Fact]
    public async Task GetVagaPorIdDeveRetornarNotFoundQuandoNaoExiste()
    {
        var id = Guid.NewGuid();

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/vagas/{id}");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    // ─── GET /api/v1/vagas ────────────────────────────────────────────────────

    [Fact]
    public async Task GetVagasDeveListarVagasPaginadas()
    {
        _ = await SeedVagaAsync("Vaga Um", "Descricao Um");
        _ = await SeedVagaAsync("Vaga Dois", "Descricao Dois");
        _ = await SeedVagaAsync("Vaga Tres", "Descricao Tres");

        await _host.Scenario(_ =>
        {
            _.Get.Url("/api/v1/vagas?pagina=1&tamanhoPagina=2");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("\"total\":3");
            _.ContentShouldContain("\"pagina\":1");
            _.ContentShouldContain("\"tamanhoPagina\":2");
        });
    }

    [Fact]
    public async Task GetVagasDeveRetornarTodasComValoresPadraoDePageQuery()
    {
        _ = await SeedVagaAsync("Vaga A", "Desc A");
        _ = await SeedVagaAsync("Vaga B", "Desc B");

        await _host.Scenario(_ =>
        {
            _.Get.Url("/api/v1/vagas");
            _.StatusCodeShouldBeOk();
            _.ContentShouldContain("\"total\":2");
        });
    }

    [Fact]
    public async Task GetVagasDeveRetornarBadRequestQuandoPaginaZero()
    {
        await _host.Scenario(_ =>
        {
            _.Get.Url("/api/v1/vagas?pagina=0&tamanhoPagina=10");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    [Fact]
    public async Task GetVagasDeveRetornarBadRequestQuandoTamanhoPaginaExcedeMaximo()
    {
        await _host.Scenario(_ =>
        {
            _.Get.Url("/api/v1/vagas?pagina=1&tamanhoPagina=101");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    [Fact]
    public async Task GetVagasDeveRetornarBadRequestQuandoTamanhoPaginaZero()
    {
        await _host.Scenario(_ =>
        {
            _.Get.Url("/api/v1/vagas?pagina=1&tamanhoPagina=0");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    [Fact]
    public async Task GetVagasDeveFiltrarPorStatus()
    {
        var vagaAberta = await SeedVagaAsync("Vaga Aberta", "Desc");
        var vagaFechada = await SeedVagaAsync("Vaga Fechada", "Desc");
        vagaFechada.Fechar();
        await _vagaRepository.AtualizarAsync(vagaFechada);

        await _host.Scenario(_ =>
        {
            _.Get.Url("/api/v1/vagas?pagina=1&tamanhoPagina=20&status=1");
            _.StatusCodeShouldBeOk();
            _.ContentShouldContain("Vaga Aberta");
            _.ContentShouldNotContain("Vaga Fechada");
        });
    }

    // ─── PUT /api/v1/vagas/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task PutVagasDeveAtualizarVaga()
    {
        var vaga = await SeedVagaAsync();
        var request = new AtualizarVagaRequest(
            "Engenheiro Senior",
            "Nova descricao atualizada",
            "C#, AWS",
            15000m);

        await _host.Scenario(_ =>
        {
            _.Put.Json(request).ToUrl($"/api/v1/vagas/{vaga.Id}");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("Engenheiro Senior");
            _.ContentShouldContain("\"salario\":15000");
        });

        var atualizada = await _vagaRepository.ObterPorIdAsync(vaga.Id);
        Assert.NotNull(atualizada);
        Assert.Equal("Engenheiro Senior", atualizada.Titulo);
    }

    [Fact]
    public async Task PutVagasDeveRetornarNotFoundQuandoVagaNaoExiste()
    {
        var request = new AtualizarVagaRequest("Titulo", "Descricao", null, 5000m);

        await _host.Scenario(_ =>
        {
            _.Put.Json(request).ToUrl($"/api/v1/vagas/{Guid.NewGuid()}");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task PutVagasDeveRetornarConflictQuandoVagaFechada()
    {
        var vaga = await SeedVagaAsync();
        vaga.Fechar();
        await _vagaRepository.AtualizarAsync(vaga);

        var request = new AtualizarVagaRequest("Titulo Novo", "Descricao Nova", null, 6000m);

        await _host.Scenario(_ =>
        {
            _.Put.Json(request).ToUrl($"/api/v1/vagas/{vaga.Id}");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("editar");
        });
    }

    // ─── PATCH /api/v1/vagas/{id}/fechar ─────────────────────────────────────

    [Fact]
    public async Task PatchFecharDeveFecharVaga()
    {
        var vaga = await SeedVagaAsync();

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { }).ToUrl($"/api/v1/vagas/{vaga.Id}/fechar");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("\"statusDescricao\":\"Fechada\"");
        });

        var fechada = await _vagaRepository.ObterPorIdAsync(vaga.Id);
        Assert.NotNull(fechada);
        Assert.Equal(ATS.Domain.Vagas.Enums.StatusVaga.Fechada, fechada.Status);
        Assert.NotNull(fechada.DataEncerramento);
    }

    [Fact]
    public async Task PatchFecharDeveRetornarNotFoundQuandoVagaNaoExiste()
    {
        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { }).ToUrl($"/api/v1/vagas/{Guid.NewGuid()}/fechar");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task PatchFecharDeveRetornarConflictQuandoVagaJaFechada()
    {
        var vaga = await SeedVagaAsync();
        vaga.Fechar();
        await _vagaRepository.AtualizarAsync(vaga);

        await _host.Scenario(_ =>
        {
            _.Patch.Json(new { }).ToUrl($"/api/v1/vagas/{vaga.Id}/fechar");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("fechada");
        });
    }

    // ─── DELETE /api/v1/vagas/{id} ────────────────────────────────────────────

    [Fact]
    public async Task DeleteVagasDeveRemoverVaga()
    {
        var vaga = await SeedVagaAsync();

        await _host.Scenario(_ =>
        {
            _.Delete.Url($"/api/v1/vagas/{vaga.Id}");
            _.StatusCodeShouldBe(HttpStatusCode.NoContent);
            _.ContentShouldBe(string.Empty);
        });

        var removida = await _vagaRepository.ObterPorIdAsync(vaga.Id);
        Assert.Null(removida);
    }

    [Fact]
    public async Task DeleteVagasDeveRetornarNotFoundQuandoVagaNaoExiste()
    {
        await _host.Scenario(_ =>
        {
            _.Delete.Url($"/api/v1/vagas/{Guid.NewGuid()}");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
        });
    }

    [Fact]
    public async Task PutVagasDeveRetornarBadRequestQuandoTituloVazio()
    {
        var vaga = await SeedVagaAsync();
        var request = new AtualizarVagaRequest("", "Descricao valida", null, 5000m);

        await _host.Scenario(_ =>
        {
            _.Put.Json(request).ToUrl($"/api/v1/vagas/{vaga.Id}");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    [Fact]
    public async Task PutVagasDeveRetornarBadRequestQuandoDescricaoVazia()
    {
        var vaga = await SeedVagaAsync();
        var request = new AtualizarVagaRequest("Titulo valido", "", null, 5000m);

        await _host.Scenario(_ =>
        {
            _.Put.Json(request).ToUrl($"/api/v1/vagas/{vaga.Id}");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

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
}
