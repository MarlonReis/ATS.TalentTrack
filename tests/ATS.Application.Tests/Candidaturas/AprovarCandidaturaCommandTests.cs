using ATS.Application.Candidaturas.Commands.AprovarCandidatura;

public class AprovarCandidaturaCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", null)]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Perfil excelente.")]
    public void DeveCriarCommandComPropriedadesDefinidas(
        string candidaturaIdStr, string? observacoes)
    {
        var candidaturaId = Guid.Parse(candidaturaIdStr);

        var command = new AprovarCandidaturaCommand(candidaturaId, observacoes);

        Assert.Equal(candidaturaId, command.CandidaturaId);
        Assert.Equal(observacoes, command.Observacoes);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "Aprovado.")]
    public void DeveSerIgualAOutroCommandComMesmosValores(
        string candidaturaIdStr, string observacoes)
    {
        var a = new AprovarCandidaturaCommand(Guid.Parse(candidaturaIdStr), observacoes);
        var b = new AprovarCandidaturaCommand(Guid.Parse(candidaturaIdStr), observacoes);

        Assert.Equal(a, b);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public void DeveUsarNullComoObservacoesParaoPadrao(string candidaturaIdStr)
    {
        var command = new AprovarCandidaturaCommand(Guid.Parse(candidaturaIdStr));

        Assert.Null(command.Observacoes);
    }
}
