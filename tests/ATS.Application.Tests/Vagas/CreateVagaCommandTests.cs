using ATS.Application.Vagas.Commands.CreateVaga;

namespace ATS.Application.Tests.Vagas;

public class CreateVagaCommandTests
{
    [Theory]
    [InlineData("Dev Back-end", "Descrição", "Requisitos", 12000, "BRL")]
    [InlineData("Tech Lead", "Outra desc", null, 0, "USD")]
    public void DeveCriarCommandComPropriedadesDefinidas(
        string titulo, string descricao, string? requisitos,
        decimal salario, string moeda)
    {
        var command = new CreateVagaCommand(titulo, descricao, requisitos, salario, moeda);

        Assert.Equal(titulo, command.Titulo);
        Assert.Equal(descricao, command.Descricao);
        Assert.Equal(requisitos, command.Requisitos);
        Assert.Equal(salario, command.Salario);
        Assert.Equal(moeda, command.Moeda);
    }

    [Theory]
    [InlineData("Dev Back-end", "Descrição")]
    public void DeveUsarValoresPadraoQuandoOmitidos(string titulo, string descricao)
    {
        var command = new CreateVagaCommand(titulo, descricao);

        Assert.Null(command.Requisitos);
        Assert.Equal(0m, command.Salario);
        Assert.Equal("BRL", command.Moeda);
    }

    [Theory]
    [InlineData("Dev Back-end", "Desc", "Req", 10000, "BRL")]
    public void DeveSerIgualAOutroCommandComMesmosValores(
        string titulo, string desc, string req, decimal salario, string moeda)
    {
        Assert.Equal(
            new CreateVagaCommand(titulo, desc, req, salario, moeda),
            new CreateVagaCommand(titulo, desc, req, salario, moeda));
    }
}
