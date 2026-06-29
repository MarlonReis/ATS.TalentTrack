namespace ATS.API.Controllers;

using ATS.API.Requests.Candidaturas;
using ATS.Application.Candidaturas.Commands.AprovarCandidatura;
using ATS.Application.Candidaturas.Commands.CancelarCandidatura;
using ATS.Application.Candidaturas.Commands.CandidatarSe;
using ATS.Application.Candidaturas.Commands.ReprovarCandidatura;
using ATS.Application.Candidaturas.DTOs;
using ATS.Application.Candidaturas.Queries.GetCandidaturaById;
using ATS.Application.Candidaturas.Queries.ListCandidatosPorVaga;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/v1/candidaturas")]
public sealed class CandidaturasController : ControllerBase
{
    private readonly CandidatarSeHandler _candidatarSeHandler;
    private readonly GetCandidaturaByIdHandler _getByIdHandler;
    private readonly AprovarCandidaturaHandler _aprovarHandler;
    private readonly ReprovarCandidaturaHandler _reprovarHandler;
    private readonly CancelarCandidaturaHandler _cancelarHandler;
    private readonly ListCandidatosPorVagaHandler _listarHandler;

    public CandidaturasController(
        CandidatarSeHandler candidatarSeHandler,
        GetCandidaturaByIdHandler getByIdHandler,
        AprovarCandidaturaHandler aprovarHandler,
        ReprovarCandidaturaHandler reprovarHandler,
        CancelarCandidaturaHandler cancelarHandler,
        ListCandidatosPorVagaHandler listarHandler)
    {
        _candidatarSeHandler = candidatarSeHandler;
        _getByIdHandler = getByIdHandler;
        _aprovarHandler = aprovarHandler;
        _reprovarHandler = reprovarHandler;
        _cancelarHandler = cancelarHandler;
        _listarHandler = listarHandler;
    }

    [HttpPost]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(CandidaturaDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidaturaDto>> CandidatarSe(
        [FromBody] CandidatarSeCommand command,
        CancellationToken ct = default)
    {
        var resultado = await _candidatarSeHandler.HandleAsync(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(CandidaturaDetalhadaDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidaturaDetalhadaDto>> ObterPorId(
        Guid id,
        CancellationToken ct = default)
    {
        var resultado = await _getByIdHandler.HandleAsync(
            new GetCandidaturaByIdQuery(id),
            ct);

        return Ok(resultado);
    }

    [HttpGet("vagas/{vagaId:guid}/candidatos")]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(IEnumerable<CandidaturaDetalhadaDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<IEnumerable<CandidaturaDetalhadaDto>>> ListarPorVaga(
        Guid vagaId,
        CancellationToken ct = default)
    {
        var resultado = await _listarHandler.HandleAsync(
            new ListCandidatosPorVagaQuery(vagaId),
            ct);

        return Ok(resultado);
    }

    [HttpPatch("{id:guid}/aprovar")]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(CandidaturaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidaturaDto>> Aprovar(
        Guid id,
        [FromBody] ObservacoesRequest? request,
        CancellationToken ct = default)
    {
        var resultado = await _aprovarHandler.HandleAsync(
            new AprovarCandidaturaCommand(id, request?.Observacoes),
            ct);

        return Ok(resultado);
    }

    [HttpPatch("{id:guid}/reprovar")]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(CandidaturaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidaturaDto>> Reprovar(
        Guid id,
        [FromBody] ObservacoesRequest? request,
        CancellationToken ct = default)
    {
        var resultado = await _reprovarHandler.HandleAsync(
            new ReprovarCandidaturaCommand(id, request?.Observacoes),
            ct);

        return Ok(resultado);
    }

    [HttpPatch("{id:guid}/cancelar")]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(CandidaturaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<CandidaturaDto>> Cancelar(
        Guid id,
        CancellationToken ct = default)
    {
        var resultado = await _cancelarHandler.HandleAsync(
            new CancelarCandidaturaCommand(id),
            ct);

        return Ok(resultado);
    }
}
