using ATS.Application.Candidaturas.Commands.CandidatarSe;
using ATS.Application.Common.Events;
using ATS.Application.Common.Validation;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Repositories;
using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Candidaturas;

public class CandidatarSeHandlerTests
{

    private readonly Mock<ICandidaturaRepository> _candidaturaRepoMock;
    private readonly Mock<ICandidatoRepository> _candidatoRepoMock;
    private readonly Mock<IVagaRepository> _vagaRepoMock;
    private readonly Mock<IDomainEventDispatcher> _dispatcherMock;
    private readonly CandidatarSeHandler _handler;

    private static readonly Guid _guidCandidato = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _guidVaga = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public CandidatarSeHandlerTests()
    {

        _candidaturaRepoMock = new Mock<ICandidaturaRepository>(MockBehavior.Strict);
        _candidatoRepoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _vagaRepoMock = new Mock<IVagaRepository>(MockBehavior.Strict);
        _dispatcherMock = new Mock<IDomainEventDispatcher>();
        _dispatcherMock
            .Setup(d => d.DispatchAndClearAsync(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CandidatarSeHandler(
            _candidaturaRepoMock.Object,
            _candidatoRepoMock.Object,
            _vagaRepoMock.Object,
            _dispatcherMock.Object,
            new CandidatarSeCommandValidator(),
            NullLogger<CandidatarSeHandler>.Instance);
    }

    private static Candidato CriarCandidato(
        string nome = "João da Silva",
        string email = "joao@email.com",
        string tel = "11912345678") =>
        Candidato.Criar(nome, email, tel);

    private static Vaga CriarVagaAberta(
        string titulo = "Dev Back-end Sênior",
        string descricao = "Descrição da vaga",
        string requisitos = "Requisitos",
        decimal salario = 12000m)
    {
        return Vaga.Criar(titulo, descricao, requisitos, salario);
    }

    private static Vaga CriarVagaFechada(string titulo = "Vaga Fechada")
    {
        var vaga = Vaga.Criar(titulo, "Descrição", "Requisitos", 8000);
        vaga.Fechar();
        return vaga;
    }


    private void SetupCandidatoEncontrado(Guid candidatoId, Candidato candidato)
    {
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(candidatoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);
    }

    private void SetupCandidatoNaoEncontrado(Guid candidatoId)
    {
        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(candidatoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);
    }

    private void SetupVagaEncontrada(Guid vagaId, Vaga vaga)
    {
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);
    }

    private void SetupVagaNaoEncontrada(Guid vagaId)
    {
        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vaga?)null);
    }

    private void SetupNaoCandidatouAinda(Guid candidatoId, Guid vagaId)
    {
        _candidaturaRepoMock
            .Setup(r => r.ExisteAsync(candidatoId, vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupJaCandidatou(Guid candidatoId, Guid vagaId)
    {
        _candidaturaRepoMock
            .Setup(r => r.ExisteAsync(candidatoId, vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupAdicionarCandidatura()
    {
        _candidaturaRepoMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Domain.Candidaturas.Entities.Candidatura>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public async Task DeveCriarESalvarCandidaturaQuandoTodosOsDadosSaoValidos(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        SetupCandidatoEncontrado(candidatoId, CriarCandidato());
        SetupVagaEncontrada(vagaId, CriarVagaAberta());
        SetupNaoCandidatouAinda(candidatoId, vagaId);
        SetupAdicionarCandidatura();

        var command = new CandidatarSeCommand(candidatoId, vagaId);


        var dto = await _handler.HandleAsync(command);


        Assert.NotNull(dto);
        _candidaturaRepoMock.Verify(
            r => r.AdicionarAsync(
                It.IsAny<Domain.Candidaturas.Entities.Candidatura>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678",
                "Dev Back-end Sênior", 12000)]
    [InlineData("Maria Santos", "maria@corp.com", "21987654321",
                "Tech Lead", 18000)]
    public async Task DeveRetornarDtoComDadosDoCandidatoEDaVaga(
        string nome, string email, string telefone,
        string tituloVaga, decimal salario)
    {

        var candidato = CriarCandidato(nome, email, telefone);
        var vaga = CriarVagaAberta(tituloVaga, salario: salario);

        SetupCandidatoEncontrado(_guidCandidato, candidato);
        SetupVagaEncontrada(_guidVaga, vaga);
        SetupNaoCandidatouAinda(_guidCandidato, _guidVaga);
        SetupAdicionarCandidatura();

        var command = new CandidatarSeCommand(_guidCandidato, _guidVaga);


        var dto = await _handler.HandleAsync(command);


        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal(_guidCandidato, dto.CandidatoId);
        Assert.Equal(_guidVaga, dto.VagaId);
        Assert.Equal(candidato.Nome, dto.NomeCandidato);
        Assert.Equal(vaga.Titulo, dto.TituloVaga);
        Assert.Equal(StatusCandidatura.EmAnalise, dto.Status);
        Assert.Equal("Em Análise", dto.StatusDescricao);
        Assert.Null(dto.Observacoes);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveSalvarCandidaturaComCandidatoIdEVagaIdCorretos(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        SetupCandidatoEncontrado(candidatoId, CriarCandidato());
        SetupVagaEncontrada(vagaId, CriarVagaAberta());
        SetupNaoCandidatouAinda(candidatoId, vagaId);
        SetupAdicionarCandidatura();

        var command = new CandidatarSeCommand(candidatoId, vagaId);


        await _handler.HandleAsync(command);


        _candidaturaRepoMock.Verify(
            r => r.AdicionarAsync(
                It.Is<Domain.Candidaturas.Entities.Candidatura>(c =>
                    c.CandidatoId == candidatoId &&
                    c.VagaId == vagaId &&
                    c.Status == StatusCandidatura.EmAnalise),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveRepassarCancellationTokenParaTodosOsRepositorios(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(candidatoId, ct))
            .ReturnsAsync(CriarCandidato());

        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(vagaId, ct))
            .ReturnsAsync(CriarVagaAberta());

        _candidaturaRepoMock
            .Setup(r => r.ExisteAsync(candidatoId, vagaId, ct))
            .ReturnsAsync(false);

        _candidaturaRepoMock
            .Setup(r => r.AdicionarAsync(
                It.IsAny<Domain.Candidaturas.Entities.Candidatura>(), ct))
            .Returns(Task.CompletedTask);

        var command = new CandidatarSeCommand(candidatoId, vagaId);


        await _handler.HandleAsync(command, ct);


        _candidatoRepoMock.Verify(r => r.ObterPorIdAsync(candidatoId, ct), Times.Once);
        _vagaRepoMock.Verify(r => r.ObterPorIdAsync(vagaId, ct), Times.Once);
        _candidaturaRepoMock.Verify(r => r.ExisteAsync(candidatoId, vagaId, ct), Times.Once);
        _candidaturaRepoMock.Verify(
            r => r.AdicionarAsync(
                It.IsAny<Domain.Candidaturas.Entities.Candidatura>(), ct), Times.Once);
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public async Task DeveLancarExcecaoQuandoCandidatoNaoForEncontrado(
        string candidatoIdStr, string vagaIdStr)
    {

        SetupCandidatoNaoEncontrado(Guid.Parse(candidatoIdStr));
        var command = new CandidatarSeCommand(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(command));


        Assert.Equal("Candidato não encontrado.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveNaoConsultarVagaNemCandidaturaQuandoCandidatoNaoExistir(
        string candidatoIdStr, string vagaIdStr)
    {

        SetupCandidatoNaoEncontrado(Guid.Parse(candidatoIdStr));
        var command = new CandidatarSeCommand(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(command));


        _vagaRepoMock.Verify(
            r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _candidaturaRepoMock.Verify(
            r => r.ExisteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _candidaturaRepoMock.Verify(
            r => r.AdicionarAsync(
                It.IsAny<Domain.Candidaturas.Entities.Candidatura>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public async Task DeveLancarExcecaoQuandoVagaNaoForEncontrada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        SetupCandidatoEncontrado(candidatoId, CriarCandidato());
        SetupVagaNaoEncontrada(vagaId);
        var command = new CandidatarSeCommand(candidatoId, vagaId);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(command));


        Assert.Equal("Vaga não encontrada.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveNaoConsultarCandidaturaQuandoVagaNaoExistir(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        SetupCandidatoEncontrado(candidatoId, CriarCandidato());
        SetupVagaNaoEncontrada(vagaId);
        var command = new CandidatarSeCommand(candidatoId, vagaId);


        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(command));


        _candidaturaRepoMock.Verify(
            r => r.ExisteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _candidaturaRepoMock.Verify(
            r => r.AdicionarAsync(
                It.IsAny<Domain.Candidaturas.Entities.Candidatura>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public async Task DeveLancarExcecaoQuandoVagaEstiverFechada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        SetupCandidatoEncontrado(candidatoId, CriarCandidato());
        SetupVagaEncontrada(vagaId, CriarVagaFechada());
        var command = new CandidatarSeCommand(candidatoId, vagaId);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(command));


        Assert.Equal("Não é possível se candidatar a uma vaga fechada.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveNaoVerificarDuplicidadeNemSalvarQuandoVagaEstiverFechada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        SetupCandidatoEncontrado(candidatoId, CriarCandidato());
        SetupVagaEncontrada(vagaId, CriarVagaFechada());
        var command = new CandidatarSeCommand(candidatoId, vagaId);


        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(command));


        _candidaturaRepoMock.Verify(
            r => r.ExisteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _candidaturaRepoMock.Verify(
            r => r.AdicionarAsync(
                It.IsAny<Domain.Candidaturas.Entities.Candidatura>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }



    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public async Task DeveLancarExcecaoQuandoCandidatoJaCandidatouAVaga(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        SetupCandidatoEncontrado(candidatoId, CriarCandidato());
        SetupVagaEncontrada(vagaId, CriarVagaAberta());
        SetupJaCandidatou(candidatoId, vagaId);
        var command = new CandidatarSeCommand(candidatoId, vagaId);


        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(command));


        Assert.Equal("Candidato já se candidatou a esta vaga.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveNaoCriarNovaCandidaturaQuandoJaCandidatou(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        SetupCandidatoEncontrado(candidatoId, CriarCandidato());
        SetupVagaEncontrada(vagaId, CriarVagaAberta());
        SetupJaCandidatou(candidatoId, vagaId);
        var command = new CandidatarSeCommand(candidatoId, vagaId);


        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(command));


        _candidaturaRepoMock.Verify(
            r => r.AdicionarAsync(
                It.IsAny<Domain.Candidaturas.Entities.Candidatura>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public async Task DeveExecutarOperacoesNaOrdemCorreta(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);
        var ordemChamadas = new List<string>();

        _candidatoRepoMock
            .Setup(r => r.ObterPorIdAsync(candidatoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCandidato())
            .Callback(() => ordemChamadas.Add("ObterCandidato"));

        _vagaRepoMock
            .Setup(r => r.ObterPorIdAsync(vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarVagaAberta())
            .Callback(() => ordemChamadas.Add("ObterVaga"));

        _candidaturaRepoMock
            .Setup(r => r.ExisteAsync(candidatoId, vagaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Callback(() => ordemChamadas.Add("ExisteCandidatura"));

        _candidaturaRepoMock
            .Setup(r => r.AdicionarAsync(
                It.IsAny<Domain.Candidaturas.Entities.Candidatura>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => ordemChamadas.Add("AdicionarCandidatura"));

        var command = new CandidatarSeCommand(candidatoId, vagaId);


        await _handler.HandleAsync(command);


        Assert.Equal(
            new[] { "ObterCandidato", "ObterVaga", "ExisteCandidatura", "AdicionarCandidatura" },
            ordemChamadas);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "A0000000-0000-0000-0000-000000000001")]
    [InlineData("A0000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000000")]
    public async Task DeveLancarValidationExceptionQuandoIdsForemInvalidos(
        string candidatoIdStr, string vagaIdStr)
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.HandleAsync(
                new CandidatarSeCommand(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr))));

        Assert.NotEmpty(ex.Errors);
    }
}
