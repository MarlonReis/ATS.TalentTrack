namespace ATS.Application.Tests.Candidaturas.Events;

using ATS.Application.Candidaturas.Events;
using ATS.Domain.Candidaturas.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class NotificarCandidatoAprovadoHandlerTests
{
    private readonly NotificarCandidatoAprovadoHandler _handler;

    public NotificarCandidatoAprovadoHandlerTests()
    {
        _handler = new NotificarCandidatoAprovadoHandler(
            NullLogger<NotificarCandidatoAprovadoHandler>.Instance);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                "cccccccc-cccc-cccc-cccc-cccccccccccc",
                "Parabéns!")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "1fa25a64-3317-1562-c3fc-1c863f66afa6",
                "2bc35b74-4427-2673-d4fd-2d974f77bfb7",
                null)]
    public async Task DeveCompletarSemExcecaoParaQualquerNotificacaoValida(
        string candidaturaIdStr, string candidatoIdStr, string vagaIdStr, string? observacoes)
    {
        var notification = new CandidaturaAprovadaEvent(
            Guid.Parse(candidaturaIdStr),
            Guid.Parse(candidatoIdStr),
            Guid.Parse(vagaIdStr),
            observacoes);

        await _handler.Handle(notification, CancellationToken.None);
    }

    [Fact]
    public async Task DeveRetornarTaskCompletedTask()
    {
        var notification = new CandidaturaAprovadaEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        var result = _handler.Handle(notification, CancellationToken.None);

        Assert.True(result.IsCompleted);
        await result;
    }

    [Fact]
    public async Task DeveProcessarCancellationTokenSemErro()
    {
        var cts = new CancellationTokenSource();
        var notification = new CandidaturaAprovadaEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "obs");

        await _handler.Handle(notification, cts.Token);
    }

    [Fact]
    public async Task HandleDeveLogarInformacaoQuandoHandleForChamado()
    {
        var loggerMock = new Mock<ILogger<NotificarCandidatoAprovadoHandler>>();
        loggerMock
            .Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);
        var handler = new NotificarCandidatoAprovadoHandler(loggerMock.Object);

        var notification = new CandidaturaAprovadaEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        await handler.Handle(notification, CancellationToken.None);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
