
using ATS.Application.Vagas.Commands.FecharVaga;

namespace ATS.Application.Tests.Vagas;

public class FecharVagaCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public void DeveCriarCommandComIdDefinido(string idStr)
    {
        var id = Guid.Parse(idStr);
        Assert.Equal(id, new FecharVagaCommand(id).Id);
    }
}
