using ATS.Application.Candidatos.Commands.DeleteCandidato;

public class DeleteCandidatoCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public void DeveCriarCommandComIdDefinido(string idStr)
    {
        var id = Guid.Parse(idStr);
        var command = new DeleteCandidatoCommand(id);

        Assert.Equal(id, command.Id);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public void DeveSerIgualAOutroCommandComMesmoId(string idStr)
    {
        var id = Guid.Parse(idStr);
        Assert.Equal(new DeleteCandidatoCommand(id), new DeleteCandidatoCommand(id));
    }
}
