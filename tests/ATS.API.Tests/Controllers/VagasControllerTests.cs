using ATS.API.Controllers;
using ATS.API.Requests.Vagas;
using ATS.Application.Common.Pagination;
using ATS.Application.Vagas.Commands.CreateVaga;
using ATS.Application.Vagas.Commands.DeleteVaga;
using ATS.Application.Vagas.Commands.FecharVaga;
using ATS.Application.Vagas.Commands.UpdateVaga;
using ATS.Application.Vagas.DTOs;
using ATS.Application.Vagas.Queries.GetVagaById;
using ATS.Application.Vagas.Queries.ListVagas;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ATS.API.Tests.Controllers;

public class VagasControllerTests
{
    private readonly Mock<IVagaRepository> _vagaRepositoryMock;
    private readonly VagasController _controller;

    public VagasControllerTests()
    {
        _vagaRepositoryMock = new Mock<IVagaRepository>(MockBehavior.Strict);

        _controller = new VagasController(
            new CreateVagaHandler(_vagaRepositoryMock.Object, NullLogger<CreateVagaHandler>.Instance),
            new GetVagaByIdHandler(_vagaRepositoryMock.Object),
            new ListVagasHandler(_vagaRepositoryMock.Object),
            new UpdateVagaHandler(_vagaRepositoryMock.Object, NullLogger<UpdateVagaHandler>.Instance),
            new DeleteVagaHandler(_vagaRepositoryMock.Object, NullLogger<DeleteVagaHandler>.Instance),
            new FecharVagaHandler(_vagaRepositoryMock.Object, NullLogger<FecharVagaHandler>.Instance));
    }

    [Fact]
    public async Task CriarDeveRetornarCreatedAtActionComDto()
    {
        Vaga? vagaAdicionada = null;

        _vagaRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Vaga>(), It.IsAny<CancellationToken>()))
            .Callback<Vaga, CancellationToken>((vaga, _) => vagaAdicionada = vaga)
            .Returns(Task.CompletedTask);

        var result = await _controller.Criar(
            new CreateVagaCommand(
                "Desenvolvedor Back-end",
                "Desenvolver APIs",
                "C# e MongoDB",
                12000m));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<VagaDto>(created.Value);

        Assert.Equal(nameof(VagasController.ObterPorId), created.ActionName);
        Assert.NotNull(created.RouteValues);
        Assert.Equal(dto.Id, created.RouteValues["id"]);
        Assert.Equal("Desenvolvedor Back-end", dto.Titulo);
        Assert.Equal("Desenvolver APIs", dto.Descricao);
        Assert.Equal("C# e MongoDB", dto.Requisitos);
        Assert.Equal(12000m, dto.Salario);
        Assert.Equal(StatusVaga.Aberta, dto.Status);
        Assert.NotNull(vagaAdicionada);
    }

    [Fact]
    public async Task ObterPorIdDeveRetornarOkComDto()
    {
        var vaga = CriarVaga();

        _vagaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);

        var result = await _controller.ObterPorId(vaga.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<VagaDto>(ok.Value);

        Assert.Equal(vaga.Id, dto.Id);
        Assert.Equal(vaga.Titulo, dto.Titulo);
        Assert.Equal(vaga.Descricao, dto.Descricao);
        Assert.Equal(vaga.Requisitos, dto.Requisitos);
        Assert.Equal(vaga.Salario.Valor, dto.Salario);
        Assert.Equal(vaga.Status, dto.Status);
    }

    [Fact]
    public async Task ListarDeveRetornarOkComResultadoPaginado()
    {
        var vagas = new[]
        {
            CriarVaga("Desenvolvedor Back-end"),
            CriarVaga("Tech Lead")
        };

        _vagaRepositoryMock
            .Setup(r => r.ListarAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vagas);
        _vagaRepositoryMock
            .Setup(r => r.ContarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        var result = await _controller.Listar(2, 10, StatusVaga.Aberta);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var paged = Assert.IsType<PagedResult<VagaDto>>(ok.Value);
        var items = paged.Items.ToList();

        Assert.Equal(2, paged.Pagina);
        Assert.Equal(10, paged.TamanhoPagina);
        Assert.Equal(25, paged.Total);
        Assert.Equal(2, items.Count);
        Assert.All(items, dto => Assert.Equal(StatusVaga.Aberta, dto.Status));
    }

    [Fact]
    public async Task AtualizarDeveRetornarOkComDtoAtualizado()
    {
        var vaga = CriarVaga();

        _vagaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        SetupAtualizar(vaga);

        var result = await _controller.Atualizar(
            vaga.Id,
            new AtualizarVagaRequest(
                "Desenvolvedor Full Stack",
                "Construir soluções",
                "C# e React",
                15000m));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<VagaDto>(ok.Value);

        Assert.Equal("Desenvolvedor Full Stack", dto.Titulo);
        Assert.Equal("Construir soluções", dto.Descricao);
        Assert.Equal("C# e React", dto.Requisitos);
        Assert.Equal(15000m, dto.Salario);
        _vagaRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Vaga>(v =>
                    v.Id == vaga.Id &&
                    v.Titulo == "Desenvolvedor Full Stack" &&
                    v.Descricao == "Construir soluções" &&
                    v.Requisitos == "C# e React" &&
                    v.Salario.Valor == 15000m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FecharDeveRetornarOkComVagaFechada()
    {
        var vaga = CriarVaga();

        _vagaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        SetupAtualizar(vaga);

        var result = await _controller.Fechar(vaga.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<VagaDto>(ok.Value);

        Assert.Equal(StatusVaga.Fechada, dto.Status);
        Assert.Equal("Fechada", dto.StatusDescricao);
        Assert.NotNull(dto.DataEncerramento);
        _vagaRepositoryMock.Verify(
            r => r.AtualizarAsync(
                It.Is<Vaga>(v => v.Id == vaga.Id && v.Status == StatusVaga.Fechada),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoverDeveRetornarNoContent()
    {
        var vaga = CriarVaga();

        _vagaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
        _vagaRepositoryMock
            .Setup(r => r.RemoverAsync(vaga.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Remover(vaga.Id);

        Assert.IsType<NoContentResult>(result);
        _vagaRepositoryMock.Verify(
            r => r.RemoverAsync(vaga.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObterPorIdDevePropagarDomainExceptionParaMiddleware()
    {
        var vagaId = Guid.Parse("33cc361e-26d7-4773-8211-dbf25efb3ee2");

        _vagaRepositoryMock
            .Setup(r => r.ObterPorIdAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vaga?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _controller.ObterPorId(vagaId));

        Assert.Equal("Vaga não encontrada.", excecao.Message);
    }

    private void SetupAtualizar(Vaga vaga)
    {
        _vagaRepositoryMock
            .Setup(r => r.AtualizarAsync(vaga, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static Vaga CriarVaga(string titulo = "Desenvolvedor Back-end") =>
        Vaga.Criar(titulo, "Descrição da vaga", "Requisitos da vaga", 12000m);
}
