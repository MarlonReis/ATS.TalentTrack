namespace ATS.API.Middlewares;

using System.Text.Json;
using ATS.Application.Common.Validation;
using ATS.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public sealed partial class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = ResolveStatusCode(exception);
        var traceId = context.TraceIdentifier;
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;

        if (exception is DomainException)
        {
            LogDomainException(method, path, statusCode, traceId, exception);
        }
        else if (exception is not ValidationException)
        {
            LogUnexpectedException(method, path, statusCode, traceId, exception);
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        if (exception is ValidationException validationEx)
        {
            var errors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var validationProblem = new ValidationProblemDetails(errors)
            {
                Status = statusCode,
                Title = "Um ou mais erros de validação ocorreram.",
                Instance = context.Request.Path
            };
            validationProblem.Extensions["traceId"] = traceId;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(validationProblem, _jsonOptions),
                context.RequestAborted);

            return;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ResolveTitle(exception, statusCode),
            Detail = ResolveDetail(exception, statusCode),
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = traceId;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, _jsonOptions),
            context.RequestAborted);
    }

    private static int ResolveStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            DomainException domainException => ResolveDomainStatusCode(domainException.Message),
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

    private static int ResolveDomainStatusCode(string message)
    {
        if (Contains(message, "não encontrada") || Contains(message, "não encontrado"))
        {
            return StatusCodes.Status404NotFound;
        }

        if (Contains(message, "já") ||
            Contains(message, "fechada") ||
            Contains(message, "Somente candidaturas"))
        {
            return StatusCodes.Status409Conflict;
        }

        return StatusCodes.Status400BadRequest;
    }

    private static string ResolveTitle(Exception exception, int statusCode) =>
        exception is DomainException
            ? exception.Message
            : statusCode switch
            {
                StatusCodes.Status400BadRequest => "Requisição inválida.",
                StatusCodes.Status404NotFound => "Recurso não encontrado.",
                _ => "Erro interno no servidor."
            };

    private static string? ResolveDetail(Exception exception, int statusCode) =>
        statusCode == StatusCodes.Status500InternalServerError
            ? "Ocorreu um erro inesperado ao processar a requisição."
            : exception.Message;

    private static bool Contains(string value, string text) =>
        value.Contains(text, StringComparison.OrdinalIgnoreCase);

    [LoggerMessage(EventId = 9001, Level = LogLevel.Warning,
        Message = "Erro de domínio em {Method} {Path} → HTTP {StatusCode} (traceId: {TraceId})")]
    private partial void LogDomainException(
        string method, string path, int statusCode, string traceId, Exception exception);

    [LoggerMessage(EventId = 9002, Level = LogLevel.Error,
        Message = "Erro inesperado em {Method} {Path} → HTTP {StatusCode} (traceId: {TraceId})")]
    private partial void LogUnexpectedException(
        string method, string path, int statusCode, string traceId, Exception exception);
}
