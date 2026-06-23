using ATS.Domain.Candidatos.ValueObjects;
using ATS.Domain.Shared;
using Xunit;

namespace ATS.Domain.Tests.Candidatos;


public class TelefoneTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DeveLancarErroQuandoForNuloOuVazio(string numero)
    {
        var excecao = Assert.Throws<DomainException>(
           () => Telefone.Create(numero));

        Assert.Equal("Telefone não pode ser vazio.", excecao.Message);
    }

    [Theory]
    [InlineData("999999999")]
    [InlineData("999999999999")]
    public void DeveLancarErroQuandoForMenorQue10EMaiorQue11(string numero)
    {
        var excecao = Assert.Throws<DomainException>(
           () => Telefone.Create(numero));

        Assert.Equal("Telefone deve ter 10 ou 11 dígitos.", excecao.Message);
    }

    [Theory]
    [InlineData("35999912345")]
    [InlineData("3534322399")]
    public void DeveCriarInstanciaValidas(string numero)
    {
        var telefone = Telefone.Create(numero);

        Assert.Equal(numero, telefone.Value);
        Assert.Equal(numero, telefone.ToString());
        Assert.Equal(telefone, Telefone.Create(numero));
    }


}
