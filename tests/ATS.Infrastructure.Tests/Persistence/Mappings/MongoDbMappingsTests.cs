using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Infrastructure.Persistence.Context;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace ATS.Infrastructure.Tests.Persistence.Mappings;

public class MongoDbMappingsTests
{
    public MongoDbMappingsTests()
    {
        _ = new MongoDbContext(new MongoDbSettings());
    }

    [Fact]
    public void DeveSerializarEDesserializarCandidatoComValueObjects()
    {
        var candidato = Candidato.Criar(
            "Maria Silva",
            "maria.silva@example.com",
            "(11) 99999-8888");
        candidato.AdicionarCurriculo("curriculo.pdf", "application/pdf", "base64");

        var bson = candidato.ToBsonDocument();
        var restaurado = BsonSerializer.Deserialize<Candidato>(bson);

        Assert.False(bson.Contains("domainEvents"));
        Assert.Equal(candidato.Id, restaurado.Id);
        Assert.Equal(candidato.Nome, restaurado.Nome);
        Assert.Equal(candidato.Email.Value, restaurado.Email.Value);
        Assert.Equal(candidato.Telefone.Value, restaurado.Telefone.Value);
        Assert.NotNull(restaurado.Curriculo);
        Assert.Equal("curriculo.pdf", restaurado.Curriculo.NomeArquivo);
        Assert.NotEqual(default, restaurado.Curriculo.DataUpload);
    }

    [Fact]
    public void DeveSerializarEDesserializarVagaComSalarioEStatusComoString()
    {
        var vaga = Vaga.Criar(
            "Desenvolvedor Back-end",
            "Descricao da vaga",
            "Requisitos",
            12000);
        vaga.Fechar();

        var bson = vaga.ToBsonDocument();
        var restaurada = BsonSerializer.Deserialize<Vaga>(bson);

        Assert.Equal("Fechada", bson["status"].AsString);
        Assert.Equal(vaga.Id, restaurada.Id);
        Assert.Equal(vaga.Titulo, restaurada.Titulo);
        Assert.Equal(vaga.Salario.Valor, restaurada.Salario.Valor);
        Assert.Equal(vaga.Salario.Moeda, restaurada.Salario.Moeda);
        Assert.Equal(StatusVaga.Fechada, restaurada.Status);
        Assert.NotNull(restaurada.DataEncerramento);
    }

    [Fact]
    public void DeveSerializarEDesserializarCandidaturaComStatusComoString()
    {
        var candidatura = Candidatura.Criar(Guid.NewGuid(), Guid.NewGuid());
        candidatura.Aprovar("Perfil aderente");

        var bson = candidatura.ToBsonDocument();
        var restaurada = BsonSerializer.Deserialize<Candidatura>(bson);

        Assert.Equal("Aprovado", bson["status"].AsString);
        Assert.Equal(candidatura.Id, restaurada.Id);
        Assert.Equal(candidatura.CandidatoId, restaurada.CandidatoId);
        Assert.Equal(candidatura.VagaId, restaurada.VagaId);
        Assert.Equal(StatusCandidatura.Aprovado, restaurada.Status);
        Assert.Equal("Perfil aderente", restaurada.Observacoes);
    }
}
