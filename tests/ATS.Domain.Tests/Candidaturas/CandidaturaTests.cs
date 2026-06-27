using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Candidaturas.Events;
using ATS.Domain.Shared;
using Xunit;

namespace ATS.Domain.Tests.Candidaturas;

public class CandidaturaTests
{
    private static readonly Guid _guidCandidato = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid _guidVaga = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid _guidVaga2 = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");


    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public void DeveCriarCandidaturaComTodasAsPropriedadesCorretamenteDefinidas(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);
        var antes = DateTime.UtcNow;


        var candidatura = Candidatura.Criar(candidatoId, vagaId);


        Assert.NotEqual(Guid.Empty, candidatura.Id);
        Assert.Equal(candidatoId, candidatura.CandidatoId);
        Assert.Equal(vagaId, candidatura.VagaId);
        Assert.Equal(StatusCandidatura.EmAnalise, candidatura.Status);
        Assert.Null(candidatura.Observacoes);
        Assert.True(candidatura.DataCandidatura >= antes);
        Assert.True(candidatura.DataCandidatura <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveGerarIdUnicoParaCadaCandidaturaCriada(string candidatoIdStr)
    {

        var candidatoId = Guid.Parse(candidatoIdStr);


        var a = Candidatura.Criar(candidatoId, _guidVaga);
        var b = Candidatura.Criar(candidatoId, _guidVaga2);


        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(Guid.Empty, b.Id);
        Assert.NotEqual(a.Id, b.Id);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveDispararExatamenteUmEventoCandidaturaRealizadaAoCriar(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        var evento = Assert.Single(candidatura.DomainEvents);
        Assert.IsType<CandidaturaRealizadaEvent>(evento);
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoQuandoCandidatoIdForGuidVazio(string vagaIdStr)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Candidatura.Criar(Guid.Empty, Guid.Parse(vagaIdStr)));


        Assert.Equal("CandidatoId inválido.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoQuandoVagaIdForGuidVazio(string candidatoIdStr)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Empty));


        Assert.Equal("VagaId inválido.", excecao.Message);
    }





    [Theory]
    [InlineData("Perfil excelente para a vaga.")]
    [InlineData("Candidato aprovado na entrevista técnica.")]
    public void DeveAprovarCandidaturaComObservacoes(string observacoes)
    {

        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);


        candidatura.Aprovar(observacoes);


        Assert.Equal(StatusCandidatura.Aprovado, candidatura.Status);
        Assert.Equal(observacoes, candidatura.Observacoes);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveAprovarCandidaturaSemObservacoes(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        candidatura.Aprovar();


        Assert.Equal(StatusCandidatura.Aprovado, candidatura.Status);
        Assert.Null(candidatura.Observacoes);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveAdicionarEventoCandidaturaAprovadaAoAprovar(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        var quantidadeAntes = candidatura.DomainEvents.Count;


        candidatura.Aprovar();


        Assert.Equal(quantidadeAntes + 1, candidatura.DomainEvents.Count);
        Assert.IsType<CandidaturaAprovadaEvent>(candidatura.DomainEvents.Last());
    }


    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoAoAprovarCandidaturaJaAprovada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Aprovar();


        var excecao = Assert.Throws<DomainException>(() => candidatura.Aprovar());


        Assert.Equal("Somente candidaturas 'Em Análise' podem ser aprovadas.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoAoAprovarCandidaturaJaReprovada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Reprovar();


        var excecao = Assert.Throws<DomainException>(() => candidatura.Aprovar());


        Assert.Equal("Somente candidaturas 'Em Análise' podem ser aprovadas.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoAoAprovarCandidaturaJaCancelada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Cancelar();


        var excecao = Assert.Throws<DomainException>(() => candidatura.Aprovar());


        Assert.Equal("Somente candidaturas 'Em Análise' podem ser aprovadas.", excecao.Message);
    }





    [Theory]
    [InlineData("Experiência insuficiente para o cargo.")]
    [InlineData("Candidato não atingiu a nota mínima na avaliação técnica.")]
    public void DeveReprovarCandidaturaComObservacoes(string observacoes)
    {

        var candidatura = Candidatura.Criar(_guidCandidato, _guidVaga);


        candidatura.Reprovar(observacoes);


        Assert.Equal(StatusCandidatura.Reprovado, candidatura.Status);
        Assert.Equal(observacoes, candidatura.Observacoes);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveReprovarCandidaturaSemObservacoes(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        candidatura.Reprovar();


        Assert.Equal(StatusCandidatura.Reprovado, candidatura.Status);
        Assert.Null(candidatura.Observacoes);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveAdicionarEventoCandidaturaReprovadaAoReprovar(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        var quantidadeAntes = candidatura.DomainEvents.Count;


        candidatura.Reprovar();


        Assert.Equal(quantidadeAntes + 1, candidatura.DomainEvents.Count);
        Assert.IsType<CandidaturaReprovadaEvent>(candidatura.DomainEvents.Last());
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoAoReprovarCandidaturaJaReprovada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Reprovar();


        var excecao = Assert.Throws<DomainException>(() => candidatura.Reprovar());


        Assert.Equal("Somente candidaturas 'Em Análise' podem ser reprovadas.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoAoReprovarCandidaturaJaAprovada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Aprovar();


        var excecao = Assert.Throws<DomainException>(() => candidatura.Reprovar());


        Assert.Equal("Somente candidaturas 'Em Análise' podem ser reprovadas.", excecao.Message);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoAoReprovarCandidaturaJaCancelada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Cancelar();


        var excecao = Assert.Throws<DomainException>(() => candidatura.Reprovar());


        Assert.Equal("Somente candidaturas 'Em Análise' podem ser reprovadas.", excecao.Message);
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveCancelarCandidaturaComStatusEmAnalise(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        Assert.Equal(StatusCandidatura.EmAnalise, candidatura.Status);


        candidatura.Cancelar();


        Assert.Equal(StatusCandidatura.Cancelado, candidatura.Status);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveCancelarCandidaturaAprovada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Aprovar();


        candidatura.Cancelar();


        Assert.Equal(StatusCandidatura.Cancelado, candidatura.Status);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveCancelarCandidaturaReprovada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Reprovar();


        candidatura.Cancelar();


        Assert.Equal(StatusCandidatura.Cancelado, candidatura.Status);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveAdicionarEventoCandidaturaCanceladaAoCancelar(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        var quantidadeAntes = candidatura.DomainEvents.Count;


        candidatura.Cancelar();


        Assert.Equal(quantidadeAntes + 1, candidatura.DomainEvents.Count);
        Assert.IsType<CandidaturaCanceladaEvent>(candidatura.DomainEvents.Last());
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveLancarExcecaoAoCancelarCandidaturaJaCancelada(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        candidatura.Cancelar();


        var excecao = Assert.Throws<DomainException>(() => candidatura.Cancelar());


        Assert.Equal("Candidatura já foi cancelada.", excecao.Message);
    }





    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DevePermitirCancelarAposAprovar(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        candidatura.Aprovar("Aprovado na entrevista.");
        candidatura.Cancelar();


        Assert.Equal(StatusCandidatura.Cancelado, candidatura.Status);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DevePermitirCancelarAposReprovar(
        string candidatoIdStr, string vagaIdStr)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        candidatura.Reprovar("Perfil não compatível.");
        candidatura.Cancelar();


        Assert.Equal(StatusCandidatura.Cancelado, candidatura.Status);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "cccccccc-cccc-cccc-cccc-cccccccccccc",
                "Candidato aprovado com distinção.")]
    public void DeveObservacoesDeAprovarNaoSeremSobrescritas(
        string candidatoIdStr, string vagaIdStr, string observacoes)
    {

        var candidatura = Candidatura.Criar(
            Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));


        candidatura.Aprovar(observacoes);
        candidatura.Cancelar();


        Assert.Equal(observacoes, candidatura.Observacoes);
        Assert.Equal(StatusCandidatura.Cancelado, candidatura.Status);
    }
}
