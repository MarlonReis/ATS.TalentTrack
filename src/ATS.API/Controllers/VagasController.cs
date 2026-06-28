namespace ATS.API.Controllers;

using ATS.API.Requests.Vagas;
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
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/v1/vagas")]
public sealed class VagasController : ControllerBase
{
    private readonly CreateVagaHandler _createHandler;
    private readonly GetVagaByIdHandler _getByIdHandler;
    private readonly ListVagasHandler _listHandler;
    private readonly ListVagasComCursorHandler _listComCursorHandler;
    private readonly UpdateVagaHandler _updateHandler;
    private readonly DeleteVagaHandler _deleteHandler;
    private readonly FecharVagaHandler _fecharHandler;

    public VagasController(
        CreateVagaHandler createHandler,
        GetVagaByIdHandler getByIdHandler,
        ListVagasHandler listHandler,
        ListVagasComCursorHandler listComCursorHandler,
        UpdateVagaHandler updateHandler,
        DeleteVagaHandler deleteHandler,
        FecharVagaHandler fecharHandler)
    {
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
        _listHandler = listHandler;
        _listComCursorHandler = listComCursorHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _fecharHandler = fecharHandler;
    }

    [HttpPost]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(VagaDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<VagaDto>> Criar(
        [FromBody] CreateVagaCommand command,
        CancellationToken ct = default)
    {
        var resultado = await _createHandler.HandleAsync(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(VagaDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
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
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(PagedResult<VagaDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
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

    /// <summary>
    /// Lista vagas usando paginação por cursor.
    /// Passe o valor de <c>proximoCursor</c> retornado na resposta anterior como parâmetro <c>cursor</c>.
    /// </summary>
    [HttpGet("cursor")]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(CursorPagedResult<VagaDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CursorPagedResult<VagaDto>>> ListarComCursor(
        [FromQuery] string? cursor = null,
        [FromQuery] int limite = 20,
        [FromQuery] StatusVaga? status = null,
        CancellationToken ct = default)
    {
        var resultado = await _listComCursorHandler.HandleAsync(
            new ListVagasComCursorQuery(cursor, limite, status),
            ct);

        return Ok(resultado);
    }

    [HttpPut("{id:guid}")]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(VagaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
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
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(VagaDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<VagaDto>> Fechar(
        Guid id,
        CancellationToken ct = default)
    {
        var resultado = await _fecharHandler.HandleAsync(new FecharVagaCommand(id), ct);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> Remover(
        Guid id,
        CancellationToken ct = default)
    {
        await _deleteHandler.HandleAsync(new DeleteVagaCommand(id), ct);
        return NoContent();
    }
}
