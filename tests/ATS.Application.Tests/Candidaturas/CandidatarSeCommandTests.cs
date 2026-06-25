

using ATS.Application.Candidaturas.Commands.CandidatarSe;

namespace ATS.Application.Tests.Candidaturas;

public class CandidatarSeCommandTests
{
    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "1fa25a64-3317-1562-c3fc-1c863f66afa6")]
    public void DeveCriarCommandComCandidatoIdEVagaIdDefinidos(
        string candidatoIdStr, string vagaIdStr)
    {
        var candidatoId = Guid.Parse(candidatoIdStr);
        var vagaId = Guid.Parse(vagaIdStr);

        var command = new CandidatarSeCommand(candidatoId, vagaId);

        Assert.Equal(candidatoId, command.CandidatoId);
        Assert.Equal(vagaId, command.VagaId);
    }

    [Theory]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                "cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveSerIgualAOutroCommandComMesmosValores(
        string candidatoIdStr, string vagaIdStr)
    {
        var a = new CandidatarSeCommand(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));
        var b = new CandidatarSeCommand(Guid.Parse(candidatoIdStr), Guid.Parse(vagaIdStr));

        Assert.Equal(a, b);
    }
}

