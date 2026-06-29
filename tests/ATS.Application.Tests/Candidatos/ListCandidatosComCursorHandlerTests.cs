namespace ATS.Application.Tests.Candidatos;

using ATS.Application.Candidatos.Queries.ListCandidatos;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Moq;

public class ListCandidatosComCursorHandlerTests
{
    private readonly Mock<ICandidatoRepository> _repoMock = new(MockBehavior.Strict);
    private readonly ListCandidatosComCursorHandler _handler;

    public ListCandidatosComCursorHandlerTests()
        => _handler = new ListCandidatosComCursorHandler(_repoMock.Object);

    // ── Validação de limite ────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task DeveLancarExcecaoQuandoLimiteForMenorQueUm(int limite)
    {
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListCandidatosComCursorQuery(Limite: limite)));

        Assert.Equal("Limite deve estar entre 1 e 100.", ex.Message);
    }

    [Fact]
    public async Task DeveLancarExcecaoQuandoLimiteForMaiorQue100()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListCandidatosComCursorQuery(Limite: 101)));

        Assert.Equal("Limite deve estar entre 1 e 100.", ex.Message);
    }

    // ── Cursor parsing ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DevePassarAfterIdNuloQuandoCursorForVazioOuNulo(string? cursor)
    {
        Guid? afterIdPassado = Guid.NewGuid();

        _repoMock
            .Setup(r => r.ListarComCursorAsync(It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid?, int, CancellationToken>((id, _, _) => afterIdPassado = id)
            .ReturnsAsync([]);

        await _handler.HandleAsync(new ListCandidatosComCursorQuery(Cursor: cursor, Limite: 10));

        Assert.Null(afterIdPassado);
    }

    [Fact]
    public async Task DevePassarAfterIdNuloQuandoCursorForBase64Invalido()
    {
        Guid? afterIdPassado = Guid.NewGuid();

        _repoMock
            .Setup(r => r.ListarComCursorAsync(It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid?, int, CancellationToken>((id, _, _) => afterIdPassado = id)
            .ReturnsAsync([]);

        await _handler.HandleAsync(new ListCandidatosComCursorQuery(Cursor: "!!!invalido!!!", Limite: 10));

        Assert.Null(afterIdPassado);
    }

    [Fact]
    public async Task DeveDecodificarCursorValidoEPassarAfterId()
    {
        var idEsperado = Guid.NewGuid();
        var cursor = ListCandidatosComCursorHandler.EncodeCursor(idEsperado);
        Guid? afterIdPassado = null;

        _repoMock
            .Setup(r => r.ListarComCursorAsync(It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid?, int, CancellationToken>((id, _, _) => afterIdPassado = id)
            .ReturnsAsync([]);

        await _handler.HandleAsync(new ListCandidatosComCursorQuery(Cursor: cursor, Limite: 10));

        Assert.Equal(idEsperado, afterIdPassado);
    }

    // ── Paginação: sem próxima página ──────────────────────────────────────

    [Fact]
    public async Task DeveRetornarTemMaisFalsoEProximoCursorNuloQuandoItensForemMenoresOuIguaisAoLimite()
    {
        var candidatos = new[] { CriarCandidato(), CriarCandidato() };

        _repoMock
            .Setup(r => r.ListarComCursorAsync(null, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatos);

        var result = await _handler.HandleAsync(new ListCandidatosComCursorQuery(Limite: 2));

        Assert.Equal(2, result.Items.Count());
        Assert.False(result.TemMais);
        Assert.Null(result.ProximoCursor);
    }

    // ── Paginação: com próxima página ──────────────────────────────────────

    [Fact]
    public async Task DeveRetornarTemMaisTrueEProximoCursorQuandoHouverMaisItens()
    {
        var limite = 2;
        var candidatos = new[] { CriarCandidato(), CriarCandidato(), CriarCandidato() };

        _repoMock
            .Setup(r => r.ListarComCursorAsync(null, limite + 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatos);

        var result = await _handler.HandleAsync(new ListCandidatosComCursorQuery(Limite: limite));

        Assert.Equal(limite, result.Items.Count());
        Assert.True(result.TemMais);
        Assert.NotNull(result.ProximoCursor);
        var ultimoItem = result.Items.Last();
        Assert.Equal(result.ProximoCursor, ListCandidatosComCursorHandler.EncodeCursor(ultimoItem.Id));
    }

    // ── Resultado vazio ────────────────────────────────────────────────────

    [Fact]
    public async Task DeveRetornarResultadoVazioQuandoNaoHouverCandidatos()
    {
        _repoMock
            .Setup(r => r.ListarComCursorAsync(null, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new ListCandidatosComCursorQuery(Limite: 10));

        Assert.Empty(result.Items);
        Assert.False(result.TemMais);
        Assert.Null(result.ProximoCursor);
    }

    // ── EncodeCursor ───────────────────────────────────────────────────────

    [Fact]
    public void EncodeCursorDeveRetornarBase64DoGuid()
    {
        var id = Guid.NewGuid();
        var cursor = ListCandidatosComCursorHandler.EncodeCursor(id);

        var bytes = Convert.FromBase64String(cursor);
        Assert.Equal(id, new Guid(bytes));
    }

    private static Candidato CriarCandidato(
        string email = "cand@example.com") =>
        Candidato.Criar("Candidato Teste", email, "11999990000");
}
