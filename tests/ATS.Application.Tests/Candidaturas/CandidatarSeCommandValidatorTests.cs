namespace ATS.Application.Tests.Candidaturas;

using ATS.Application.Candidaturas.Commands.CandidatarSe;

public class CandidatarSeCommandValidatorTests
{
    private readonly CandidatarSeCommandValidator _validator = new();

    [Fact]
    public async Task DeveAceitarComandoValido()
    {
        var result = await _validator.ValidateAsync(
            new CandidatarSeCommand(Guid.NewGuid(), Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task DeveRejeitarCandidatoIdVazio()
    {
        var result = await _validator.ValidateAsync(
            new CandidatarSeCommand(Guid.Empty, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CandidatoId" && e.ErrorMessage == "CandidatoId é obrigatório.");
    }

    [Fact]
    public async Task DeveRejeitarVagaIdVazio()
    {
        var result = await _validator.ValidateAsync(
            new CandidatarSeCommand(Guid.NewGuid(), Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "VagaId" && e.ErrorMessage == "VagaId é obrigatório.");
    }

    [Fact]
    public async Task DeveRejeitarAmbosOsIdsVazios()
    {
        var result = await _validator.ValidateAsync(
            new CandidatarSeCommand(Guid.Empty, Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }
}
