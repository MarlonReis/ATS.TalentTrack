using System.Text.Json;
using ATS.API.Middlewares;
using ATS.Application.Common.Validation;
using ATS.Domain.Shared;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;

namespace ATS.API.Tests.Middlewares;

public class ExceptionHandlingMiddlewareTests
{
    [Theory]
    [InlineData("Candidatura não encontrada.", StatusCodes.Status404NotFound)]
    [InlineData("Candidato já se candidatou a esta vaga.", StatusCodes.Status409Conflict)]
    [InlineData("Não é possível se candidatar a uma vaga fechada.", StatusCodes.Status409Conflict)]
    [InlineData("Somente candidaturas 'Em Análise' podem ser aprovadas.", StatusCodes.Status409Conflict)]
    [InlineData("E-mail não pode ser vazio.", StatusCodes.Status400BadRequest)]
    public async Task DeveConverterDomainExceptionEmProblemDetails(
        string mensagem,
        int statusCodeEsperado)
    {
        var exception = new DomainException(mensagem);
        var (context, loggerMock) = await InvokeMiddlewareAsync(exception);

        using var json = await ReadJsonAsync(context);

        AssertProblem(
            json.RootElement,
            statusCodeEsperado,
            mensagem,
            mensagem,
            "/api/v1/candidaturas",
            "trace-test");
        Assert.Equal(statusCodeEsperado, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        VerifyLog(loggerMock, LogLevel.Warning, exception, "Erro de domínio");
    }

    [Theory]
    [MemberData(nameof(ClientExceptionCases))]
    public async Task DeveConverterExcecoesDeClienteEmProblemDetails(
        Exception exception,
        int statusCodeEsperado,
        string tituloEsperado)
    {
        var (context, loggerMock) = await InvokeMiddlewareAsync(exception);

        using var json = await ReadJsonAsync(context);

        AssertProblem(
            json.RootElement,
            statusCodeEsperado,
            tituloEsperado,
            exception.Message,
            "/api/v1/candidaturas",
            "trace-test");
        Assert.Equal(statusCodeEsperado, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        VerifyLog(loggerMock, LogLevel.Error, exception, "Erro inesperado");
    }

    [Fact]
    public async Task DeveConverterExcecaoInesperadaEmProblemDetailsGenerico()
    {
        var exception = new InvalidOperationException("Erro interno sensivel.");

        var (context, loggerMock) = await InvokeMiddlewareAsync(exception);

        using var json = await ReadJsonAsync(context);

        AssertProblem(
            json.RootElement,
            StatusCodes.Status500InternalServerError,
            "Erro interno no servidor.",
            "Ocorreu um erro inesperado ao processar a requisição.",
            "/api/v1/candidaturas",
            "trace-test");
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.DoesNotContain(
            exception.Message,
            json.RootElement.GetProperty("detail").GetString(),
            StringComparison.OrdinalIgnoreCase);
        VerifyLog(loggerMock, LogLevel.Error, exception, "Erro inesperado");
    }

    [Fact]
    public async Task DeveResponderCorretamenteQuandoLogDomainExceptionEstiverDesabilitado()
    {
        var exception = new DomainException("Candidatura não encontrada.");
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        // IsEnabled=false aciona o branch de retorno antecipado do código gerado por [LoggerMessage]
        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(false);

        var context = CriarContexto();
        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.FromException(exception),
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(StatusCodes.Status404NotFound, json.RootElement.GetProperty("status").GetInt32());
        loggerMock.Verify(l => l.Log(
            It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public async Task DeveResponderCorretamenteQuandoLogUnexpectedExceptionEstiverDesabilitado()
    {
        var exception = new InvalidOperationException("Erro interno.");
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        // IsEnabled=false aciona o branch de retorno antecipado do código gerado por [LoggerMessage]
        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(false);

        var context = CriarContexto();
        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.FromException(exception),
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(StatusCodes.Status500InternalServerError, json.RootElement.GetProperty("status").GetInt32());
        loggerMock.Verify(l => l.Log(
            It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public async Task DeveExecutarProximoMiddlewareQuandoNaoHouverExcecao()
    {
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var context = CriarContexto();
        var middleware = new ExceptionHandlingMiddleware(
            ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                return Task.CompletedTask;
            },
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
        loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeveUsarStringVaziaQuandoPathForNulo()
    {
        var exception = new DomainException("E-mail não pode ser vazio.");
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var context = new DefaultHttpContext { TraceIdentifier = "trace-null-path" };
        context.Request.Method = HttpMethods.Post;
        context.Response.Body = new MemoryStream();

        // DefaultHttpRequest armazena Path como string via IHttpRequestFeature.
        // Definir null diretamente força PathString.Value == null → aciona ?? string.Empty
        context.Features.Get<IHttpRequestFeature>()!.Path = null!;

        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.FromException(exception),
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(StatusCodes.Status400BadRequest, json.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task DevePropagarExcecaoQuandoRespostaJaTiverIniciado()
    {
        var exception = new InvalidOperationException("Falha apos inicio da resposta.");
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var context = CriarContexto();
        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.FromException(exception),
            loggerMock.Object);

        context.Features.Set<IHttpResponseFeature>(
            new StartedResponseFeature(context.Response.Body));

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));

        Assert.Same(exception, excecao);
        loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeveRetornar400EValidationProblemDetailsParaValidationException()
    {
        var errors = new[]
        {
            new ValidationFailure("Nome", "Nome é obrigatório."),
            new ValidationFailure("Email", "E-mail inválido.")
        };
        var exception = new ValidationException(errors);

        var (context, _) = await InvokeMiddlewareAsync(exception);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Um ou mais erros de validação ocorreram.", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("/api/v1/candidaturas", json.RootElement.GetProperty("instance").GetString());
        Assert.Equal("trace-test", GetTraceId(json.RootElement));
    }

    [Fact]
    public async Task DeveAgruparErrosDeValidacaoPorPropriedade()
    {
        var errors = new[]
        {
            new ValidationFailure("Nome", "Nome é obrigatório."),
            new ValidationFailure("Nome", "Nome não pode exceder 200 caracteres."),
            new ValidationFailure("Email", "E-mail inválido.")
        };
        var exception = new ValidationException(errors);

        var (context, _) = await InvokeMiddlewareAsync(exception);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);

        var errosJson = json.RootElement.GetProperty("errors");
        var nomeErros = errosJson.GetProperty("Nome");
        Assert.Equal(2, nomeErros.GetArrayLength());
        var emailErros = errosJson.GetProperty("Email");
        Assert.Equal(1, emailErros.GetArrayLength());
    }

    [Fact]
    public async Task DeveNaoLogarValidationException()
    {
        var exception = new ValidationException([new ValidationFailure("Campo", "Inválido.")]);
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var context = CriarContexto();
        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.FromException(exception),
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        loggerMock.Verify(l => l.Log(
            It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public StartedResponseFeature(Stream body)
        {
            Body = body;
        }

        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; }
        public bool HasStarted => true;

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }
    }

    public static IEnumerable<object[]> ClientExceptionCases()
    {
        yield return new object[]
        {
            new BadHttpRequestException("Corpo da requisição inválido."),
            StatusCodes.Status400BadRequest,
            "Requisição inválida."
        };
        yield return new object[]
        {
            new ArgumentException("Argumento inválido."),
            StatusCodes.Status400BadRequest,
            "Requisição inválida."
        };
        yield return new object[]
        {
            new KeyNotFoundException("Registro não encontrado."),
            StatusCodes.Status404NotFound,
            "Recurso não encontrado."
        };
    }

    private static async Task<(DefaultHttpContext Context, Mock<ILogger<ExceptionHandlingMiddleware>> LoggerMock)>
        InvokeMiddlewareAsync(Exception exception)
    {
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        // Source-generated [LoggerMessage] verifica IsEnabled antes de logar
        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var context = CriarContexto();
        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.FromException(exception),
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        return (context, loggerMock);
    }

    private static DefaultHttpContext CriarContexto()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-test"
        };

        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/candidaturas";
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }

    private static void AssertProblem(
        JsonElement root,
        int status,
        string title,
        string? detail,
        string instance,
        string traceId)
    {
        Assert.Equal(status, root.GetProperty("status").GetInt32());
        Assert.Equal(title, root.GetProperty("title").GetString());
        Assert.Equal(detail, root.GetProperty("detail").GetString());
        Assert.Equal(instance, root.GetProperty("instance").GetString());
        Assert.Equal(traceId, GetTraceId(root));
    }

    private static string GetTraceId(JsonElement root)
    {
        if (root.TryGetProperty("traceId", out var traceId))
        {
            return traceId.GetString()!;
        }

        if (root.TryGetProperty("extensions", out var extensions) &&
            extensions.TryGetProperty("traceId", out traceId))
        {
            return traceId.GetString()!;
        }

        throw new InvalidOperationException("traceId não encontrado no ProblemDetails.");
    }

    private static void VerifyLog(
        Mock<ILogger<ExceptionHandlingMiddleware>> loggerMock,
        LogLevel level,
        Exception exception,
        string mensagemEsperada)
    {
        loggerMock.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains(mensagemEsperada, StringComparison.OrdinalIgnoreCase)),
                It.Is<Exception>(ex => ReferenceEquals(ex, exception)),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
