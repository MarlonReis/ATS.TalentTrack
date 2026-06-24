using ATS.Application.Candidaturas.Commands.ReprovarCandidatura;

public class ReprovarCandidaturaCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", null)]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Perfil insuficiente.")]
    public void DeveCriarCommandComPropriedadesDefinidas(
        string candidaturaIdStr, string? observacoes)
    {
        var candidaturaId = Guid.Parse(candidaturaIdStr);

        var command = new ReprovarCandidaturaCommand(candidaturaId, observacoes);

        Assert.Equal(candidaturaId, command.CandidaturaId);
        Assert.Equal(observacoes, command.Observacoes);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public void DeveUsarNullComoObservacoesParaoPadrao(string candidaturaIdStr)
    {
        var command = new ReprovarCandidaturaCommand(Guid.Parse(candidaturaIdStr));

        Assert.Null(command.Observacoes);
    }
}
