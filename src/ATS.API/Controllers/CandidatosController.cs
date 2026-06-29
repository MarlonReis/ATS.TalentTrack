namespace ATS.API.Controllers;

using ATS.API.Requests.Candidatos;
using ATS.Application.Candidatos.Commands.AddCurriculo;
using ATS.Application.Candidatos.Commands.CreateCandidato;
using ATS.Application.Candidatos.Commands.DeleteCandidato;
using ATS.Application.Candidatos.Commands.UpdateCandidato;
using ATS.Application.Candidatos.DTOs;
using ATS.Application.Candidatos.Queries.GetCandidatoById;
using ATS.Application.Candidatos.Queries.ListCandidatos;
using ATS.Application.Common.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/v1/candidatos")]
public sealed class CandidatosController : ControllerBase
{
    private readonly CreateCandidatoHandler _createHandler;
    private readonly GetCandidatoByIdHandler _getByIdHandler;
    private readonly ListCandidatosHandler _listHandler;
    private readonly ListCandidatosComCursorHandler _listComCursorHandler;
    private readonly UpdateCandidatoHandler _updateHandler;
    private readonly DeleteCandidatoHandler _deleteHandler;
    private readonly AddCurriculoHandler _addCurriculoHandler;

    public CandidatosController(
        CreateCandidatoHandler createHandler,
        GetCandidatoByIdHandler getByIdHandler,
        ListCandidatosHandler listHandler,
        ListCandidatosComCursorHandler listComCursorHandler,
        UpdateCandidatoHandler updateHandler,
        DeleteCandidatoHandler deleteHandler,
        AddCurriculoHandler addCurriculoHandler)
    {
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
        _listHandler = listHandler;
        _listComCursorHandler = listComCursorHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _addCurriculoHandler = addCurriculoHandler;
    }

    [HttpPost]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(CandidatoDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidatoDto>> Criar(
        [FromBody] CreateCandidatoCommand command,
        CancellationToken ct = default)
    {
        var resultado = await _createHandler.HandleAsync(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(CandidatoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidatoDto>> ObterPorId(
        Guid id,
        CancellationToken ct = default)
    {
        var resultado = await _getByIdHandler.HandleAsync(
            new GetCandidatoByIdQuery(id),
            ct);

        return Ok(resultado);
    }

    [HttpGet]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(PagedResult<CandidatoDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<PagedResult<CandidatoDto>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken ct = default)
    {
        var resultado = await _listHandler.HandleAsync(
            new ListCandidatosQuery(pagina, tamanhoPagina),
            ct);

        return Ok(resultado);
    }

    /// <summary>
    /// Lista candidatos usando paginação por cursor.
    /// Passe o valor de <c>proximoCursor</c> retornado na resposta anterior como parâmetro <c>cursor</c>.
    /// </summary>
    [HttpGet("cursor")]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(CursorPagedResult<CandidatoDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CursorPagedResult<CandidatoDto>>> ListarComCursor(
        [FromQuery] string? cursor = null,
        [FromQuery] int limite = 20,
        CancellationToken ct = default)
    {
        var resultado = await _listComCursorHandler.HandleAsync(
            new ListCandidatosComCursorQuery(cursor, limite),
            ct);

        return Ok(resultado);
    }

    [HttpPut("{id:guid}")]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(CandidatoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidatoDto>> Atualizar(
        Guid id,
        [FromBody] AtualizarCandidatoRequest request,
        CancellationToken ct = default)
    {
        var resultado = await _updateHandler.HandleAsync(
            new UpdateCandidatoCommand(
                id,
                request.Nome,
                request.Email,
                request.Telefone),
            ct);

        return Ok(resultado);
    }

    [HttpPost("{id:guid}/curriculo")]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(CandidatoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidatoDto>> AdicionarCurriculo(
        Guid id,
        [FromBody] AdicionarCurriculoRequest request,
        CancellationToken ct = default)
    {
        var resultado = await _addCurriculoHandler.HandleAsync(
            new AddCurriculoCommand(
                id,
                request.NomeArquivo,
                request.ContentType,
                request.UrlOuBase64),
            ct);

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
        await _deleteHandler.HandleAsync(new DeleteCandidatoCommand(id), ct);
        return NoContent();
    }
}
