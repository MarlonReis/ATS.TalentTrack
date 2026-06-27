namespace ATS.Infrastructure.Tests.Events;

using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Events;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Events;
using ATS.Infrastructure.Events;
using MediatR;
using Moq;
using Xunit;

public class MediatRDomainEventDispatcherTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MediatRDomainEventDispatcher _dispatcher;

    public MediatRDomainEventDispatcherTests()
    {
        _mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
        _dispatcher = new MediatRDomainEventDispatcher(_mediatorMock.Object);
    }

    private static void SetupPublish(Mock<IMediator> mock)
    {
        mock.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task DevePublicarCadaEventoDoAgregadoNaOrdem()
    {
        SetupPublish(_mediatorMock);
        var candidatura = Candidatura.Criar(Guid.NewGuid(), Guid.NewGuid());

        await _dispatcher.DispatchAndClearAsync(candidatura);

        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeveLimparEventosDoAgregadoAposDespachar()
    {
        SetupPublish(_mediatorMock);
        var candidatura = Candidatura.Criar(Guid.NewGuid(), Guid.NewGuid());

        Assert.NotEmpty(candidatura.DomainEvents);

        await _dispatcher.DispatchAndClearAsync(candidatura);

        Assert.Empty(candidatura.DomainEvents);
    }

    [Fact]
    public async Task DeveDespacharMultiplosEventosQuandoAgregadoTemVarios()
    {
        SetupPublish(_mediatorMock);
        var candidatura = Candidatura.Criar(Guid.NewGuid(), Guid.NewGuid());
        candidatura.Aprovar();

        var quantidadeEventos = candidatura.DomainEvents.Count;
        Assert.Equal(2, quantidadeEventos);

        await _dispatcher.DispatchAndClearAsync(candidatura);

        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(quantidadeEventos));
        Assert.Empty(candidatura.DomainEvents);
    }

    [Fact]
    public async Task DeveNaoPublicarNenhumEventoQuandoAgregadoNaoTemEventos()
    {
        var vaga = Vaga.Criar("Dev Back-end", "Descrição", "Requisitos", 12000);
        vaga.ClearDomainEvents();

        Assert.Empty(vaga.DomainEvents);

        await _dispatcher.DispatchAndClearAsync(vaga);

        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveLimparEventosMesmoQuandoNaoHaNada()
    {
        var vaga = Vaga.Criar("Dev Back-end", "Descrição", "Requisitos", 12000);
        vaga.ClearDomainEvents();

        await _dispatcher.DispatchAndClearAsync(vaga);

        Assert.Empty(vaga.DomainEvents);
    }

    [Fact]
    public async Task DeveRepassarCancellationTokenParaPublish()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _mediatorMock
            .Setup(m => m.Publish(It.IsAny<IDomainEvent>(), ct))
            .Returns(Task.CompletedTask);

        var candidatura = Candidatura.Criar(Guid.NewGuid(), Guid.NewGuid());

        await _dispatcher.DispatchAndClearAsync(candidatura, ct);

        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<IDomainEvent>(), ct), Times.Once);
    }

    [Fact]
    public async Task DevePublicarEventoDoTipoCorreto()
    {
        CandidaturaRealizadaEvent? eventoPublicado = null;

        _mediatorMock
            .Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IDomainEvent, CancellationToken>((e, _) =>
                eventoPublicado = e as CandidaturaRealizadaEvent)
            .Returns(Task.CompletedTask);

        var candidatura = Candidatura.Criar(Guid.NewGuid(), Guid.NewGuid());

        await _dispatcher.DispatchAndClearAsync(candidatura);

        Assert.NotNull(eventoPublicado);
        Assert.Equal(candidatura.Id, eventoPublicado!.CandidaturaId);
    }

    [Fact]
    public async Task DevePublicarEventosDeVaga()
    {
        SetupPublish(_mediatorMock);
        var vaga = Vaga.Criar("Tech Lead", "Descrição", "Requisitos", 18000);

        Assert.Single(vaga.DomainEvents);
        Assert.IsType<VagaPublicadaEvent>(vaga.DomainEvents.First());

        await _dispatcher.DispatchAndClearAsync(vaga);

        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Empty(vaga.DomainEvents);
    }
}
