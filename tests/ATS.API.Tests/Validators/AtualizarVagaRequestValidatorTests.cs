namespace ATS.API.Tests.Validators;

using ATS.API.Requests.Vagas;

public class AtualizarVagaRequestValidatorTests
{
    private readonly AtualizarVagaRequestValidator _validator = new();

    private static AtualizarVagaRequest Valido(
        string titulo = "Dev Back-end",
        string descricao = "Descrição da vaga",
        string? requisitos = "C#",
        decimal salario = 10000m) =>
        new(titulo, descricao, requisitos, salario);

    [Fact]
    public async Task DeveAceitarRequestValido()
    {
        var result = await _validator.ValidateAsync(Valido());

        Assert.True(result.IsValid);
    }

    // Titulo
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeveRejeitarTituloVazio(string titulo)
    {
        var result = await _validator.ValidateAsync(Valido(titulo: titulo));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Titulo" && e.ErrorMessage == "Título é obrigatório.");
    }

    [Fact]
    public async Task DeveRejeitarTituloComMaisDe200Caracteres()
    {
        var result = await _validator.ValidateAsync(Valido(titulo: new string('A', 201)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Titulo" && e.ErrorMessage == "Título não pode exceder 200 caracteres.");
    }

    [Fact]
    public async Task DeveAceitarTituloComExatamente200Caracteres()
    {
        var result = await _validator.ValidateAsync(Valido(titulo: new string('A', 200)));

        Assert.True(result.IsValid);
    }

    // Descricao
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeveRejeitarDescricaoVazia(string descricao)
    {
        var result = await _validator.ValidateAsync(Valido(descricao: descricao));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Descricao" && e.ErrorMessage == "Descrição é obrigatória.");
    }

    [Fact]
    public async Task DeveRejeitarDescricaoComMaisDe5000Caracteres()
    {
        var result = await _validator.ValidateAsync(Valido(descricao: new string('x', 5001)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Descricao" && e.ErrorMessage == "Descrição não pode exceder 5000 caracteres.");
    }

    [Fact]
    public async Task DeveAceitarDescricaoComExatamente5000Caracteres()
    {
        var result = await _validator.ValidateAsync(Valido(descricao: new string('x', 5000)));

        Assert.True(result.IsValid);
    }

    // Requisitos
    [Fact]
    public async Task DeveAceitarRequisitosNulo()
    {
        var result = await _validator.ValidateAsync(Valido(requisitos: null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task DeveRejeitarRequisitosComMaisDe5000Caracteres()
    {
        var result = await _validator.ValidateAsync(Valido(requisitos: new string('r', 5001)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Requisitos" && e.ErrorMessage == "Requisitos não podem exceder 5000 caracteres.");
    }

    [Fact]
    public async Task DeveAceitarRequisitosComExatamente5000Caracteres()
    {
        var result = await _validator.ValidateAsync(Valido(requisitos: new string('r', 5000)));

        Assert.True(result.IsValid);
    }

    // Salario
    [Fact]
    public async Task DeveRejeitarSalarioNegativo()
    {
        var result = await _validator.ValidateAsync(Valido(salario: -1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Salario" && e.ErrorMessage == "Salário não pode ser negativo.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5000)]
    public async Task DeveAceitarSalarioZeroOuPositivo(decimal salario)
    {
        var result = await _validator.ValidateAsync(Valido(salario: salario));

        Assert.True(result.IsValid);
    }
}
