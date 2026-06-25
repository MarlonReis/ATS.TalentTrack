using ATS.Application.Candidatos.Queries.GetCandidatoById;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Repositories;
using ATS.Domain.Shared;
using Moq;
using Xunit;

namespace ATS.Application.Tests.Candidatos;

public class GetCandidatoByIdHandlerTests
{
    private readonly Mock<ICandidatoRepository> _repoMock;
    private readonly GetCandidatoByIdHandler _handler;

    private static readonly Guid _guidCandidato = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public GetCandidatoByIdHandlerTests()
    {
        _repoMock = new Mock<ICandidatoRepository>(MockBehavior.Strict);
        _handler = new GetCandidatoByIdHandler(_repoMock.Object);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    [InlineData("Maria Santos", "maria@empresa.com.br", "21987654321")]
    public async Task DeveRetornarDtoComDadosCorretos(
        string nome, string email, string telefone)
    {

        var candidato = Candidato.Criar(nome, email, telefone);

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);


        var dto = await _handler.HandleAsync(new GetCandidatoByIdQuery(_guidCandidato));


        Assert.Equal(candidato.Id, dto.Id);
        Assert.Equal(candidato.Nome, dto.Nome);
        Assert.Equal(candidato.Email.Value, dto.Email);
        Assert.Equal(candidato.Telefone.Value, dto.Telefone);
        Assert.False(dto.PossuiCurriculo);
        Assert.Null(dto.NomeCurriculo);
    }

    [Fact]
    public async Task DeveNaoChamarAtualizarAsync()
    {

        var candidato = Candidato.Criar("João", "joao@email.com", "11912345678");

        _repoMock
            .Setup(r => r.ObterPorIdAsync(_guidCandidato, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidato);


        await _handler.HandleAsync(new GetCandidatoByIdQuery(_guidCandidato));


        _repoMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Candidato>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveRepassarCancellationToken()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var candidato = Candidato.Criar("João", "joao@email.com", "11912345678");

        _repoMock.Setup(r => r.ObterPorIdAsync(_guidCandidato, ct)).ReturnsAsync(candidato);

        await _handler.HandleAsync(new GetCandidatoByIdQuery(_guidCandidato), ct);

        _repoMock.Verify(r => r.ObterPorIdAsync(_guidCandidato, ct), Times.Once);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public async Task DeveLancarExcecaoQuandoCandidatoNaoForEncontrado(string idStr)
    {
        _repoMock
            .Setup(r => r.ObterPorIdAsync(Guid.Parse(idStr), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidato?)null);

        var excecao = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new GetCandidatoByIdQuery(Guid.Parse(idStr))));

        Assert.Equal("Candidato não encontrado.", excecao.Message);
    }
}
