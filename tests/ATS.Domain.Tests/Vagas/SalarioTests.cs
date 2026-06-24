using ATS.Domain.Shared;
using ATS.Domain.Vagas.ValueObjects;
using Xunit;

namespace ATS.Domain.Tests.Vagas;

public class SalarioTests
{

    [Theory]
    [InlineData(5000, "BRL", "BRL")]
    [InlineData(3500.50, "usd", "USD")]
    [InlineData(1200.75, "eur", "EUR")]
    [InlineData(0, "BRL", "BRL")]
    public void DeveCriarSalarioComPropriedadesCorretamenteDefinidas(
        decimal valor, string moedaEntrada, string moedaEsperada)
    {

        var salario = Salario.Create(valor, moedaEntrada);


        Assert.Equal(valor, salario.Valor);
        Assert.Equal(moedaEsperada, salario.Moeda);
    }

    [Theory]
    [InlineData(3000)]
    [InlineData(0)]
    [InlineData(15000.99)]
    public void DeveUsarBRLComoMoedaPadraoQuandoNaoInformada(decimal valor)
    {

        var salario = Salario.Create(valor);


        Assert.Equal("BRL", salario.Moeda);
        Assert.Equal(valor, salario.Valor);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    [InlineData(-9999999)]
    public void DeveLancarExcecaoQuandoValorForNegativo(decimal valor)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Salario.Create(valor));


        Assert.Equal("Salário não pode ser negativo.", excecao.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveLancarExcecaoQuandoMoedaForNulaOuVazia(string? moeda)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Salario.Create(1000, moeda!));


        Assert.Equal("Moeda é obrigatória.", excecao.Message);
    }

    [Theory]
    [InlineData(0, "BRL")]
    public void DeveCriarSalarioIndefinidoComValorZeroEMoedaBRL(
        decimal valorEsperado, string moedaEsperada)
    {

        var salario = Salario.Indefinido();


        Assert.Equal(valorEsperado, salario.Valor);
        Assert.Equal(moedaEsperada, salario.Moeda);
    }

    [Theory]
    [InlineData(1500, "BRL", "BRL")]
    [InlineData(3000.50, "USD", "USD")]
    [InlineData(0, "EUR", "EUR")]
    public void DeveFormatarSalarioComMoedaEValorNaRepresentacaoTextual(
        decimal valor, string moeda, string moedaEsperada)
    {

        var salario = Salario.Create(valor, moeda);


        var texto = salario.ToString();


        Assert.StartsWith(moedaEsperada, texto);
        Assert.Contains(valor.ToString("N2"), texto
            .Replace(moedaEsperada, "")
            .Trim());
    }

    [Theory]
    [InlineData(5000, "BRL", "USD")]
    [InlineData(0, "EUR", "BRL")]
    public void DeveUsarMoedaComoSegundoComponenteNaComparacaoDeIgualdade(
        decimal valor, string moedaA, string moedaB)
    {

        var salarioA = Salario.Create(valor, moedaA);
        var salarioB = Salario.Create(valor, moedaB);


        var saoIguais = salarioA.Equals((object)salarioB);


        Assert.False(saoIguais);
    }

    [Theory]
    [InlineData(5000, "BRL", 5000, "USD")]
    [InlineData(1000, "BRL", 2000, "BRL")]
    [InlineData(3500, "EUR", 7000, "USD")]
    public void DeveGerarHashCodesDiferentesParaSalariosDistintos(
        decimal valorA, string moedaA, decimal valorB, string moedaB)
    {

        var salarioA = Salario.Create(valorA, moedaA);
        var salarioB = Salario.Create(valorB, moedaB);


        var hashA = salarioA.GetHashCode();
        var hashB = salarioB.GetHashCode();


        Assert.NotEqual(hashA, hashB);
    }

    [Theory]
    [InlineData(5000, "BRL")]
    [InlineData(3500.50, "USD")]
    [InlineData(0, "EUR")]
    public void DeveConsiderarIguaisOsSalariosComMesmoValorEMoeda(
        decimal valor, string moeda)
    {

        var salarioA = Salario.Create(valor, moeda);
        var salarioB = Salario.Create(valor, moeda);


        Assert.Equal(salarioA, salarioB);
        Assert.Equal(salarioA.GetHashCode(), salarioB.GetHashCode());
    }
}
