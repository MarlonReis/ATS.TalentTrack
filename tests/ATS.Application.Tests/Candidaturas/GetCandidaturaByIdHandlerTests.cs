using ATS.Application.Candidaturas.Queries.GetCandidaturaById;
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

public class GetCandidaturaByIdHandlerTests
{
    private readonly Mock<ICandidaturaRepository> _candidaturaRepoMock;
    private readonly Mock<ICandidatoRepository> _candidatoRepoMock;
    private readonly Mock<IVagaRepository> _vagaRepoMock;
    private readonly GetCandidaturaByIdHandler _handler;

    private static readonly Guid _guidCandidatura = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid _guidCandidato = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _guidVaga = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public GetCandidaturaByIdHandlerTests()
    {
        _candidaturaRepoMock = new Mock<ICandidaturaRepository>(MockBehavior.Strict);
        _candidatoRepoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _vagaRepoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new GetCandidaturaByIdHandler(
            _candidaturaRepoMock.Object,
            _candidatoRepoMock.Object,
            _vagaRepoMock.Object);
    }

    private static Candidatura CriarCandidatura() => Candidatura.Criar(_guidCandidato, _guidVaga);

    private static Candidato CriarCandidato(
        string nome = "João Silva",
        string email = "joao@email.com",
        string tel = "11912345678") =>
        Candidato.Criar(nome, email, tel);

    private static Vaga CriarVaga(string titulo = "Dev Back-end") =>
        Vaga.Criar(titulo, "Descrição", "Requisitos", 12000);

    private void SetupFluxoCompleto(
        Candidatura candidatura, Candidato candidato, Vaga vaga)
    {
        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678", "Dev Back-end")]
    [InlineData("Maria Santos", "maria@empresa.com.br", "21987654321", "Tech Lead")]
    public async Task DeveRetornarDtoDetalhadoComTodosOsDados(
        string nomeCandidato, string email, string telefone, string tituloVaga)
    {
        var candidatura = CriarCandidatura();
        var candidato = CriarCandidato(nomeCandidato, email, telefone);
        var vaga = CriarVaga(tituloVaga);

        SetupFluxoCompleto(candidatura, candidato, vaga);

        var dto = await _handler.HandleAsync(new GetCandidaturaByIdQuery(_guidCandidatura));

        // Dados da candidatura
        Assert.Equal(candidatura.Id, dto.Id);
        Assert.Equal(_guidVaga, dto.VagaId);
        Assert.Equal(StatusCandidatura.EmAnalise, dto.Status);
        Assert.Equal("Em Análise", dto.StatusDescricao);
        Assert.Null(dto.Observacoes);

        // Dados do candidato
        Assert.Equal(candidato.Id, dto.CandidatoId);
        Assert.Equal(nomeCandidato, dto.NomeCandidato);
        Assert.Equal(email, dto.EmailCandidato);
        Assert.Equal(telefone, dto.TelefoneCandidato);
        Assert.False(dto.PossuiCurriculo);
        Assert.Null(dto.NomeCurriculo);

        // Dados da vaga
        Assert.Equal(tituloVaga, dto.TituloVaga);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    public async Task DeveRetornarStatusDescricaoCorretoParaCadaStatus(string tituloVaga)
    {
        // Testa Aprovado
        var candidaturaAprovada = CriarCandidatura();
        candidaturaAprovada.Aprovar("aprovado");

        SetupFluxoCompleto(candidaturaAprovada, CriarCandidato(), CriarVaga(tituloVaga));

        var dto = await _handler.HandleAsync(new GetCandidaturaByIdQuery(_guidCandidatura));

        Assert.Equal(StatusCandidatura.Aprovado, dto.Status);
        Assert.Equal("Aprovado", dto.StatusDescricao);
        Assert.Equal("aprovado", dto.Observacoes);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveConsultarNaOrdemCandidaturaDepoisCandidatoDepoisVaga(string _)
    {
        var ordemChamadas = new List<string>();
        var candidatura = CriarCandidatura();

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura)
            .Callback(() => ordemChamadas.Add("ObterCandidatura"));

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidato())
            .Callback(() => ordemChamadas.Add("ObterCandidato"));

        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarVaga())
            .Callback(() => ordemChamadas.Add("ObterVaga"));

        await _handler.HandleAsync(new GetCandidaturaByIdQuery(_guidCandidatura));

        Assert.Equal(
            new[] { "ObterCandidatura", "ObterCandidato", "ObterVaga" },
            ordemChamadas);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveRepassarCancellationTokenParaTodosOsRepositorios(string _)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var candidatura = CriarCandidatura();

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, ct)).ReturnsAsync(candidatura);
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, ct)).ReturnsAsync(CriarCandidato());
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, ct)).ReturnsAsync(CriarVaga());

        await _handler.HandleAsync(new GetCandidaturaByIdQuery(_guidCandidatura), ct);

        _candidaturaRepoMock.Verify(r => r.ObterPorIdAsync(_guidCandidatura, ct), Times.Once);
        _candidatoRepoMock.Verify(r => r.ObterPorIdAsync(_guidCandidato, ct), Times.Once);
        _vagaRepoMock.Verify(r => r.ObterPorIdAsync(_guidVaga, ct), Times.Once);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoCandidaturaNaoForEncontrada(string _)
    {
        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidatura?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new GetCandidaturaByIdQuery(_guidCandidatura)));

        Assert.Equal("Candidatura não encontrada.", excecao.Message);
        _candidatoRepoMock.Verify(
            r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoCandidatoNaoForEncontrado(string _)
    {
        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidatura());
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new GetCandidaturaByIdQuery(_guidCandidatura)));

        Assert.Equal("Candidato vinculado à candidatura não encontrado.", excecao.Message);
        _vagaRepoMock.Verify(
            r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontrada(string _)
    {
        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidatura());
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidato());
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vaga?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new GetCandidaturaByIdQuery(_guidCandidatura)));

        Assert.Equal("Vaga vinculada à candidatura não encontrada.", excecao.Message);
    }
}
