using ATS.Application.Candidaturas.Commands.AprovarCandidatura;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ATS.Application.Tests.Candidaturas;

public class AprovarCandidaturaHandlerTests
{
    private readonly Mock<ICandidaturaRepository> _candidaturaRepoMock;
    private readonly Mock<ICandidatoRepository> _candidatoRepoMock;
    private readonly Mock<IVagaRepository> _vagaRepoMock;
    private readonly AprovarCandidaturaHandler _handler;

    private static readonly Guid _guidCandidatura = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid _guidCandidato = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _guidVaga = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public AprovarCandidaturaHandlerTests()
    {
        _candidaturaRepoMock = new Mock<ICandidaturaRepository>(MockBehavior.Strict);
        _candidatoRepoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _vagaRepoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _handler = new AprovarCandidaturaHandler(
            _candidaturaRepoMock.Object,
            _candidatoRepoMock.Object,
            _vagaRepoMock.Object,
            NullLogger<AprovarCandidaturaHandler>.Instance);
    }

    private static Candidatura CriarCandidatura() =>
        Candidatura.Criar(_guidCandidato, _guidVaga);

    private static Candidato CriarCandidato(string nome = "João Silva") =>
        Candidato.Criar(nome, "joao@email.com", "11912345678");

    private static Vaga CriarVaga(string titulo = "Dev Back-end") =>
        Vaga.Criar(titulo, "Descrição", "Requisitos", 12000);

    private void SetupFluxoCompleto(
        Candidatura candidatura,
        Candidato candidato,
        Vaga vaga)
    {
        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);

        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);

        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Perfil excelente para a vaga.")]
    public async Task DeveAprovarCandidaturaERetornarDto(string? observacoes)
    {
        var candidatura = CriarCandidatura();
        var candidato = CriarCandidato();
        var vaga = CriarVaga();

        SetupFluxoCompleto(candidatura, candidato, vaga);

        var command = new AprovarCandidaturaCommand(_guidCandidatura, observacoes);

        var dto = await _handler.HandleAsync(command);

        Assert.Equal(StatusCandidatura.Aprovado, dto.Status);
        Assert.Equal("Aprovado", dto.StatusDescricao);
        Assert.Equal(observacoes, dto.Observacoes);
    }

    [Theory]
    [InlineData("João Silva", "Dev Back-end")]
    [InlineData("Maria Santos", "Tech Lead")]
    public async Task DeveRetornarDtoComDadosDeCandidatoEVaga(
        string nomeCandidato, string tituloVaga)
    {
        var candidatura = CriarCandidatura();
        var candidato = CriarCandidato(nomeCandidato);
        var vaga = CriarVaga(tituloVaga);

        SetupFluxoCompleto(candidatura, candidato, vaga);

        var command = new AprovarCandidaturaCommand(_guidCandidatura);

        var dto = await _handler.HandleAsync(command);

        Assert.Equal(_guidCandidato, dto.CandidatoId);
        Assert.Equal(_guidVaga, dto.VagaId);
        Assert.Equal(nomeCandidato, dto.NomeCandidato);
        Assert.Equal(tituloVaga, dto.TituloVaga);
    }

    [Fact]
    public async Task DeveChamarAtualizarAsyncUmaVez()
    {
        var candidatura = CriarCandidatura();
        SetupFluxoCompleto(candidatura, CriarCandidato(), CriarVaga());

        await _handler.HandleAsync(new AprovarCandidaturaCommand(_guidCandidatura));

        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeveRepassarCancellationTokenParaTodosOsRepositorios()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var candidatura = CriarCandidatura();

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, ct)).ReturnsAsync(candidatura);
        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, ct)).Returns(Task.CompletedTask);
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, ct)).ReturnsAsync(CriarCandidato());
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, ct)).ReturnsAsync(CriarVaga());

        await _handler.HandleAsync(new AprovarCandidaturaCommand(_guidCandidatura), ct);

        _candidaturaRepoMock.Verify(r => r.ObterPorIdAsync(_guidCandidatura, ct), Times.Once);
        _candidaturaRepoMock.Verify(r => r.AtualizarAsync(candidatura, ct), Times.Once);
        _candidatoRepoMock.Verify(r => r.ObterPorIdAsync(_guidCandidato, ct), Times.Once);
        _vagaRepoMock.Verify(r => r.ObterPorIdAsync(_guidVaga, ct), Times.Once);
    }


    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveExecutarOperacoesNaOrdemCorreta(string _)
    {
        var candidatura = CriarCandidatura();
        var ordemChamadas = new List<string>();

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura)
            .Callback(() => ordemChamadas.Add("ObterCandidatura"));

        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => ordemChamadas.Add("AtualizarCandidatura"));

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidato())
            .Callback(() => ordemChamadas.Add("ObterCandidato"));

        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarVaga())
            .Callback(() => ordemChamadas.Add("ObterVaga"));

        await _handler.HandleAsync(new AprovarCandidaturaCommand(_guidCandidatura));

        // Atualizar deve ocorrer ANTES de buscar candidato e vaga
        Assert.Equal(
            new[] { "ObterCandidatura", "AtualizarCandidatura", "ObterCandidato", "ObterVaga" },
            ordemChamadas);
    }


    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoCandidaturaNaoForEncontrada(string _)
    {
        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidatura?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new AprovarCandidaturaCommand(_guidCandidatura)));

        Assert.Equal("Candidatura não encontrada.", excecao.Message);
        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidatura>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoCandidaturaJaEstiverAprovada(string _)
    {
        var candidatura = CriarCandidatura();
        candidatura.Aprovar("já aprovada");

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new AprovarCandidaturaCommand(_guidCandidatura)));

        Assert.Equal(
            "Somente candidaturas 'Em Análise' podem ser aprovadas.",
            excecao.Message);
        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidatura>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoCandidaturaEstiverReprovadaOuCancelada(string _)
    {
        var candidaturaReprovada = CriarCandidatura();
        candidaturaReprovada.Reprovar();

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidaturaReprovada);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new AprovarCandidaturaCommand(_guidCandidatura)));

        Assert.Equal(
            "Somente candidaturas 'Em Análise' podem ser aprovadas.",
            excecao.Message);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoCandidatoNaoForEncontradoAposAtualizar(string _)
    {
        var candidatura = CriarCandidatura();

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);
        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new AprovarCandidaturaCommand(_guidCandidatura)));

        Assert.Equal("Candidato vinculado à candidatura não encontrado.", excecao.Message);

        // A candidatura JÁ foi atualizada no banco (aprovação persiste)
        _candidaturaRepoMock.Verify(
            r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontradaAposAtualizar(string _)
    {
        var candidatura = CriarCandidatura();

        _candidaturaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidatura, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);
        _candidaturaRepoMock
            .Setup(r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidato());
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(_guidVaga, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vaga?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new AprovarCandidaturaCommand(_guidCandidatura)));

        Assert.Equal("Vaga vinculada à candidatura não encontrada.", excecao.Message);
    }
}
