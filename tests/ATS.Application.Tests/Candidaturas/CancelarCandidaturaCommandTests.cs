using ATS.Application.Candidaturas.Commands.CancelarCandidatura;

public class CancelarCandidaturaCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public void DeveCriarCommandComCandidaturaIdDefinido(string candidaturaIdStr)
    {
        var candidaturaId = Guid.Parse(candidaturaIdStr);

        var command = new CancelarCandidaturaCommand(candidaturaId);

        Assert.Equal(candidaturaId, command.CandidaturaId);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public void DeveSerIgualAOutroCommandComMesmoId(string candidaturaIdStr)
    {
        var a = new CancelarCandidaturaCommand(Guid.Parse(candidaturaIdStr));
        var b = new CancelarCandidaturaCommand(Guid.Parse(candidaturaIdStr));

        Assert.Equal(a, b);
    }
}
