using ATS.Application.Vagas.DTOs;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using Xunit;

namespace ATS.Application.Tests.Vagas;

public class VagaDtoTests
{
    private static Vaga CriarVaga(
        string titulo = "Dev Back-end Sênior",
        string descricao = "Vaga para desenvolvedor .NET",
        string requisitos = "5+ anos, .NET 8+",
        decimal salario = 12000m) =>
        Vaga.Criar(titulo, descricao, requisitos, salario);


    [Theory]
    [InlineData("Dev Back-end", "Descrição da vaga", "Requisitos", 12000)]
    [InlineData("Tech Lead", "Liderar equipe", "10+ anos exp", 18000)]
    public void DeveMapearVagaAbertaParaDtoCorretamente(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        var antes = DateTime.UtcNow;
        var vaga = CriarVaga(titulo, descricao, requisitos, salario);

        var dto = VagaDto.FromDomain(vaga);

        Assert.Equal(vaga.Id, dto.Id);
        Assert.Equal(titulo, dto.Titulo);
        Assert.Equal(descricao, dto.Descricao);
        Assert.Equal(requisitos, dto.Requisitos);
        Assert.Equal(salario, dto.Salario);
        Assert.Equal("BRL", dto.Moeda);
        Assert.Equal(StatusVaga.Aberta, dto.Status);
        Assert.Equal("Aberta", dto.StatusDescricao);
        Assert.True(dto.DataAbertura >= antes);
        Assert.Null(dto.DataEncerramento);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    [InlineData("Tech Lead")]
    public void DeveMapearVagaFechadaComDataEncerramentoEStatusDescricaoCorretos(string titulo)
    {

        var vaga = CriarVaga(titulo);
        vaga.Fechar();


        var dto = VagaDto.FromDomain(vaga);


        Assert.Equal(StatusVaga.Fechada, dto.Status);
        Assert.Equal("Fechada", dto.StatusDescricao);
        Assert.NotNull(dto.DataEncerramento);
    }

    [Theory]
    [InlineData(StatusVaga.Aberta, "Aberta")]
    [InlineData(StatusVaga.Fechada, "Fechada")]
    public void DeveRetornarStatusDescricaoCorretoParaCadaStatus(
        StatusVaga status, string descricaoEsperada)
    {

        var vaga = CriarVaga();
        if (status == StatusVaga.Fechada)
        {
            vaga.Fechar();
        }

        var dto = VagaDto.FromDomain(vaga);

        Assert.Equal(status, dto.Status);
        Assert.Equal(descricaoEsperada, dto.StatusDescricao);
    }

    [Theory]
    [InlineData(5000)]
    [InlineData(3000)]
    public void DeveMapearSalarioComMoedaCorretamente(decimal salario)
    {
        var vaga = Vaga.Criar("Titulo", "Desc", "Req", salario);

        var dto = VagaDto.FromDomain(vaga);

        Assert.Equal(salario, dto.Salario);
        Assert.NotEmpty(dto.Moeda);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    public void DeveSerIgualAOutroDtoComMesmosValores(string titulo)
    {
        var vaga = CriarVaga(titulo);
        var a = VagaDto.FromDomain(vaga);
        var b = VagaDto.FromDomain(vaga);

        Assert.Equal(a, b);
    }
}
