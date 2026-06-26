using ATS.API.Controllers;
using ATS.API.Requests.Candidatos;
using ATS.Application.Candidatos.Commands.AddCurriculo;
using ATS.Application.Candidatos.Commands.CreateCandidato;
using ATS.Application.Candidatos.Commands.DeleteCandidato;
using ATS.Application.Candidatos.Commands.UpdateCandidato;
using ATS.Application.Candidatos.DTOs;
using ATS.Application.Candidatos.Queries.GetCandidatoById;
using ATS.Application.Candidatos.Queries.ListCandidatos;
using ATS.Application.Common.Pagination;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ATS.API.Tests.Controllers;

public class CandidatosControllerTests
{
    private readonly Mock<ICandidatoRepository> _candidatoRepositoryMock;
    private readonly CandidatosController _controller;

    public CandidatosControllerTests()
    {
        _candidatoRepositoryMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);

        _controller = new CandidatosController(
            new CreateCandidatoHandler(_candidatoRepositoryMock.Object, NullLogger<CreateCandidatoHandler>.Instance),
            new GetCandidatoByIdHandler(_candidatoRepositoryMock.Object),
            new ListCandidatosHandler(_candidatoRepositoryMock.Object),
            new UpdateCandidatoHandler(_candidatoRepositoryMock.Object, NullLogger<UpdateCandidatoHandler>.Instance),
            new DeleteCandidatoHandler(_candidatoRepositoryMock.Object, NullLogger<DeleteCandidatoHandler>.Instance),
            new AddCurriculoHandler(_candidatoRepositoryMock.Object, NullLogger<AddCurriculoHandler>.Instance));
    }

    [Fact]
    public async Task CriarDeveRetornarCreatedAtActionComDto()
    {
        Candidato? candidatoAdicionado = null;

        _candidatoRepositoryMock
            .Setup(r => r.ObterPorEmailAsync("maria.silva@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);
        _candidatoRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()))
            .Callback<Candidato, CancellationToken>((candidato, _) =>
                candidatoAdicionado = candidato)
            .Returns(Task.CompletedTask);

        var result = await _controller.Criar(
            new CreateCandidatoCommand(
                "Maria Silva",
                "maria.silva@example.com",
                "11999998888"));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<CandidatoDto>(created.Value);

        Assert.Equal(nameof(CandidatosController.ObterPorId), created.ActionName);
        Assert.NotNull(created.RouteValues);
        Assert.Equal(dto.Id, created.RouteValues["id"]);
        Assert.Equal("Maria Silva", dto.Nome);
        Assert.Equal("maria.silva@example.com", dto.Email);
        Assert.Equal("11999998888", dto.Telefone);
        Assert.False(dto.PossuiCurriculo);
        Assert.Null(dto.NomeCurriculo);
        Assert.NotNull(candidatoAdicionado);
    }

    [Fact]
    public async Task ObterPorIdDeveRetornarOkComDto()
    {
        var candidato = CriarCandidato();

        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);

        var result = await _controller.ObterPorId(candidato.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidatoDto>(ok.Value);

        Assert.Equal(candidato.Id, dto.Id);
        Assert.Equal(candidato.Nome, dto.Nome);
        Assert.Equal(candidato.Email.Value, dto.Email);
        Assert.Equal(candidato.Telefone.Value, dto.Telefone);
        Assert.False(dto.PossuiCurriculo);
    }

    [Fact]
    public async Task ListarDeveRetornarOkComResultadoPaginado()
    {
        var candidatos = new[]
        {
            CriarCandidato("Maria Silva", "maria@example.com"),
            CriarCandidato("Joao Souza", "joao@example.com")
        };

        _candidatoRepositoryMock
            .Setup(r => r.ListarAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidatos);
        _candidatoRepositoryMock
            .Setup(r => r.ContarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        var result = await _controller.Listar(2, 10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var paged = Assert.IsType<PagedResult<CandidatoDto>>(ok.Value);
        var items = paged.Items.ToList();

        Assert.Equal(2, paged.Pagina);
        Assert.Equal(10, paged.TamanhoPagina);
        Assert.Equal(25, paged.Total);
        Assert.Equal(2, items.Count);
        Assert.Contains(items, dto => dto.Email == "maria@example.com");
        Assert.Contains(items, dto => dto.Email == "joao@example.com");
    }

    [Fact]
    public async Task AtualizarDeveRetornarOkComDtoAtualizado()
    {
        var candidato = CriarCandidato();

        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _candidatoRepositoryMock
            .Setup(r => r.ObterPorEmailAsync("maria.nova@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);
        SetupAtualizar(candidato);

        var result = await _controller.Atualizar(
            candidato.Id,
            new AtualizarCandidatoRequest(
                "Maria Silva Nova",
                "maria.nova@example.com",
                "11888887777"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidatoDto>(ok.Value);

        Assert.Equal("Maria Silva Nova", dto.Nome);
        Assert.Equal("maria.nova@example.com", dto.Email);
        Assert.Equal("11888887777", dto.Telefone);
        _candidatoRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Candidato>(c =>
                    c.Id == candidato.Id &&
                    c.Nome == "Maria Silva Nova" &&
                    c.Email.Value == "maria.nova@example.com" &&
                    c.Telefone.Value == "11888887777"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AdicionarCurriculoDeveRetornarOkComDtoAtualizado()
    {
        var candidato = CriarCandidato();

        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        SetupAtualizar(candidato);

        var result = await _controller.AdicionarCurriculo(
            candidato.Id,
            new AdicionarCurriculoRequest(
                "curriculo.pdf",
                "application/pdf",
                "base64"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CandidatoDto>(ok.Value);

        Assert.True(dto.PossuiCurriculo);
        Assert.Equal("curriculo.pdf", dto.NomeCurriculo);
        _candidatoRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Candidato>(c =>
                    c.Id == candidato.Id &&
                    c.Curriculo != null &&
                    c.Curriculo.NomeArquivo == "curriculo.pdf" &&
                    c.Curriculo.ContentType == "application/pdf" &&
                    c.Curriculo.UrlOuBase64 == "base64"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoverDeveRetornarNoContent()
    {
        var candidato = CriarCandidato();

        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
        _candidatoRepositoryMock
            .Setup(r => r.RemoverAsync(candidato.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Remover(candidato.Id);

        Assert.IsType<NoContentResult>(result);
        _candidatoRepositoryMock.Verify(
            r => r.RemoverAsync(candidato.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObterPorIdDevePropagarDomainExceptionParaMiddleware()
    {
        var candidatoId = Guid.Parse("05706950-0c5c-4576-ad37-780843963d4c");

        _candidatoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(candidatoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _controller.ObterPorId(candidatoId));

        Assert.Equal("Candidato não encontrado.", excecao.Message);
    }

    private void SetupAtualizar(Candidato candidato)
    {
        _candidatoRepositoryMock
            .Setup(r => r.AtualizarAsync(candidato, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static Candidato CriarCandidato(
        string nome = "Maria Silva",
        string email = "maria.silva@example.com") =>
        Candidato.Criar(nome, email, "11999998888");
}
