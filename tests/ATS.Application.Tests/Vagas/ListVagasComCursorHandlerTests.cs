namespace ATS.Application.Tests.Vagas;

using ATS.Application.Vagas.Queries.ListVagas;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;
using Moq;

public class ListVagasComCursorHandlerTests
{
    private readonly Mock<IVagaRepository> _repoMock = new(MockBehavior.Strict);
    private readonly ListVagasComCursorHandler _handler;

    public ListVagasComCursorHandlerTests()
        => _handler = new ListVagasComCursorHandler(_repoMock.Object);

    // ── Validação de limite ────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeveLancarExcecaoQuandoLimiteForMenorQueUm(int limite)
    {
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListVagasComCursorQuery(Limite: limite)));

        Assert.Equal("Limite deve estar entre 1 e 100.", ex.Message);
    }

    [Fact]
    public async Task DeveLancarExcecaoQuandoLimiteForMaiorQue100()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new ListVagasComCursorQuery(Limite: 101)));

        Assert.Equal("Limite deve estar entre 1 e 100.", ex.Message);
    }

    // ── Cursor parsing ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DevePassarAfterIdNuloQuandoCursorForVazioOuNulo(string? cursor)
    {
        Guid? afterIdPassado = Guid.NewGuid(); // valor sentinela — deve ser substituído por null

        _repoMock
            .Setup(r => r.ListarComCursorAsync(It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid?, int, CancellationToken>((id, _, _) => afterIdPassado = id)
            .ReturnsAsync([]);

        await _handler.HandleAsync(new ListVagasComCursorQuery(Cursor: cursor, Limite: 10));

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

        await _handler.HandleAsync(new ListVagasComCursorQuery(Cursor: "nao-e-base64-valido!!!", Limite: 10));

        Assert.Null(afterIdPassado);
    }

    [Fact]
    public async Task DeveDecodificarCursorValidoEPassarAfterId()
    {
        var idEsperado = Guid.NewGuid();
        var cursor = ListVagasComCursorHandler.EncodeCursor(idEsperado);
        Guid? afterIdPassado = null;

        _repoMock
            .Setup(r => r.ListarComCursorAsync(It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid?, int, CancellationToken>((id, _, _) => afterIdPassado = id)
            .ReturnsAsync([]);

        await _handler.HandleAsync(new ListVagasComCursorQuery(Cursor: cursor, Limite: 10));

        Assert.Equal(idEsperado, afterIdPassado);
    }

    // ── Paginação: sem próxima página ──────────────────────────────────────

    [Fact]
    public async Task DeveRetornarTemMaisFalsoEProximoCursorNuloQuandoItensForemMenoresOuIguaisAoLimite()
    {
        var vagas = new[] { CriarVaga(), CriarVaga() };

        _repoMock
            .Setup(r => r.ListarComCursorAsync(null, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vagas); // 2 < limite+1=3

        var result = await _handler.HandleAsync(new ListVagasComCursorQuery(Limite: 2));

        Assert.Equal(2, result.Items.Count());
        Assert.False(result.TemMais);
        Assert.Null(result.ProximoCursor);
    }

    // ── Paginação: com próxima página ──────────────────────────────────────

    [Fact]
    public async Task DeveRetornarTemMaisTrueEProximoCursorQuandoHouverMaisItens()
    {
        var limite = 2;
        var vagas = new[] { CriarVaga(), CriarVaga(), CriarVaga() }; // limite+1 itens

        _repoMock
            .Setup(r => r.ListarComCursorAsync(null, limite + 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vagas);

        var result = await _handler.HandleAsync(new ListVagasComCursorQuery(Limite: limite));

        Assert.Equal(limite, result.Items.Count());
        Assert.True(result.TemMais);
        Assert.NotNull(result.ProximoCursor);
        // O cursor deve codificar o id do último item retornado
        var ultimoItem = result.Items.Last();
        Assert.Equal(result.ProximoCursor, ListVagasComCursorHandler.EncodeCursor(ultimoItem.Id));
    }

    // ── Filtro por status ──────────────────────────────────────────────────

    [Fact]
    public async Task DeveAplicarFiltroDeStatusQuandoInformado()
    {
        var vagaAberta = CriarVaga();
        var vagaFechada = CriarVaga();
        vagaFechada.Fechar();

        _repoMock
            .Setup(r => r.ListarComCursorAsync(null, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { vagaAberta, vagaFechada });

        var result = await _handler.HandleAsync(
            new ListVagasComCursorQuery(Limite: 10, Status: StatusVaga.Aberta));

        var items = result.Items.ToList();
        Assert.Single(items);
        Assert.Equal(StatusVaga.Aberta, items[0].Status);
    }

    [Fact]
    public async Task NaoDeveAplicarFiltroDeStatusQuandoNaoInformado()
    {
        var vagaAberta = CriarVaga();
        var vagaFechada = CriarVaga();
        vagaFechada.Fechar();

        _repoMock
            .Setup(r => r.ListarComCursorAsync(null, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { vagaAberta, vagaFechada });

        var result = await _handler.HandleAsync(
            new ListVagasComCursorQuery(Limite: 10, Status: null));

        Assert.Equal(2, result.Items.Count());
    }

    // ── Resultado vazio ────────────────────────────────────────────────────

    [Fact]
    public async Task DeveRetornarResultadoVazioQuandoNaoHouverVagas()
    {
        _repoMock
            .Setup(r => r.ListarComCursorAsync(null, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new ListVagasComCursorQuery(Limite: 10));

        Assert.Empty(result.Items);
        Assert.False(result.TemMais);
        Assert.Null(result.ProximoCursor);
    }

    // ── EncodeCursor ───────────────────────────────────────────────────────

    [Fact]
    public void EncodeCursorDeveRetornarBase64DoGuid()
    {
        var id = Guid.NewGuid();
        var cursor = ListVagasComCursorHandler.EncodeCursor(id);

        var bytes = Convert.FromBase64String(cursor);
        Assert.Equal(id, new Guid(bytes));
    }

    private static Vaga CriarVaga() =>
        Vaga.Criar("Dev", "Descrição", "Req", 5000m);
}
