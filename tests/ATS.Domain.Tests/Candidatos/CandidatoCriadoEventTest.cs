using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Events;
using Xunit;

namespace ATS.Domain.Tests.Candidatos;

public class CandidatoCriadoEventTests
{


    [Theory]
    [InlineData("João da Silva")]
    [InlineData("Maria Santos")]
    [InlineData("Ana Lima")]
    public void DeveCriarEventoComCandidatoIdENomeCorretamenteDefinidos(string nome)
    {

        var candidatoId = Guid.NewGuid();


        var evento = new CandidatoCriadoEvent(candidatoId, nome);


        Assert.Equal(candidatoId, evento.CandidatoId);
        Assert.Equal(nome, evento.Nome);
    }


    [Theory]
    [InlineData("João da Silva")]
    [InlineData("Maria Santos")]
    public void DeveDefinirOcorridoEmComoDataUtcNaCriacao(string nome)
    {

        var antes = DateTime.UtcNow;


        var evento = new CandidatoCriadoEvent(Guid.NewGuid(), nome);


        Assert.Equal(DateTimeKind.Utc, evento.OcorridoEm.Kind);


        Assert.True(evento.OcorridoEm >= antes);
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow);
    }


    [Theory]
    [InlineData("João da Silva", "Maria Santos")]
    public void DeveGerarOcorridoEmIndependenteParaCadaInstancia(
        string nomeA, string nomeB)
    {

        var eventoA = new CandidatoCriadoEvent(Guid.NewGuid(), nomeA);
        var eventoB = new CandidatoCriadoEvent(Guid.NewGuid(), nomeB);

        Assert.NotSame(eventoA, eventoB);
        Assert.True(eventoA.OcorridoEm <= eventoB.OcorridoEm);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    [InlineData("Maria Santos", "maria@empresa.com.br", "21987654321")]
    public void DeveConterIdENomeDoAgregadoQuandoDisparadoPeloCandidato(
        string nome, string email, string telefone)
    {

        var candidato = Candidato.Criar(nome, email, telefone);
        var evento = candidato.DomainEvents.OfType<CandidatoCriadoEvent>().Single();


        Assert.Equal(candidato.Id, evento.CandidatoId);
        Assert.Equal(candidato.Nome, evento.Nome);
    }

    [Theory]
    [InlineData("  João Silva  ", "João Silva")]
    [InlineData("  Ana Lima", "Ana Lima")]
    public void DeveConterNomeTrimadoQuandoDisparadoPeloCandidato(
        string nomeComEspacos, string nomeEsperado)
    {

        var candidato = Candidato.Criar(nomeComEspacos, "joao@email.com", "11912345678");


        var evento = candidato.DomainEvents.OfType<CandidatoCriadoEvent>().Single();


        Assert.Equal(nomeEsperado, evento.Nome);
        Assert.Equal(candidato.Nome, evento.Nome);
    }

    [Theory]
    [InlineData("João da Silva")]
    public void DeveOcorridoEmDoEventoSerAnteriorOuIgualADataCadastroDoAgregado(string nome)
    {

        var candidato = Candidato.Criar(nome, "joao@email.com", "11912345678");
        var evento = candidato.DomainEvents.OfType<CandidatoCriadoEvent>().Single();


        Assert.True(evento.OcorridoEm <= DateTime.UtcNow);
        Assert.True(evento.OcorridoEm >= candidato.DataCadastro.AddSeconds(-1));
    }
}


