using ATS.Application.Vagas.Commands.DeleteVaga;

namespace ATS.Application.Tests.Vagas;

public class DeleteVagaCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public void DeveCriarCommandComIdDefinido(string idStr)
    {
        var id = Guid.Parse(idStr);
        Assert.Equal(id, new DeleteVagaCommand(id).Id);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public void DeveSerIgualAOutroCommandComMesmoId(string idStr)
    {
        var id = Guid.Parse(idStr);
        Assert.Equal(new DeleteVagaCommand(id), new DeleteVagaCommand(id));
    }
}
