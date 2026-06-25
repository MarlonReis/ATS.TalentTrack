namespace ATS.E2E.Tests.API;

using System.Net;
using Alba;
using ATS.API.Requests.Candidatos;
using ATS.Application.Candidatos.Commands.CreateCandidato;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.E2E.Tests.Infrastructure;
using ATS.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

[Collection(nameof(AlbaHostCollection))]
public sealed class CandidatosControllerE2ETests : IAsyncLifetime
{
    private readonly IAlbaHost _host;
    private readonly ICandidatoRepository _repository;
    private readonly IMongoDbContext _dbContext;

    public CandidatosControllerE2ETests(AtsAlbaHostFixture fixture)
    {
        _host = fixture.Host;
        _repository = _host.Services.GetRequiredService<ICandidatoRepository>();
        _dbContext = _host.Services.GetRequiredService<IMongoDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.GetCollection<Candidato>("candidatos")
            .DeleteManyAsync(FilterDefinition<Candidato>.Empty);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostCandidatosDeveCriarCandidato()
    {
        var command = new CreateCandidatoCommand(
            "Maria Silva",
            "maria.silva@example.com",
            "11999998888");

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/candidatos");
            _.StatusCodeShouldBe(HttpStatusCode.Created);
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("Maria Silva");
            _.ContentShouldContain("maria.silva@example.com");
            _.ContentShouldContain("\"possuiCurriculo\":false");
        });

        var candidato = await _repository.ObterPorEmailAsync("maria.silva@example.com");

        Assert.NotNull(candidato);
        Assert.NotEqual(Guid.Empty, candidato.Id);
        Assert.Equal("Maria Silva", candidato.Nome);
        Assert.Equal("maria.silva@example.com", candidato.Email.Value);
        Assert.Equal("11999998888", candidato.Telefone.Value);
        Assert.Null(candidato.Curriculo);
    }

    [Fact]
    public async Task PostCandidatosDeveRetornarConflictQuandoEmailJaExiste()
    {
        _ = await SeedCandidatoAsync("Maria Silva", "maria.silva@example.com");

        var command = new CreateCandidatoCommand(
            "Maria Santos",
            "maria.silva@example.com",
            "11888887777");

        await _host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/api/v1/candidatos");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("maria.silva@example.com");
        });

        Assert.Equal(1, await _repository.ContarAsync());
    }

    [Fact]
    public async Task GetCandidatosPorIdDeveRetornarCandidato()
    {
        var candidato = await SeedCandidatoAsync();

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/candidatos/{candidato.Id}");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain(candidato.Id.ToString());
            _.ContentShouldContain("Maria Silva");
        });
    }

    [Fact]
    public async Task GetCandidatosPorIdDeveRetornarNotFoundQuandoNaoExiste()
    {
        var candidatoId = Guid.Parse("bcfe2549-81f1-45e8-8b2b-92f7e3c5ce6a");

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/candidatos/{candidatoId}");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":404");
            _.ContentShouldContain($"/api/v1/candidatos/{candidatoId}");
        });
    }

    [Fact]
    public async Task GetCandidatosDeveListarCandidatosPaginados()
    {
        _ = await SeedCandidatoAsync("Ana Costa", "ana.costa@example.com");
        _ = await SeedCandidatoAsync("Bruno Lima", "bruno.lima@example.com");
        _ = await SeedCandidatoAsync("Carla Souza", "carla.souza@example.com");

        await _host.Scenario(_ =>
        {
            _.Get.Url("/api/v1/candidatos?pagina=1&tamanhoPagina=2");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("\"total\":3");
            _.ContentShouldContain("\"pagina\":1");
            _.ContentShouldContain("\"tamanhoPagina\":2");
            _.ContentShouldContain("Ana Costa");
            _.ContentShouldContain("Bruno Lima");
        });
    }

    [Fact]
    public async Task GetCandidatosDeveRetornarBadRequestQuandoPaginacaoInvalida()
    {
        await _host.Scenario(_ =>
        {
            _.Get.Url("/api/v1/candidatos?pagina=0&tamanhoPagina=10");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":400");
        });
    }

    [Fact]
    public async Task PutCandidatosDeveAtualizarCandidato()
    {
        var candidato = await SeedCandidatoAsync();
        var request = new AtualizarCandidatoRequest(
            "Maria Silva Atualizada",
            "maria.atualizada@example.com",
            "11777776666");

        await _host.Scenario(_ =>
        {
            _.Put.Json(request).ToUrl($"/api/v1/candidatos/{candidato.Id}");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("Maria Silva Atualizada");
            _.ContentShouldContain("maria.atualizada@example.com");
            _.ContentShouldContain("11777776666");
        });

        var atualizado = await _repository.ObterPorIdAsync(candidato.Id);

        Assert.NotNull(atualizado);
        Assert.Equal("Maria Silva Atualizada", atualizado.Nome);
        Assert.Equal("maria.atualizada@example.com", atualizado.Email.Value);
        Assert.Equal("11777776666", atualizado.Telefone.Value);
    }

    [Fact]
    public async Task PutCandidatosDeveRetornarConflictQuandoEmailPertenceAOutroCandidato()
    {
        var candidato = await SeedCandidatoAsync();
        _ = await SeedCandidatoAsync("Joao Souza", "joao.souza@example.com");
        var request = new AtualizarCandidatoRequest(
            "Maria Silva Atualizada",
            "joao.souza@example.com",
            "11777776666");

        await _host.Scenario(_ =>
        {
            _.Put.Json(request).ToUrl($"/api/v1/candidatos/{candidato.Id}");
            _.StatusCodeShouldBe(HttpStatusCode.Conflict);
            _.ContentTypeShouldBe("application/problem+json");
            _.ContentShouldContain("\"status\":409");
            _.ContentShouldContain("joao.souza@example.com");
        });

        var naoAtualizado = await _repository.ObterPorIdAsync(candidato.Id);

        Assert.NotNull(naoAtualizado);
        Assert.Equal("maria.silva@example.com", naoAtualizado.Email.Value);
    }

    [Fact]
    public async Task PostCurriculoDeveAdicionarCurriculoAoCandidato()
    {
        var candidato = await SeedCandidatoAsync();
        var request = new AdicionarCurriculoRequest(
            "curriculo.pdf",
            "application/pdf",
            "base64");

        await _host.Scenario(_ =>
        {
            _.Post.Json(request).ToUrl($"/api/v1/candidatos/{candidato.Id}/curriculo");
            _.StatusCodeShouldBeOk();
            _.ContentTypeShouldBe("application/json; charset=utf-8");
            _.ContentShouldContain("\"possuiCurriculo\":true");
            _.ContentShouldContain("\"nomeCurriculo\":\"curriculo.pdf\"");
        });

        var atualizado = await _repository.ObterPorIdAsync(candidato.Id);

        Assert.NotNull(atualizado);
        Assert.NotNull(atualizado.Curriculo);
        Assert.Equal("curriculo.pdf", atualizado.Curriculo.NomeArquivo);
    }

    [Fact]
    public async Task DeleteCandidatosDeveRemoverCandidato()
    {
        var candidato = await SeedCandidatoAsync();

        await _host.Scenario(_ =>
        {
            _.Delete.Url($"/api/v1/candidatos/{candidato.Id}");
            _.StatusCodeShouldBe(HttpStatusCode.NoContent);
            _.ContentShouldBe(string.Empty);
        });

        await _host.Scenario(_ =>
        {
            _.Get.Url($"/api/v1/candidatos/{candidato.Id}");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
            _.ContentTypeShouldBe("application/problem+json");
        });
    }

    private async Task<Candidato> SeedCandidatoAsync(
        string nome = "Maria Silva",
        string email = "maria.silva@example.com",
        string telefone = "11999998888")
    {
        var candidato = Candidato.Criar(nome, email, telefone);
        await _repository.AdicionarAsync(candidato);
        return candidato;
    }
}
