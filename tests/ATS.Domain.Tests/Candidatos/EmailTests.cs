using ATS.Domain.Candidatos.ValueObjects;
using ATS.Domain.Shared;
using Xunit;

namespace ATS.Domain.Tests.Candidatos;

public class EmailTests
{

    [Theory]
    [InlineData("joao@email.com", "joao@email.com")]
    [InlineData("JOAO@EMAIL.COM", "joao@email.com")]
    public void CreateComEmailValidoDeveRetornarValueObjectNormalizado(string entrada, string esperado)
    {

        var email = Email.Create(entrada);

        Assert.Equal(esperado, email.Value);
        Assert.Equal(esperado, email.ToString());

        var emailDuplicado = Email.Create(entrada);
        Assert.Equal(email, emailDuplicado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateComEmailNuloOuVazioDeveLancarDomainExceptionComMensagemCorreta(
        string? emailInvalido)
    {
        var excecao = Assert.Throws<DomainException>(
            () => Email.Create(emailInvalido!));

        Assert.Equal("E-mail não pode ser vazio.", excecao.Message);
    }

    [Theory]
    [InlineData("emailsemarroba.com")]    
    [InlineData("@semusuario.com")]       
    [InlineData("usuario@")]             
    [InlineData("usuario@semextensao")]
    [InlineData("usu ario@email.com")]
    public void CreateComFormatoInvalidoDeveLancarDomainException(string emailMalFormado)
    {
        var excecao = Assert.Throws<DomainException>(
            () => Email.Create(emailMalFormado));

        Assert.Contains(emailMalFormado.Trim(), excecao.Message);
        Assert.Contains("formato inválido", excecao.Message);
    }
}