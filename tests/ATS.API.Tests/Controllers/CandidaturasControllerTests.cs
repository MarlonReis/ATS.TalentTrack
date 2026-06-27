using ATS.API.Controllers;
using ATS.API.Requests.Candidaturas;
using ATS.Application.Candidaturas.Commands.AprovarCandidatura;
using ATS.Application.Candidaturas.Commands.CancelarCandidatura;
using ATS.Application.Candidaturas.Commands.CandidatarSe;
using ATS.Application.Candidaturas.Commands.ReprovarCandidatura;
using ATS.Application.Candidaturas.DTOs;
using ATS.Application.Candidaturas.Queries.GetCandidaturaById;
using ATS.Application.Candidaturas.Queries.ListCandidatosPorVaga;
using ATS.Application.Common.Events;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ATS.API.Tests.Controllers;

public class CandidaturasControllerTests
{
    private readonly Mock<ICandidaturaRepository> _candidaturaRepositoryMock;
    private readonly Mock<ICandidatoRepository> _candidatoRepositoryMock;
    private readonly Mock<IVagaRepository> _vagaRepositoryMock;
    private readonly Mock<IDomainEventDispatcher> _dispatcherMock;
    private readonly CandidaturasController _controller;

    public CandidaturasControllerTests()
    {
        _candidaturaRepositoryMock = new Mock<ICandidaturaRepository>(MockBehavior.Strict);
        _candidatoRepositoryMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _vagaRepositoryMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _dispatcherMock = new Mock<IDomainEventDispatcher>();
        _dispatcherMock
            .Setup(d => d.DispatchAndClearAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _controller = new CandidaturasController(
            new CandidatarSeHandler(
                _candidaturaRepositoryMock.Object,
                _candidatoRepositoryMock.Object,
                _vagaRepositoryMock.Object,
                _dispatcherMock.Object,
                NullLogger<CandidatarSeHandler>.Instance),
            new GetCandidaturaByIdHandler(
                _candidaturaRepositoryMock.Object,
                _candidatoRepositoryMock.Object,
                _vagaRepositoryMock.Object),
            new AprovarCandidaturaHandler(
                _candidaturaRepositoryMock.Object,
                _candidatoRepositoryMock.Object,
                _vagaRepositoryMock.Object,
                _dispatcherMock.Object,
                NullLogger<AprovarCandidaturaHandler>.Instance),
            new ReprovarCandidaturaHandler(
                _candidaturaRepositoryMock.Object,
                _candidatoRepositoryMock.Object,
                _vagaRepositoryMock.Object,
                _dispatcherMock.Object,
                NullLogger<ReprovarCandidaturaHandler>.Instance),
            new CancelarCandidaturaHandler(
                _candidaturaRepositoryMock.Object,
                _candidatoRepositoryMock.Object,
                _vagaRepositoryMock.Object,
                _dispatcherMock.Object,
                NullLogger<CancelarCandidaturaHandler>.Instance),
            new ListCandidatosPorVagaHandler(
                _candidaturaRepositoryMock.Object,
                _candidatoRepositoryMock.Object,
                _vagaRepositoryMock.Object));
    }

    [Fact]
    public async Task CandidatarSeDeveRetornarCreatedAtActionComDto()
    {
        var candidato = CriarCandidato();
        var vaga = CriarVaga();
        Candidatura? candidaturaAdicionada = null;

        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _vagaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        _candidaturaRepositoryMock
            .Setup(r => r.ExisteAsync(candidato.Id, vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _candidaturaRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Candidatura>(), It.IsAny<CancellationToken>()))
            .Callback<Candidatura, CancellationToken>((candidatura, _) =>
                candidaturaAdicionada = candidatura)
            .Returns(Task.CompletedTask);

        var result = await _controller.CandidatarSe(
            new CandidatarSeCommand(candidato.Id, vaga.Id));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<CandidaturaDto>(created.Value);

        Assert.Equal(nameof(CandidaturasController.ObterPorId), created.ActionName);
        Assert.NotNull(created.RouteValues);
        Assert.Equal(dto.Id, created.RouteValues["id"]);
        Assert.Equal(candidato.Id, dto.CandidatoId);
        Assert.Equal(candidato.Nome, dto.NomeCandidato);
        Assert.Equal(vaga.Id, dto.VagaId);
        Assert.Equal(vaga.Titulo, dto.TituloVaga);
        Assert.Equal(StatusCandidatura.EmAnalise, dto.Status);
        Assert.NotNull(candidaturaAdicionada);
    }

    [Fact]
    public async Task ObterPorIdDeveRetornarOkComCandidaturaDetalhada()
    {
        var candidato = CriarCandidato();
        var vaga = CriarVaga();
        var candidatura = CriarCandidatura(candidato.Id, vaga.Id);

        SetupCandidaturaComRelacionamentos(candidatura, candidato, vaga);

        var result = await _controller.ObterPorId(candidatura.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidaturaDetalhadaDto>(ok.Value);

        Assert.Equal(candidatura.Id, dto.Id);
        Assert.Equal(candidato.Id, dto.CandidatoId);
        Assert.Equal(candidato.Nome, dto.NomeCandidato);
        Assert.Equal(candidato.Email.Value, dto.EmailCandidato);
        Assert.Equal(candidato.Telefone.Value, dto.TelefoneCandidato);
        Assert.Equal(vaga.Id, dto.VagaId);
        Assert.Equal(vaga.Titulo, dto.TituloVaga);
    }

    [Fact]
    public async Task ListarPorVagaDeveRetornarOkComCandidatosDaVaga()
    {
        var vaga = CriarVaga();
        var candidatoA = CriarCandidato("Maria Silva", "maria@example.com");
        var candidatoB = CriarCandidato("Joao Souza", "joao@example.com");
        var candidaturaA = CriarCandidatura(candidatoA.Id, vaga.Id);
        var candidaturaB = CriarCandidatura(candidatoB.Id, vaga.Id);

        _vagaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        _candidaturaRepositoryMock
            .Setup(r => r.ListarPorVagaAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { candidaturaA, candidaturaB });
        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidatoA.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatoA);
        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidatoB.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatoB);

        var result = await _controller.ListarPorVaga(vaga.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<CandidaturaDetalhadaDto>>(ok.Value).ToList();

        Assert.Equal(2, dtos.Count);
        Assert.Contains(dtos, dto => dto.CandidatoId == candidatoA.Id);
        Assert.Contains(dtos, dto => dto.CandidatoId == candidatoB.Id);
        Assert.All(dtos, dto => Assert.Equal(vaga.Titulo, dto.TituloVaga));
    }

    [Fact]
    public async Task AprovarDeveRetornarOkComCandidaturaAprovada()
    {
        var candidato = CriarCandidato();
        var vaga = CriarVaga();
        var candidatura = CriarCandidatura(candidato.Id, vaga.Id);

        SetupCandidaturaComRelacionamentos(candidatura, candidato, vaga);
        SetupAtualizar(candidatura);

        var result = await _controller.Aprovar(
            candidatura.Id,
            new ObservacoesRequest("Perfil aderente"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidaturaDto>(ok.Value);

        Assert.Equal(StatusCandidatura.Aprovado, dto.Status);
        Assert.Equal("Perfil aderente", dto.Observacoes);
        _candidaturaRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Candidatura>(c => c.Status == StatusCandidatura.Aprovado),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AprovarDeveAceitarRequestNulo()
    {
        var candidato = CriarCandidato();
        var vaga = CriarVaga();
        var candidatura = CriarCandidatura(candidato.Id, vaga.Id);

        SetupCandidaturaComRelacionamentos(candidatura, candidato, vaga);
        SetupAtualizar(candidatura);

        var result = await _controller.Aprovar(candidatura.Id, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidaturaDto>(ok.Value);

        Assert.Equal(StatusCandidatura.Aprovado, dto.Status);
        Assert.Null(dto.Observacoes);
        _candidaturaRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Candidatura>(c =>
                    c.Status == StatusCandidatura.Aprovado &&
                    c.Observacoes == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReprovarDeveRetornarOkComCandidaturaReprovada()
    {
        var candidato = CriarCandidato();
        var vaga = CriarVaga();
        var candidatura = CriarCandidatura(candidato.Id, vaga.Id);

        SetupCandidaturaComRelacionamentos(candidatura, candidato, vaga);
        SetupAtualizar(candidatura);

        var result = await _controller.Reprovar(
            candidatura.Id,
            new ObservacoesRequest("Sem fit tecnico"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidaturaDto>(ok.Value);

        Assert.Equal(StatusCandidatura.Reprovado, dto.Status);
        Assert.Equal("Sem fit tecnico", dto.Observacoes);
        _candidaturaRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Candidatura>(c => c.Status == StatusCandidatura.Reprovado),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReprovarDeveAceitarRequestNulo()
    {
        var candidato = CriarCandidato();
        var vaga = CriarVaga();
        var candidatura = CriarCandidatura(candidato.Id, vaga.Id);

        SetupCandidaturaComRelacionamentos(candidatura, candidato, vaga);
        SetupAtualizar(candidatura);

        var result = await _controller.Reprovar(candidatura.Id, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidaturaDto>(ok.Value);

        Assert.Equal(StatusCandidatura.Reprovado, dto.Status);
        Assert.Null(dto.Observacoes);
        _candidaturaRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Candidatura>(c =>
                    c.Status == StatusCandidatura.Reprovado &&
                    c.Observacoes == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelarDeveRetornarOkComCandidaturaCancelada()
    {
        var candidato = CriarCandidato();
        var vaga = CriarVaga();
        var candidatura = CriarCandidatura(candidato.Id, vaga.Id);

        SetupCandidaturaComRelacionamentos(candidatura, candidato, vaga);
        SetupAtualizar(candidatura);

        var result = await _controller.Cancelar(candidatura.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidaturaDto>(ok.Value);

        Assert.Equal(StatusCandidatura.Cancelado, dto.Status);
        _candidaturaRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Candidatura>(c => c.Status == StatusCandidatura.Cancelado),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObterPorIdDevePropagarDomainExceptionParaMiddleware()
    {
        var candidaturaId = Guid.Parse("390d59b1-4113-4ae3-a9b2-5ab53497947c");

        _candidaturaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidaturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidatura?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _controller.ObterPorId(candidaturaId));

        Assert.Equal("Candidatura não encontrada.", excecao.Message);
    }

    private void SetupCandidaturaComRelacionamentos(
        Candidatura candidatura,
        Candidato candidato,
        Vaga vaga)
    {
        _candidaturaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidatura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatura);
        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _vagaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
    }

    private void SetupAtualizar(Candidatura candidatura)
    {
        _candidaturaRepositoryMock
            .Setup(r => r.AtualizarAsync(candidatura, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static Candidato CriarCandidato(
        string nome = "Maria Silva",
        string email = "maria.silva@example.com") =>
        Candidato.Criar(nome, email, "11999998888");

    private static Vaga CriarVaga(string titulo = "Desenvolvedor Back-end") =>
        Vaga.Criar(titulo, "Descricao da vaga", "Requisitos da vaga", 12000);

    private static Candidatura CriarCandidatura(Guid candidatoId, Guid vagaId) =>
        Candidatura.Criar(candidatoId, vagaId);
}
