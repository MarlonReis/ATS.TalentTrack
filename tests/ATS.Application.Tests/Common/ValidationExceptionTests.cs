namespace ATS.Application.Tests.Common;

using ATS.Application.Common.Validation;
using FluentValidation.Results;

public class ValidationExceptionTests
{
    [Fact]
    public void DeveDefinirMensagemPadrao()
    {
        var ex = new ValidationException([]);

        Assert.Equal("Um ou mais erros de validação ocorreram.", ex.Message);
    }

    [Fact]
    public void DeveArmazenarListaDeErros()
    {
        var erros = new[]
        {
            new ValidationFailure("Nome", "Nome é obrigatório."),
            new ValidationFailure("Email", "E-mail inválido.")
        };

        var ex = new ValidationException(erros);

        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains(ex.Errors, e => e.PropertyName == "Nome");
        Assert.Contains(ex.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void DeveRetornarListaVaziaQuandoNenhumErroForPassado()
    {
        var ex = new ValidationException([]);

        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void DeveSerSubclasseDeException()
    {
        var ex = new ValidationException([]);

        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void DevePreservarMensagemDeErroDoFailure()
    {
        var failure = new ValidationFailure("Telefone", "Telefone inválido.");

        var ex = new ValidationException([failure]);

        Assert.Equal("Telefone inválido.", ex.Errors[0].ErrorMessage);
        Assert.Equal("Telefone", ex.Errors[0].PropertyName);
    }

    [Fact]
    public void DeveConvertarEnumerableParaListaReadOnly()
    {
        IEnumerable<ValidationFailure> erros = [
            new ValidationFailure("A", "msg A"),
            new ValidationFailure("B", "msg B"),
            new ValidationFailure("C", "msg C"),
        ];

        var ex = new ValidationException(erros);

        Assert.IsAssignableFrom<IReadOnlyList<ValidationFailure>>(ex.Errors);
        Assert.Equal(3, ex.Errors.Count);
    }
}
