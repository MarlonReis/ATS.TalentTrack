namespace ATS.Application.Tests.Candidatos;

using ATS.Application.Candidatos.Commands.CreateCandidato;

public class CreateCandidatoCommandValidatorTests
{
    private readonly CreateCandidatoCommandValidator _validator = new();

    private static CreateCandidatoCommand Valido(
        string nome = "João Silva",
        string email = "joao@example.com",
        string telefone = "11999990000") =>
        new(nome, email, telefone);

    [Fact]
    public async Task DeveAceitarComandoValido()
    {
        var result = await _validator.ValidateAsync(Valido());

        Assert.True(result.IsValid);
    }

    // Nome
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeveRejeitarNomeVazio(string nome)
    {
        var result = await _validator.ValidateAsync(Valido(nome: nome));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome" && e.ErrorMessage == "Nome é obrigatório.");
    }

    [Fact]
    public async Task DeveRejeitarNomeComMaisDe200Caracteres()
    {
        var result = await _validator.ValidateAsync(Valido(nome: new string('A', 201)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome" && e.ErrorMessage == "Nome não pode exceder 200 caracteres.");
    }

    [Fact]
    public async Task DeveAceitarNomeComExatamente200Caracteres()
    {
        var result = await _validator.ValidateAsync(Valido(nome: new string('A', 200)));

        Assert.True(result.IsValid);
    }

    // Email
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeveRejeitarEmailVazio(string email)
    {
        var result = await _validator.ValidateAsync(Valido(email: email));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "E-mail é obrigatório.");
    }

    [Theory]
    [InlineData("semdominio")]
    [InlineData("sem@")]
    [InlineData("@semlocal.com")]
    public async Task DeveRejeitarEmailInvalido(string email)
    {
        var result = await _validator.ValidateAsync(Valido(email: email));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "E-mail inválido.");
    }

    [Fact]
    public async Task DeveRejeitarEmailComMaisDe254Caracteres()
    {
        var local = new string('a', 243);
        var email = $"{local}@example.com"; // 243 + 11 = 254+1 = 255

        var result = await _validator.ValidateAsync(Valido(email: email));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "E-mail não pode exceder 254 caracteres.");
    }

    [Fact]
    public async Task DeveAceitarEmailComExatamente254Caracteres()
    {
        var local = new string('a', 242);
        var email = $"{local}@example.com"; // 242 + 12 = 254

        var result = await _validator.ValidateAsync(Valido(email: email));

        Assert.True(result.IsValid);
    }

    // Telefone
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeveRejeitarTelefoneVazio(string telefone)
    {
        var result = await _validator.ValidateAsync(Valido(telefone: telefone));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Telefone" && e.ErrorMessage == "Telefone é obrigatório.");
    }

    [Theory]
    [InlineData("1234567")] // menos de 8 dígitos
    [InlineData("abc")]
    [InlineData("123456789012345678901")] // mais de 20 chars
    public async Task DeveRejeitarTelefoneInvalido(string telefone)
    {
        var result = await _validator.ValidateAsync(Valido(telefone: telefone));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Telefone");
    }

    [Theory]
    [InlineData("11999990000")]
    [InlineData("+5511999990000")]
    [InlineData("(11) 99999-0000")]
    public async Task DeveAceitarTelefonesValidos(string telefone)
    {
        var result = await _validator.ValidateAsync(Valido(telefone: telefone));

        Assert.True(result.IsValid);
    }
}
