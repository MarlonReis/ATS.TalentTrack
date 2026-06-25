namespace ATS.API.Controllers;

using ATS.Application.Common.Pagination;
using ATS.Application.Vagas.Commands.CreateVaga;
using ATS.Application.Vagas.Commands.DeleteVaga;
using ATS.Application.Vagas.Commands.FecharVaga;
using ATS.Application.Vagas.Commands.UpdateVaga;
using ATS.Application.Vagas.DTOs;
using ATS.Application.Vagas.Queries.GetVagaById;
using ATS.Application.Vagas.Queries.ListVagas;
using ATS.Domain.Vagas.Enums;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/vagas")]
public sealed class VagasController : ControllerBase
{
    private readonly CreateVagaHandler _createHandler;
    private readonly GetVagaByIdHandler _getByIdHandler;
    private readonly ListVagasHandler _listHandler;
    private readonly UpdateVagaHandler _updateHandler;
    private readonly DeleteVagaHandler _deleteHandler;
    private readonly FecharVagaHandler _fecharHandler;

    public VagasController(
        CreateVagaHandler createHandler,
        GetVagaByIdHandler getByIdHandler,
        ListVagasHandler listHandler,
        UpdateVagaHandler updateHandler,
        DeleteVagaHandler deleteHandler,
        FecharVagaHandler fecharHandler)
    {
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
        _listHandler = listHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _fecharHandler = fecharHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(VagaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<VagaDto>> Criar(
        [FromBody] CreateVagaCommand command,
        CancellationToken ct = default)
    {
        var resultado = await _createHandler.HandleAsync(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VagaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VagaDto>> ObterPorId(
        Guid id,
        CancellationToken ct = default)
    {
        var resultado = await _getByIdHandler.HandleAsync(
            new GetVagaByIdQuery(id),
            ct);

        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VagaDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<PagedResult<VagaDto>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        [FromQuery] StatusVaga? status = null,
        CancellationToken ct = default)
    {
        var resultado = await _listHandler.HandleAsync(
            new ListVagasQuery(pagina, tamanhoPagina, status),
            ct);

        return Ok(resultado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VagaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<VagaDto>> Atualizar(
        Guid id,
        [FromBody] AtualizarVagaRequest request,
        CancellationToken ct = default)
    {
        var resultado = await _updateHandler.HandleAsync(
            new UpdateVagaCommand(
                id,
                request.Titulo,
                request.Descricao,
                request.Requisitos,
                request.Salario),
            ct);

        return Ok(resultado);
    }

    [HttpPatch("{id:guid}/fechar")]
    [ProducesResponseType(typeof(VagaDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<VagaDto>> Fechar(
        Guid id,
        CancellationToken ct = default)
    {
        var resultado = await _fecharHandler.HandleAsync(new FecharVagaCommand(id), ct);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Remover(
        Guid id,
        CancellationToken ct = default)
    {
        await _deleteHandler.HandleAsync(new DeleteVagaCommand(id), ct);
        return NoContent();
    }

    public sealed record AtualizarVagaRequest(
        string Titulo,
        string Descricao,
        string? Requisitos = null,
        decimal Salario = 0m);
}
