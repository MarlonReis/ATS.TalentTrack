
using ATS.Application.Vagas.Commands.UpdateVaga;

namespace ATS.Application.Tests.Vagas;

public class UpdateVagaCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "Novo Titulo", "Nova Desc", "Req", 15000)]
    public void DeveCriarCommandComPropriedadesDefinidas(
        string idStr, string titulo, string descricao, string? requisitos, decimal salario)
    {
        var id = Guid.Parse(idStr);
        var command = new UpdateVagaCommand(id, titulo, descricao, requisitos, salario);

        Assert.Equal(id, command.Id);
        Assert.Equal(titulo, command.Titulo);
        Assert.Equal(descricao, command.Descricao);
        Assert.Equal(requisitos, command.Requisitos);
        Assert.Equal(salario, command.Salario);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "Titulo", "Desc")]
    public void DeveUsarValoresPadraoQuandoOmitidos(string idStr, string titulo, string desc)
    {
        var command = new UpdateVagaCommand(Guid.Parse(idStr), titulo, desc);
        Assert.Null(command.Requisitos);
        Assert.Equal(0m, command.Salario);
    }
}
