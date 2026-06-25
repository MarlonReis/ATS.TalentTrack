using ATS.Application.Candidaturas.Queries.ListCandidatosPorVaga;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Candidaturas;

public class ListCandidatosPorVagaHandlerTests
{

    private readonly Mock<ICandidaturaRepository> _candidaturaRepoMock;
    private readonly Mock<ICandidatoRepository> _candidatoRepoMock;
    private readonly Mock<IVagaRepository> _vagaRepoMock;
    private readonly ListCandidatosPorVagaHandler _handler;

    private static readonly Guid _guidVaga = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public ListCandidatosPorVagaHandlerTests()
    {
        _candidaturaRepoMock = new Mock<ICandidaturaRepository>(MockBehavior.Strict);
        _candidatoRepoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _vagaRepoMock = new Mock<IVagaRepository>(MockBehavior.Strict);

        _handler = new ListCandidatosPorVagaHandler(
            _candidaturaRepoMock.Object,
            _candidatoRepoMock.Object,
            _vagaRepoMock.Object);
    }


    private static Vaga CriarVaga(string titulo = "Dev Back-end Sênior") =>
        Vaga.Criar(titulo, "Descrição da vaga", "Requisitos", 12000);

    private static Candidato CriarCandidato(
        string nome = "João da Silva",
        string email = "joao@email.com",
        string tel = "11912345678") =>
        Candidato.Criar(nome, email, tel);

    private static Candidatura CriarCandidatura(Guid candidatoId, Guid vagaId) =>
        Candidatura.Criar(candidatoId, vagaId);

    private void SetupVagaEncontrada(Guid vagaId, Vaga vaga) =>
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);

    private void SetupVagaNaoEncontrada(Guid vagaId) =>
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vaga?)null);

    private void SetupCandidaturas(Guid vagaId, IEnumerable<Candidatura> candidaturas) =>
        _candidaturaRepoMock
            .Setup(r => r.ListarPorVagaAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidaturas);

    private void SetupCandidatoEncontrado(Guid candidatoId, Candidato candidato) =>
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(candidatoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);

    private void SetupCandidatoNaoEncontrado(Guid candidatoId) =>
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(candidatoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);





    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontrada(string vagaIdStr)
    {

        var vagaId = Guid.Parse(vagaIdStr);
        SetupVagaNaoEncontrada(vagaId);

        var query = new ListCandidatosPorVagaQuery(vagaId);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(query));


        Assert.Equal("Vaga não encontrada.", excecao.Message);
    }

    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveNaoListarCandidaturasQuandoVagaNaoExistir(string vagaIdStr)
    {

        var vagaId = Guid.Parse(vagaIdStr);
        SetupVagaNaoEncontrada(vagaId);

        var query = new ListCandidatosPorVagaQuery(vagaId);


        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(query));


        _candidaturaRepoMock.Verify(
            r => r.ListarPorVagaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }





    [Theory]
    [InlineData("Dev Back-end Sênior")]
    [InlineData("Tech Lead")]
    public async Task DeveRetornarListaVaziaQuandoNaoHouverCandidaturas(string tituloVaga)
    {

        var vaga = CriarVaga(tituloVaga);
        SetupVagaEncontrada(_guidVaga, vaga);
        SetupCandidaturas(_guidVaga, Enumerable.Empty<Candidatura>());

        var query = new ListCandidatosPorVagaQuery(_guidVaga);


        var resultado = await _handler.HandleAsync(query);


        Assert.Empty(resultado);
    }

    [Theory]
    [InlineData("Dev Back-end Sênior")]
    public async Task DeveNaoConsultarCandidatosQuandoListaDeCandidaturasForVazia(
        string tituloVaga)
    {

        SetupVagaEncontrada(_guidVaga, CriarVaga(tituloVaga));
        SetupCandidaturas(_guidVaga, Enumerable.Empty<Candidatura>());

        var query = new ListCandidatosPorVagaQuery(_guidVaga);


        await _handler.HandleAsync(query);


        _candidatoRepoMock.Verify(
            r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }





    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678", "Dev Back-end Sênior")]
    [InlineData("Maria Santos", "maria@empresa.com.br", "21987654321", "Tech Lead")]
    public async Task DeveRetornarDtoComTodosOsDadosDaCandidatura(
        string nomeCandidato, string email, string telefone, string tituloVaga)
    {

        var candidato = CriarCandidato(nomeCandidato, email, telefone);
        var vaga = CriarVaga(tituloVaga);
        var candidatura = CriarCandidatura(candidato.Id, _guidVaga);

        SetupVagaEncontrada(_guidVaga, vaga);
        SetupCandidaturas(_guidVaga, new[] { candidatura });
        SetupCandidatoEncontrado(candidato.Id, candidato);

        var query = new ListCandidatosPorVagaQuery(_guidVaga);


        var resultado = (await _handler.HandleAsync(query)).ToList();


        var dto = Assert.Single(resultado);

        Assert.Equal(candidatura.Id, dto.Id);
        Assert.Equal(candidato.Id, dto.CandidatoId);
        Assert.Equal(candidato.Nome, dto.NomeCandidato);
        Assert.Equal(candidato.Email.Value, dto.EmailCandidato);
        Assert.Equal(candidato.Telefone.Value, dto.TelefoneCandidato);
        Assert.False(dto.PossuiCurriculo);
        Assert.Null(dto.NomeCurriculo);
        Assert.Equal(_guidVaga, dto.VagaId);
        Assert.Equal(vaga.Titulo, dto.TituloVaga);
        Assert.Equal(StatusCandidatura.EmAnalise, dto.Status);
        Assert.Equal("Em Análise", dto.StatusDescricao);
        Assert.Null(dto.Observacoes);
    }


    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task DeveRetornarUmDtoPorCandidaturaEncontrada(int quantidadeCandidatos)
    {

        var vaga = CriarVaga();
        SetupVagaEncontrada(_guidVaga, vaga);

        var candidatos = Enumerable.Range(1, quantidadeCandidatos)
            .Select(i => CriarCandidato($"Candidato {i}", $"c{i}@email.com", "11912345678"))
            .ToList();

        var candidaturas = candidatos
            .Select(c => CriarCandidatura(c.Id, _guidVaga))
            .ToList();

        SetupCandidaturas(_guidVaga, candidaturas);

        foreach (var candidato in candidatos)
        {
            SetupCandidatoEncontrado(candidato.Id, candidato);
        }

        var query = new ListCandidatosPorVagaQuery(_guidVaga);


        var resultado = (await _handler.HandleAsync(query)).ToList();


        Assert.Equal(quantidadeCandidatos, resultado.Count);
    }

    [Theory]
    [InlineData("Dev Back-end Sênior")]
    public async Task DeveUsarTituloCorretoEmTodosOsDtos(string tituloVaga)
    {

        var vaga = CriarVaga(tituloVaga);
        var candidatoA = CriarCandidato("Ana Lima", "ana@email.com", "11911111111");
        var candidatoB = CriarCandidato("Bruno Cruz", "bruno@email.com", "11922222222");

        SetupVagaEncontrada(_guidVaga, vaga);
        SetupCandidaturas(_guidVaga, new[]
        {
            CriarCandidatura(candidatoA.Id, _guidVaga),
            CriarCandidatura(candidatoB.Id, _guidVaga)
        });
        SetupCandidatoEncontrado(candidatoA.Id, candidatoA);
        SetupCandidatoEncontrado(candidatoB.Id, candidatoB);

        var query = new ListCandidatosPorVagaQuery(_guidVaga);


        var resultado = (await _handler.HandleAsync(query)).ToList();


        Assert.All(resultado, dto => Assert.Equal(tituloVaga, dto.TituloVaga));
    }





    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveIgnorarCandidaturaQuandoCandidatoNaoForEncontrado(
        string vagaIdStr)
    {

        var vagaId = Guid.Parse(vagaIdStr);
        var candidatoId = Guid.NewGuid();
        var candidatura = CriarCandidatura(candidatoId, vagaId);

        SetupVagaEncontrada(vagaId, CriarVaga());
        SetupCandidaturas(vagaId, new[] { candidatura });
        SetupCandidatoNaoEncontrado(candidatoId);

        var query = new ListCandidatosPorVagaQuery(vagaId);


        var resultado = await _handler.HandleAsync(query);


        Assert.Empty(resultado);
    }

    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveRetornarApenasCandidatosEncontradosQuandoAlgunsEstiremAusentes(
        string vagaIdStr)
    {

        var vagaId = Guid.Parse(vagaIdStr);
        var vaga = CriarVaga();

        var candidatoA = CriarCandidato("Ana Lima", "ana@email.com", "11911111111");
        var candidatoC = CriarCandidato("Carlos Melo", "carlos@email.com", "11933333333");
        var candidatoIdAusente = Guid.NewGuid();

        var candidaturas = new[]
        {
            CriarCandidatura(candidatoA.Id,         vagaId),
            CriarCandidatura(candidatoIdAusente,    vagaId),
            CriarCandidatura(candidatoC.Id,         vagaId)
        };

        SetupVagaEncontrada(vagaId, vaga);
        SetupCandidaturas(vagaId, candidaturas);
        SetupCandidatoEncontrado(candidatoA.Id, candidatoA);
        SetupCandidatoNaoEncontrado(candidatoIdAusente);
        SetupCandidatoEncontrado(candidatoC.Id, candidatoC);

        var query = new ListCandidatosPorVagaQuery(vagaId);


        var resultado = (await _handler.HandleAsync(query)).ToList();


        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, dto => dto.NomeCandidato == candidatoA.Nome);
        Assert.Contains(resultado, dto => dto.NomeCandidato == candidatoC.Nome);
    }





    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveRepassarCancellationTokenParaTodosOsRepositorios(
        string vagaIdStr)
    {

        var vagaId = Guid.Parse(vagaIdStr);
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        var candidato = CriarCandidato();
        var vaga = CriarVaga();
        var candidatura = CriarCandidatura(candidato.Id, vagaId);

        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(vagaId, ct))
            .ReturnsAsync(vaga);

        _candidaturaRepoMock
            .Setup(r => r.ListarPorVagaAsync(vagaId, ct))
            .ReturnsAsync(new[] { candidatura });

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(candidato.Id, ct))
            .ReturnsAsync(candidato);

        var query = new ListCandidatosPorVagaQuery(vagaId);


        await _handler.HandleAsync(query, ct);


        _vagaRepoMock.Verify(r => r.ObterPorIdAsync(vagaId, ct), Times.Once);
        _candidaturaRepoMock.Verify(r => r.ListarPorVagaAsync(vagaId, ct), Times.Once);
        _candidatoRepoMock.Verify(r => r.ObterPorIdAsync(candidato.Id, ct), Times.Once);
    }





    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveConsultarVagaAntesDeListarCandidaturas(string vagaIdStr)
    {

        var vagaId = Guid.Parse(vagaIdStr);
        var ordemChamadas = new List<string>();

        var candidato = CriarCandidato();
        var candidatura = CriarCandidatura(candidato.Id, vagaId);

        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarVaga())
            .Callback(() => ordemChamadas.Add("ObterVaga"));

        _candidaturaRepoMock
            .Setup(r => r.ListarPorVagaAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { candidatura })
            .Callback(() => ordemChamadas.Add("ListarCandidaturas"));

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(candidato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato)
            .Callback(() => ordemChamadas.Add("ObterCandidato"));

        var query = new ListCandidatosPorVagaQuery(vagaId);


        await _handler.HandleAsync(query);


        Assert.Equal(
            new[] { "ObterVaga", "ListarCandidaturas", "ObterCandidato" },
            ordemChamadas);
    }
}
